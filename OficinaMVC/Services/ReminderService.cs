using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;

namespace OficinaMVC.Services
{
    public class ReminderService : IReminderService
    {
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;

        public ReminderService(DataContext context, IMailHelper mailHelper)
        {
            _context = context;
            _mailHelper = mailHelper;
        }

        public async Task SendAppointmentReminders()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            var appointmentsForTomorrow = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Where(a => a.Date.Date == tomorrow && a.Status == "Confirmed")
                .ToListAsync();

            if (!appointmentsForTomorrow.Any())
            {
                Console.WriteLine("No appointments for tomorrow. No reminders sent.");
                return;
            }

            Console.WriteLine($"Found {appointmentsForTomorrow.Count} appointments for tomorrow. Sending reminders...");

            foreach (var appt in appointmentsForTomorrow)
            {
                var subject = $"Appointment Reminder for {appt.Date:dd-MM-yyyy}";
                var body = BuildReminderEmailBody(appt);

                _mailHelper.SendEmail(appt.Client.Email, subject, body);
            }
            Console.WriteLine("Finished sending reminders.");
        }

        private string BuildReminderEmailBody(Data.Entities.Appointment appt)
        {
            return $@"
                <h1>Appointment Reminder</h1>
                <p>Hello {appt.Client.FirstName},</p>
                <p>This is a friendly reminder of your upcoming appointment with FredAuto.</p>
                <hr>
                <h3>Details:</h3>
                <ul>
                    <li><strong>Date:</strong> {appt.Date:dddd, MMMM dd, yyyy}</li>
                    <li><strong>Time:</strong> {appt.Date:h:mm tt}</li>
                    <li><strong>Vehicle:</strong> {appt.Vehicle.CarModel.Brand.Name} {appt.Vehicle.CarModel.Name} ({appt.Vehicle.LicensePlate})</li>
                    <li><strong>Service:</strong> {appt.ServiceType}</li>
                    <li><strong>Assigned Mechanic:</strong> {appt.Mechanic.FullName}</li>
                </ul>
                <hr>
                <p>We look forward to seeing you!</p>
                <p><em>The FredAuto Team</em></p>";
        }
    }
}