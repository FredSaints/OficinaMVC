using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OficinaMVC.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="Repair"/> entities.
    /// Implements transactional logic for operations that affect multiple tables, such as inventory management.
    /// </summary>
    public class RepairRepository : IRepairRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepairRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public RepairRepository(DataContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Part> AddPartToRepairAsync(int repairId, int partId, int quantity)
        {
            IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Everything inside this block is now an atomic, retriable transaction.
                var repair = await _context.Repairs.FindAsync(repairId);
                if (repair == null) throw new InvalidOperationException("Repair not found.");

                // Use UPDLOCK to place a row-level lock, preventing other transactions from interfering.
                var part = await _context.Parts
                    .FromSqlRaw("SELECT * FROM Parts WITH (UPDLOCK) WHERE Id = {0}", partId)
                    .FirstOrDefaultAsync();

                if (part == null) throw new InvalidOperationException("Part not found.");

                if (part.StockQuantity < quantity)
                {
                    throw new InvalidOperationException($"Not enough stock for {part.Name}. Required: {quantity}, Available: {part.StockQuantity}.");
                }

                var existingRepairPart = await _context.RepairParts
                    .FirstOrDefaultAsync(rp => rp.RepairId == repairId && rp.PartId == partId);

                if (existingRepairPart != null)
                {
                    existingRepairPart.Quantity += quantity;
                }
                else
                {
                    _context.RepairParts.Add(new RepairPart
                    {
                        RepairId = repairId,
                        PartId = partId,
                        Quantity = quantity,
                        UnitPrice = part.Price
                    });
                }

                part.StockQuantity -= quantity;
                await _context.SaveChangesAsync();

                // Recalculate cost within the same transaction for consistency.
                repair.TotalCost = await _context.RepairParts
                                       .Where(rp => rp.RepairId == repairId)
                                       .SumAsync(rp => rp.Quantity * rp.UnitPrice);
                await _context.SaveChangesAsync();

                return part;
            });
        }

        /// <inheritdoc/>
        public async Task RemovePartFromRepairAsync(int repairPartId)
        {
            IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                var repairPart = await _context.RepairParts
                    .Include(rp => rp.Part)
                    .FirstOrDefaultAsync(rp => rp.Id == repairPartId);

                if (repairPart == null) throw new InvalidOperationException("Repair part entry not found.");

                var repairId = repairPart.RepairId;
                var repair = await _context.Repairs.FindAsync(repairId);

                // Lock the specific part row before updating it.
                var partToUpdate = await _context.Parts
                    .FromSqlRaw("SELECT * FROM Parts WITH (UPDLOCK) WHERE Id = {0}", repairPart.PartId)
                    .FirstOrDefaultAsync();

                if (partToUpdate != null)
                {
                    partToUpdate.StockQuantity += repairPart.Quantity;
                }

                _context.RepairParts.Remove(repairPart);
                await _context.SaveChangesAsync();

                if (repair != null)
                {
                    repair.TotalCost = await _context.RepairParts
                                           .Where(rp => rp.RepairId == repairId)
                                           .SumAsync(rp => rp.Quantity * rp.UnitPrice);
                    await _context.SaveChangesAsync();
                }
            });
        }

        /// <inheritdoc/>
        public async Task<Repair> CreateRepairFromAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.Include(a => a.Vehicle).FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) throw new InvalidOperationException("Appointment not found.");
            if (appointment.Status == "Completed") throw new InvalidOperationException("A repair has already been started for this appointment.");
            if (appointment.RepairId != null) return await GetByIdWithDetailsAsync(appointment.RepairId.Value);
            var newRepair = new Repair { StartDate = DateTime.UtcNow, Description = $"Initial repair job from appointment: {appointment.ServiceType}.", Status = "Ongoing", VehicleId = appointment.VehicleId, TotalCost = 0, AppointmentId = appointment.Id };
            var mechanic = await _context.Users.FindAsync(appointment.MechanicId);
            if (mechanic != null) newRepair.Mechanics.Add(mechanic);
            _context.Repairs.Add(newRepair);
            appointment.RepairId = newRepair.Id;
            appointment.Status = "Completed";
            await _context.SaveChangesAsync();
            return newRepair;
        }

        /// <inheritdoc/>
        public async Task UpdateRepairStatusAndNotesAsync(int repairId, string status, string description)
        {
            var repair = await _context.Repairs.FindAsync(repairId);
            if (repair == null) throw new InvalidOperationException("Repair not found.");
            repair.Description = description;
            if (repair.Status != "Completed" && status == "Completed") { repair.EndDate = DateTime.UtcNow; }
            repair.Status = status;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteRepairAndReturnPartsToStockAsync(int repairId)
        {
            var repair = await GetByIdWithDetailsAsync(repairId);
            if (repair == null) return;
            foreach (var repairPart in repair.RepairParts)
            {
                var partToUpdate = await _context.Parts.FindAsync(repairPart.PartId);
                if (partToUpdate != null) { partToUpdate.StockQuantity += repairPart.Quantity; }
            }
            _context.RepairParts.RemoveRange(repair.RepairParts);
            if (repair.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments.FindAsync(repair.AppointmentId.Value);
                if (appointment != null) { appointment.RepairId = null; appointment.Status = "Pending"; }
            }
            repair.Mechanics.Clear();
            _context.Repairs.Remove(repair);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateMechanicsForRepairAsync(int repairId, List<string> mechanicIds)
        {
            var repair = await _context.Repairs.Include(r => r.Mechanics).FirstOrDefaultAsync(r => r.Id == repairId);
            if (repair == null) return;
            repair.Mechanics.Clear();
            if (mechanicIds != null && mechanicIds.Any())
            {
                var selectedMechanics = await _context.Users.Where(u => mechanicIds.Contains(u.Id)).ToListAsync();
                foreach (var mechanic in selectedMechanics) { repair.Mechanics.Add(mechanic); }
            }
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Repair>> GetAllWithDetailsAsync()
        {
            return await _context.Repairs.Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand).Include(r => r.Vehicle).ThenInclude(v => v.Owner).OrderByDescending(r => r.StartDate).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Repair> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Repairs.Include(r => r.Vehicle).ThenInclude(v => v.Owner).Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand).Include(r => r.RepairParts).ThenInclude(rp => rp.Part).Include(r => r.Mechanics).FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<RepairPart> GetRepairPartByIdAsync(int repairPartId)
        {
            return await _context.RepairParts.AsNoTracking().FirstOrDefaultAsync(rp => rp.Id == repairPartId);
        }
    }
}