using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;

namespace OficinaMVC.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public AppointmentRepository(DataContext context, IUserHelper userHelper) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<Appointment> GetByIdWithDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle.CarModel.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task<IEnumerable<Appointment>> GetByClientIdAsync(string clientId, bool includeCompleted)
        {
            var query = _context.Appointments.Where(a => a.ClientId == clientId);
            if (!includeCompleted)
            {
                query = query.Where(a => a.Status != "Completed");
            }
            return await query
                .Include(a => a.Vehicle.CarModel.Brand)
                .Include(a => a.Mechanic)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAvailableMechanicsAsync(DateTime appointmentDate)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var timeOfDay = appointmentDate.TimeOfDay;

            // A single, comprehensive query to find available mechanics.
            var availableMechanicsQuery = _context.Users
                // 1. Filter for users who have a valid schedule for the given day and time.
                //    This is the most restrictive filter, so we apply it first.
                .Where(user => user.Schedules.Any(schedule =>
                    schedule.DayOfWeek == dayOfWeek &&
                    schedule.StartTime <= timeOfDay &&
                    schedule.EndTime > timeOfDay))

                // 2. From that group, filter out any who have a conflicting appointment.
                //    A conflicting appointment is one on the same day with a "Pending" status.
                .Where(user => !user.Appointments.Any(appointment =>
                    appointment.Date.Date == appointmentDate.Date &&
                    appointment.Status == "Pending"))

                // 3. Order the final results by name. These are mappable columns.
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName);

            // 4. Execute the single, optimized query against the database.
            return await availableMechanicsQuery.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUnavailableDaysAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var allDaysInMonth = Enumerable.Range(1, endDate.Day).Select(day => new DateTime(year, month, day)).ToList();

            var mechanicCountByDay = await _context.Schedules
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Select(s => s.UserId).Distinct().Count() })
                .ToDictionaryAsync(k => k.Day, v => v.Count);

            var appointmentsInMonth = await _context.Appointments
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.Status == "Pending")
                .GroupBy(a => a.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            var unavailableDays = new List<string>();
            foreach (var day in allDaysInMonth)
            {
                if (!mechanicCountByDay.ContainsKey(day.DayOfWeek))
                {
                    unavailableDays.Add(day.ToString("yyyy-MM-dd"));
                    continue;
                }
                int mechanicCapacity = mechanicCountByDay[day.DayOfWeek];
                int bookedCount = appointmentsInMonth.TryGetValue(day.Date, out int apptCount) ? apptCount : 0;
                if (bookedCount >= mechanicCapacity)
                {
                    unavailableDays.Add(day.ToString("yyyy-MM-dd"));
                }
            }
            return unavailableDays;
        }

        public async Task<bool> IsMechanicAvailableAtTimeAsync(string mechanicId, DateTime appointmentDate)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var timeOfDay = appointmentDate.TimeOfDay;

            return await _context.Schedules
                .AnyAsync(s => s.UserId == mechanicId && s.DayOfWeek == dayOfWeek && s.StartTime <= timeOfDay && s.EndTime > timeOfDay);
        }

        public async Task<Appointment> CreateAndReturnAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
    }
}
