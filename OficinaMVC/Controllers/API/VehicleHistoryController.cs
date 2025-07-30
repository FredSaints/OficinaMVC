using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Models.API;
using System.Security.Claims;

namespace OficinaMVC.Controllers.API
{
    /// <summary>
    /// API controller for retrieving the repair history of a specific vehicle.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},Identity.Application")]
    public class VehicleHistoryController : ControllerBase
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleHistoryController"/> class.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        public VehicleHistoryController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the repair history for a specific vehicle, if the user is authorized.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to retrieve history for.</param>
        /// <returns>A list of repair history DTOs, or an error if unauthorized or not found.</returns>
        /// <summary>
        /// Gets the repair history for a specific vehicle, if the user is authorized.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to retrieve history for.</param>
        /// <returns>A list of repair history DTOs, or an error if unauthorized or not found.</returns>
        // GET: api/VehicleHistory/{vehicleId}
        [HttpGet("{vehicleId:int}")]
        public async Task<IActionResult> GetHistory(int vehicleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID could not be determined from token.");
            }

            var vehicle = await _context.Vehicles.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found.");
            }

            bool isOwner = vehicle.OwnerId == userId;
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Receptionist") || User.IsInRole("Mechanic");

            if (!isOwner && !isStaff)
            {
                return Forbid();
            }

            var repairs = await _context.Repairs
                .Where(r => r.VehicleId == vehicleId)
                .Include(r => r.RepairParts)
                .ThenInclude(rp => rp.Part)
                .Include(r => r.Mechanics)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            if (!repairs.Any())
            {
                return Ok(new List<RepairHistoryDto>());
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