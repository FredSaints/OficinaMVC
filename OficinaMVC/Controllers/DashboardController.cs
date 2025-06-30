using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Dashboard;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Receptionist,Mechanic")]
    public class DashboardController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public DashboardController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            IQueryable<Data.Entities.Appointment> appointmentsQuery = _context.Appointments;
            IQueryable<Data.Entities.Repair> ongoingRepairsQuery = _context.Repairs.Where(r => r.Status == "Ongoing");

            if (User.IsInRole("Mechanic"))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.MechanicId == user.Id);
                ongoingRepairsQuery = ongoingRepairsQuery.Where(r => r.Mechanics.Any(m => m.Id == user.Id));
            }

            var todaysAppointments = await appointmentsQuery
                .Where(a => a.Date.Date == today)
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel)
                .OrderBy(a => a.Date)
                .Select(a => new Models.Dashboard.AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentTime = a.Date,
                    ClientName = a.Client.FullName,
                    VehicleInfo = $"{a.Vehicle.LicensePlate} ({a.Vehicle.CarModel.Name})",
                    MechanicName = a.Mechanic.FullName,
                    ServiceType = a.ServiceType,
                    RepairId = a.RepairId
                })
                .ToListAsync();

            var ongoingRepairs = await ongoingRepairsQuery
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .OrderBy(r => r.StartDate)
                .Select(r => new OngoingRepairViewModel
                {
                    RepairId = r.Id,
                    LicensePlate = r.Vehicle.LicensePlate,
                    VehicleDescription = r.Vehicle.CarModel.Name,
                    ClientName = r.Vehicle.Owner.FullName,
                    StartDate = r.StartDate
                })
                .ToListAsync();

            var lowStockParts = new List<LowStockPartViewModel>();
            if (User.IsInRole("Receptionist"))
            {
                lowStockParts = await _context.Parts
                    .Where(p => p.StockQuantity <= 5 && p.StockQuantity > 0)
                    .OrderBy(p => p.StockQuantity)
                    .Select(p => new LowStockPartViewModel
                    {
                        PartId = p.Id,
                        PartName = p.Name,
                        StockQuantity = p.StockQuantity
                    })
                    .Take(5)
                    .ToListAsync();
            }

            var chartData = new ChartDataViewModel();
            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                chartData.Labels.Add(day.ToString("ddd, MMM dd"));
                var count = await appointmentsQuery.CountAsync(a => a.Date.Date == day.Date);
                chartData.Data.Add(count);
            }

            var viewModel = new DashboardViewModel
            {
                AppointmentsTodayCount = todaysAppointments.Count,
                OngoingRepairsCount = ongoingRepairs.Count,
                LowStockPartsCount = await _context.Parts.CountAsync(p => p.StockQuantity <= 5),

                TodaysAppointments = todaysAppointments,
                OngoingRepairs = ongoingRepairs,
                LowStockParts = lowStockParts,
                AppointmentsChartData = chartData
            };

            return View(viewModel);
        }
    }
}