using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;

namespace OficinaMVC.Services
{
    public class RepairService : IRepairService
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMailHelper _mailHelper;
        private readonly IViewRendererService _viewRenderer;
        private readonly DataContext _context;

        public RepairService(
            IRepairRepository repairRepository,
            IHubContext<NotificationHub, INotificationClient> notificationHub,
            IMailHelper mailHelper,
            IViewRendererService viewRenderer,
            DataContext context)
        {
            _repairRepository = repairRepository;
            _notificationHub = notificationHub;
            _mailHelper = mailHelper;
            _viewRenderer = viewRenderer;
            _context = context;
        }

        public async Task<IEnumerable<Repair>> GetFilteredRepairsAsync(string status, string clientName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Repairs
                .Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .AsQueryable();

            var effectiveStatus = status ?? "Ongoing";
            if (effectiveStatus != "All")
            {
                query = query.Where(r => r.Status == effectiveStatus);
            }
            if (!string.IsNullOrEmpty(clientName))
            {
                query = query.Where(r => (r.Vehicle.Owner.FirstName + " " + r.Vehicle.Owner.LastName).Contains(clientName));
            }
            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(r => r.StartDate.Date <= endDate.Value.Date);
            }

            return await query.OrderByDescending(r => r.StartDate).ToListAsync();
        }

        public async Task<Repair> CompleteRepairAsync(int repairId)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(repairId);
            if (repair == null || repair.Status != "Ongoing")
            {
                return null;
            }

            repair.Status = "Completed";
            repair.EndDate = DateTime.Now;
            await _context.SaveChangesAsync();

            await SendCompletionNotificationsAsync(repair);

            return repair;
        }

        private async Task SendCompletionNotificationsAsync(Repair repair)
        {
            var clientMessage = $"Good news! The repair on your vehicle ({repair.Vehicle.LicensePlate}) is complete.";
            var clientUrl = $"/Vehicle/History/{repair.VehicleId}";
            await _notificationHub.Clients.User(repair.Vehicle.OwnerId).ReceiveNotification(clientMessage, clientUrl, "bi-check-circle-fill text-success");

            var receptionistMessage = $"Repair #{repair.Id} for {repair.Vehicle.LicensePlate} is complete. An invoice can now be generated.";
            var receptionistUrl = $"/Invoices/GenerateFromRepair?repairId={repair.Id}";
            await _notificationHub.Clients.Group("Receptionist").ReceiveNotification(receptionistMessage, receptionistUrl, "bi-receipt text-info");

            try
            {
                var emailViewModel = new Models.Email.RepairCompleteEmailViewModel
                {
                    ClientFirstName = repair.Vehicle.Owner.FirstName,
                    VehicleDescription = $"{repair.Vehicle.CarModel.Brand.Name} {repair.Vehicle.CarModel.Name}",
                    LicensePlate = repair.Vehicle.LicensePlate,
                    RepairId = repair.Id,
                    CompletionDate = repair.EndDate?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "N/A"
                };
                var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/RepairCompleteEmail.cshtml", emailViewModel);
                _mailHelper.SendEmail(repair.Vehicle.Owner.Email, "Your Repair is Complete - FredAuto Workshop", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending completion email: {ex.Message}");
            }
        }
    }
}