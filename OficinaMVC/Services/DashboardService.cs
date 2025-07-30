using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Dashboard;
using System.Security.Claims;

namespace OficinaMVC.Services
{
    /// <inheritdoc cref="IDashboardService"/>
    public class DashboardService : IDashboardService
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="userHelper">The user helper service for user-related operations.</param>
        public DashboardService(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        /// <inheritdoc />
        public async Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal userPrincipal)
        {
            var today = DateTime.UtcNow.Date;
            var user = await _userHelper.GetUserByEmailAsync(userPrincipal.Identity.Name);
            if (user == null)
            {
                return new DashboardViewModel { TodaysAppointments = new(), OngoingRepairs = new(), LowStockParts = new(), AppointmentsChartData = new() };
            }

            var appointmentsQuery = _context.Appointments.AsQueryable();
            var ongoingRepairsQuery = _context.Repairs.Where(r => r.Status == "Ongoing");

            if (userPrincipal.IsInRole("Mechanic"))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.MechanicId == user.Id);
                ongoingRepairsQuery = ongoingRepairsQuery.Where(r => r.Mechanics.Any(m => m.Id == user.Id));
            }

            var todaysAppointments = await GetTodaysAppointmentsAsync(appointmentsQuery, today);
            var ongoingRepairs = await GetOngoingRepairsAsync(ongoingRepairsQuery);
            var lowStockParts = userPrincipal.IsInRole("Receptionist") ? await GetLowStockPartsAsync() : new List<LowStockPartViewModel>();
            var chartData = await GetChartDataAsync(appointmentsQuery, today);

            var viewModel = new DashboardViewModel
            {
                AppointmentsTodayCount = todaysAppointments.Count,
                OngoingRepairsCount = ongoingRepairs.Count,
                LowStockPartsCount = userPrincipal.IsInRole("Receptionist") ? await _context.Parts.CountAsync(p => p.StockQuantity <= 5) : 0,
                TodaysAppointments = todaysAppointments,
                OngoingRepairs = ongoingRepairs,
                LowStockParts = lowStockParts,
                AppointmentsChartData = chartData
            };

            return viewModel;
        }

        private async Task<List<AppointmentViewModel>> GetTodaysAppointmentsAsync(IQueryable<Data.Entities.Appointment> query, DateTime today)
        {
            return await query
                .Where(a => a.Date.Date == today &&
                            a.RepairId == null)
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel)
                .OrderBy(a => a.Date)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentTime = a.Date,
                    ClientName = a.Client.FullName,
                    VehicleInfo = $"{a.Vehicle.LicensePlate} ({a.Vehicle.CarModel.Name})",
                    MechanicName = a.Mechanic.FullName,
                    ServiceType = a.ServiceType,
                    RepairId = a.RepairId
                }).ToListAsync();
        }

        private async Task<List<OngoingRepairViewModel>> GetOngoingRepairsAsync(IQueryable<Data.Entities.Repair> query)
        {
            return await query
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .Include(r => r.Vehicle).ThenInclude(v => v.CarModel)
                .OrderBy(r => r.StartDate)
                .Select(r => new OngoingRepairViewModel
                {
                    RepairId = r.Id,
                    LicensePlate = r.Vehicle.LicensePlate,
                    VehicleDescription = r.Vehicle.CarModel.Name,
                    ClientName = r.Vehicle.Owner.FullName,
                    StartDate = r.StartDate
                }).ToListAsync();
        }

        private async Task<List<LowStockPartViewModel>> GetLowStockPartsAsync()
        {
            return await _context.Parts
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

        private async Task<ChartDataViewModel> GetChartDataAsync(IQueryable<Data.Entities.Appointment> query, DateTime today)
        {
            var sevenDaysAgo = today.AddDays(-6);

            var dailyCounts = await query
                .Where(a => a.Date.Date >= sevenDaysAgo && a.Date.Date <= today)
                .GroupBy(a => a.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            var chartData = new ChartDataViewModel();
            for (int i = 0; i < 7; i++)
            {
                var day = today.AddDays(-6 + i);
                chartData.Labels.Add(day.ToString("ddd, MMM dd"));
                chartData.Data.Add(dailyCounts.TryGetValue(day, out int count) ? count : 0);
            }
            return chartData;
        }
    }
}