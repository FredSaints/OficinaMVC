using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Models.API;

namespace OficinaMVC.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleHistoryController : ControllerBase
    {
        private readonly DataContext _context;

        public VehicleHistoryController(DataContext context)
        {
            _context = context;
        }

        // GET: api/VehicleHistory/{vehicleId}
        [HttpGet("{vehicleId:int}")]
        public async Task<IActionResult> GetHistory(int vehicleId)
        {
            var repairs = await _context.Repairs
                .Include(r => r.RepairParts)
                .ThenInclude(rp => rp.Part)
                .Include(r => r.Mechanics)
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            if (repairs == null || !repairs.Any())
            {
                return NotFound("No repair history found for this vehicle.");
            }

            var historyDto = repairs.Select(r => new RepairHistoryDto
            {
                RepairId = r.Id,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                Description = r.Description,
                TotalCost = r.TotalCost,
                Mechanics = r.Mechanics.Select(m => m.FullName).ToList(),
                PartsUsed = r.RepairParts.Select(rp => new PartUsedDto
                {
                    Name = rp.Part.Name,
                    Quantity = rp.Quantity,
                    UnitPrice = rp.UnitPrice
                }).ToList()
            }).ToList();

            return Ok(historyDto);
        }
    }
}