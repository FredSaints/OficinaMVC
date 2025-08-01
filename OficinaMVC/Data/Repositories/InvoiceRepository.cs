﻿using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="Invoice"/> entities.
    /// </summary>
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public InvoiceRepository(DataContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Invoice> GetByIdAsync(int invoiceId)
        {
            return await _context.Invoices.FindAsync(invoiceId);
        }

        /// <inheritdoc/>
        public async Task<Invoice> GenerateInvoiceForRepairAsync(int repairId)
        {
            var existingInvoice = await GetByRepairIdAsync(repairId);
            if (existingInvoice != null) return existingInvoice;

            var repair = await _context.Repairs
                .Include(r => r.RepairParts).ThenInclude(rp => rp.Part)
                .FirstOrDefaultAsync(r => r.Id == repairId);

            if (repair == null) throw new Exception("Repair not found.");
            if (repair.Status != "Completed") throw new Exception("Cannot generate an invoice for a repair that is not completed.");

            decimal subtotal = repair.TotalCost;
            decimal taxRate = 0.23m;
            decimal taxAmount = subtotal * taxRate;
            decimal totalAmount = subtotal + taxAmount;

            var newInvoice = new Invoice
            {
                RepairId = repair.Id,
                InvoiceDate = DateTime.UtcNow,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                Status = "Unpaid"
            };

            foreach (var repairPart in repair.RepairParts)
            {
                newInvoice.InvoiceItems.Add(new InvoiceItem
                {
                    Description = repairPart.Part.Name,
                    Quantity = repairPart.Quantity,
                    UnitPrice = repairPart.UnitPrice
                });
            }

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();

            return newInvoice;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Invoice>> GetAllWithDetailsAsync(bool showAll = false)
        {
            var query = _context.Invoices
                .Include(i => i.Repair)
                .ThenInclude(r => r.Vehicle)
                .ThenInclude(v => v.Owner)
                .AsQueryable();

            if (!showAll)
            {
                query = query.Where(i => i.Status == "Unpaid");
            }

            return await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Invoice> GetByIdWithDetailsAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Repair.Vehicle.Owner)
                .Include(i => i.Repair.Vehicle.CarModel.Brand)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        /// <inheritdoc/>
        public async Task<Invoice> GetByRepairIdAsync(int repairId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.RepairId == repairId);
        }

        /// <inheritdoc/>
        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }
    }
}