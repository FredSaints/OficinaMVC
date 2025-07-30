using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace OficinaMVC.Hubs
{
    /// <summary>
    /// Provides a custom user ID provider for SignalR based on the user's name identifier claim.
    /// </summary>
    public class NameUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// Gets the user ID from the SignalR connection context using the NameIdentifier claim.
        /// </summary>
        /// <param name="connection">The SignalR hub connection context.</param>
        /// <returns>The user ID as a string, or null if not found.</returns>
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}