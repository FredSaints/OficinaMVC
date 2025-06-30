using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class RepairRepository : IRepairRepository
    {
        private readonly DataContext _context;

        public RepairRepository(DataContext context)
        {
            _context = context;
        }

        public async Task UpdateMechanicsForRepairAsync(int repairId, List<string> mechanicIds)
        {
            var repair = await _context.Repairs
                .Include(r => r.Mechanics)
                .FirstOrDefaultAsync(r => r.Id == repairId);

            if (repair == null)
            {
                return;
            }

            repair.Mechanics.Clear();

            if (mechanicIds != null && mechanicIds.Any())
            {
                var selectedMechanics = await _context.Users
                    .Where(u => mechanicIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var mechanic in selectedMechanics)
                {
                    repair.Mechanics.Add(mechanic);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Repair> CreateRepairFromAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) throw new Exception("Appointment not found.");

            if (appointment.Status == "Completed") throw new Exception("A repair has already been started for this appointment.");

            if (appointment.RepairId != null)
            {
                return await GetByIdWithDetailsAsync(appointment.RepairId.Value);
            }

            var newRepair = new Repair
            {
                StartDate = DateTime.Now,
                Description = $"Initial repair job from appointment: {appointment.ServiceType}.",
                Status = "Ongoing",
                VehicleId = appointment.VehicleId,
                TotalCost = 0,
                AppointmentId = appointment.Id
            };

            var mechanic = await _context.Users.FindAsync(appointment.MechanicId);
            if (mechanic != null) newRepair.Mechanics.Add(mechanic);

            _context.Repairs.Add(newRepair);
            await _context.SaveChangesAsync();

            appointment.RepairId = newRepair.Id;
            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            return newRepair;
        }

        public async Task AddPartToRepairAsync(int repairId, int partId, int quantity)
        {
            var repair = await _context.Repairs.FindAsync(repairId);
            var part = await _context.Parts.FindAsync(partId);

            if (repair == null || part == null) throw new Exception("Repair or Part not found.");
            if (part.StockQuantity < quantity) throw new Exception($"Not enough stock for {part.Name}. Required: {quantity}, Available: {part.StockQuantity}.");

            var existingRepairPart = await _context.RepairParts
                .FirstOrDefaultAsync(rp => rp.RepairId == repairId && rp.PartId == partId);

            if (existingRepairPart != null)
            {
                existingRepairPart.Quantity += quantity;
            }
            else
            {
                var newRepairPart = new RepairPart
                {
                    RepairId = repairId,
                    PartId = partId,
                    Quantity = quantity,
                    UnitPrice = part.Price
                };
                _context.RepairParts.Add(newRepairPart);
            }

            part.StockQuantity -= quantity;
            await _context.SaveChangesAsync();
            await RecalculateRepairTotalCostAsync(repairId);
        }

        public async Task RemovePartFromRepairAsync(int repairPartId)
        {
            var repairPart = await _context.RepairParts
                .Include(rp => rp.Part)
                .FirstOrDefaultAsync(rp => rp.Id == repairPartId);

            if (repairPart == null) throw new Exception("Repair part entry not found.");

            repairPart.Part.StockQuantity += repairPart.Quantity;
            _context.RepairParts.Remove(repairPart);

            var repairId = repairPart.RepairId;
            await _context.SaveChangesAsync();
            await RecalculateRepairTotalCostAsync(repairId);
        }

        public async Task UpdateRepairStatusAndNotesAsync(int repairId, string status, string description)
        {
            var repair = await _context.Repairs.FindAsync(repairId);
            if (repair == null) throw new Exception("Repair not found.");

            repair.Description = description;
            if (repair.Status != "Completed" && status == "Completed")
            {
                repair.EndDate = DateTime.Now;
            }
            repair.Status = status;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRepairAndReturnPartsToStockAsync(int repairId)
        {
            // Use the updated method that now includes Mechanics
            var repair = await GetByIdWithDetailsAsync(repairId);
            if (repair == null)
            {
                return;
            }

            // 1. Return parts to stock
            foreach (var repairPart in repair.RepairParts)
            {
                var partToUpdate = await _context.Parts.FindAsync(repairPart.PartId);
                if (partToUpdate != null)
                {
                    partToUpdate.StockQuantity += repairPart.Quantity;
                }
            }

            // 2. Explicitly remove the RepairPart child records
            _context.RepairParts.RemoveRange(repair.RepairParts);

            // 3. Unlink the Appointment
            if (repair.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments.FindAsync(repair.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.RepairId = null;
                    appointment.Status = "Pending";
                }
            }

            // 4. Clear the many-to-many relationship with Mechanics
            repair.Mechanics.Clear();

            // 5. Now remove the parent Repair record
            _context.Repairs.Remove(repair);

            // 6. Save all changes in one transaction
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Repair>> GetAllWithDetailsAsync()
        {
            return await _context.Repairs
                .Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<Repair> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Repairs
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(r => r.RepairParts).ThenInclude(rp => rp.Part)
                .Include(r => r.Mechanics)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        private async Task RecalculateRepairTotalCostAsync(int repairId)
        {
            var repair = await _context.Repairs
                .Include(r => r.RepairParts)
                .FirstOrDefaultAsync(r => r.Id == repairId);

            if (repair != null)
            {
                repair.TotalCost = repair.RepairParts.Sum(rp => rp.Quantity * rp.UnitPrice);
                await _context.SaveChangesAsync();
            }
        }
    }
}