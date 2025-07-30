using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="Appointment"/> entities.
    /// </summary>
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="userHelper">The helper for user-related operations.</param>
        public AppointmentRepository(DataContext context, IUserHelper userHelper) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        /// <summary>
        /// Gets a single appointment by its ID, including detailed related entities.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to retrieve.</param>
        /// <returns>The <see cref="Appointment"/> with its client, mechanic, and vehicle details, or null if not found.</returns>
        public async Task<Appointment> GetByIdWithDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle.CarModel.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        /// <summary>
        /// Gets all appointments for a specific client, with an option to include completed and cancelled ones.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <param name="includeCompleted">A flag to determine whether to include appointments with 'Completed' or 'Cancelled' status.</param>
        /// <returns>A collection of the client's appointments, sorted by status and then by date.</returns>
        public async Task<IEnumerable<Appointment>> GetByClientIdAsync(string clientId, bool includeCompleted)
        {
            var query = _context.Appointments
                .Where(a => a.ClientId == clientId);

            if (!includeCompleted)

            {
                query = query.Where(a => a.Status != "Completed" && a.Status != "Cancelled");
            }

            return await query
                .Include(a => a.Vehicle.CarModel.Brand)
                .Include(a => a.Mechanic)
                .OrderBy(a =>
                    a.Status == "Pending" ? 1 :  
                    a.Status == "Completed" ? 2 :
                    a.Status == "Cancelled" ? 3 :
                    4)                           
                .ThenByDescending(a => a.Date)   
                .ToListAsync();
        }

        /// <summary>
        /// Gets a list of mechanics who are available at a specific date and time.
        /// </summary>
        /// <param name="appointmentDate">The date and time to check for availability.</param>
        /// <returns>A collection of <see cref="User"/> objects representing available mechanics.</returns>
        /// <remarks>
        /// An available mechanic is one who is on schedule and does not have another "Pending" appointment on the same day.
        /// </remarks>
        public async Task<IEnumerable<User>> GetAvailableMechanicsAsync(DateTime appointmentDate)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var timeOfDay = appointmentDate.TimeOfDay;

            var availableMechanicsQuery = _context.Users
                .Where(user => user.Schedules.Any(schedule =>
                    schedule.DayOfWeek == dayOfWeek &&
                    schedule.StartTime <= timeOfDay &&
                    schedule.EndTime > timeOfDay))

                .Where(user => !user.Appointments.Any(appointment =>
                    appointment.Date.Date == appointmentDate.Date &&
                    appointment.Status == "Pending"))

                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName);

            return await availableMechanicsQuery.ToListAsync();
        }

        /// <summary>
        /// Calculates which days in a given month are fully booked and thus unavailable for new appointments.
        /// </summary>
        /// <param name="year">The year of the month to check.</param>
        /// <param name="month">The month to check.</param>
        /// <returns>A collection of strings representing the unavailable dates in "yyyy-MM-dd" format.</returns>
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

        /// <summary>
        /// Checks if a specific mechanic is scheduled to be working at a given date and time.
        /// </summary>
        /// <param name="mechanicId">The ID of the mechanic to check.</param>
        /// <param name="appointmentDate">The date and time to check against the mechanic's schedule.</param>
        /// <returns>True if the mechanic is scheduled to work; otherwise, false.</returns>
        public async Task<bool> IsMechanicAvailableAtTimeAsync(string mechanicId, DateTime appointmentDate)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var timeOfDay = appointmentDate.TimeOfDay;

            return await _context.Schedules
                .AnyAsync(s => s.UserId == mechanicId && s.DayOfWeek == dayOfWeek && s.StartTime <= timeOfDay && s.EndTime > timeOfDay);
        }

        /// <summary>
        /// Creates a new appointment in the database and returns the created entity.
        /// </summary>
        /// <param name="appointment">The appointment entity to create.</param>
        /// <returns>The created <see cref="Appointment"/> entity with its new ID.</returns>
        public async Task<Appointment> CreateAndReturnAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
    }
}
