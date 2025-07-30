using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Hubs
{
    /// <summary>
    /// SignalR hub for managing real-time notifications and group membership based on user roles.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User.Identity.Name;
            var roles = Context.User.Claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToList();

            Console.WriteLine($"--> SignalR User Connected: ID='{userId}', Name='{userName}', Roles='{string.Join(", ", roles)}'");

            // --- Add user to role-based groups ---

            if (Context.User.IsInRole("Admin"))
            {
                // Admins get alerts for everything receptionists do
                await Groups.AddToGroupAsync(Context.ConnectionId, "Receptionist");
            }
            if (Context.User.IsInRole("Receptionist"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Receptionist");
            }
            if (Context.User.IsInRole("Mechanic"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Mechanics");
            }
            if (Context.User.IsInRole("Client"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Clients");
            }

            await base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity.Name;

            if (Context.User.IsInRole("Admin"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Receptionist");
            }
            if (Context.User.IsInRole("Receptionist"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Receptionist");
            }
            if (Context.User.IsInRole("Mechanic"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Mechanics");
            }
            if (Context.User.IsInRole("Client"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Clients");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}