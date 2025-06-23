using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Models.Dashboard;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Receptionist,Mechanic")]
    public class DashboardController : Controller
    {
        private readonly DataContext _context;

        public DashboardController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (startOfWeek > today) startOfWeek = startOfWeek.AddDays(-7);
            var endOfWeek = startOfWeek.AddDays(6);

            var ongoingRepairsCount = await _context.Repairs.CountAsync(r => r.Status == "Ongoing");
            var appointmentsTodayCount = await _context.Appointments.CountAsync(a => a.Date.Date == today);
            var lowStockPartsCount = await _context.Parts.CountAsync(p => p.StockQuantity <= 5);


            var todaysAppointments = await _context.Appointments
                .Where(a => a.Date.Date == today)
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .OrderBy(a => a.Date)
                .Select(a => new Models.Dashboard.AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentTime = a.Date,
                    ClientName = a.Client.FullName,
                    VehicleInfo = $"{a.Vehicle.LicensePlate} ({a.Vehicle.CarModel.Name})",
                    MechanicName = a.Mechanic.FullName,
                    ServiceType = a.ServiceType
                })
                .ToListAsync();

            var chartData = new ChartDataViewModel();
            for (int i = 0; i < 7; i++)
            {
                var day = today.AddDays(-i);
                chartData.Labels.Insert(0, day.ToString("ddd, MMM dd"));
                var count = await _context.Appointments.CountAsync(a => a.Date.Date == day.Date);
                chartData.Data.Insert(0, count);
            }


            var viewModel = new DashboardViewModel
            {
                OngoingRepairsCount = ongoingRepairsCount,
                AppointmentsTodayCount = appointmentsTodayCount,
                LowStockPartsCount = lowStockPartsCount,
                TodaysAppointments = todaysAppointments,
                AppointmentsChartData = chartData
            };

            return View(viewModel);
        }
    }
}