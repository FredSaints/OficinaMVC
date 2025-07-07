using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
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
                Console.WriteLine($"--> User '{userName}' (Admin) added to group 'Receptionist'.");
            }
            if (Context.User.IsInRole("Receptionist"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Receptionist");
                Console.WriteLine($"--> User '{userName}' added to group 'Receptionist'.");
            }
            if (Context.User.IsInRole("Mechanic"))
            {
                // **** THIS IS THE NEW PART YOU CORRECTLY IDENTIFIED ****
                await Groups.AddToGroupAsync(Context.ConnectionId, "Mechanics");
                Console.WriteLine($"--> User '{userName}' added to group 'Mechanics'.");
            }
            if (Context.User.IsInRole("Client"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Clients");
                Console.WriteLine($"--> User '{userName}' added to group 'Clients'.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity.Name;
            Console.WriteLine($"--> SignalR User Disconnected: Name='{userName}'");

            // --- Remove user from their groups upon disconnection ---
            // This is good practice to keep the groups clean
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