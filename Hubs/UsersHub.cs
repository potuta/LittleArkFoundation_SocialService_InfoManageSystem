using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Hubs
{
    public class UsersHub : Hub
    {
        // Thread-safe dictionary of active users
        private static readonly ConcurrentDictionary<string, string> ActiveUsers = new();

        public override async Task OnConnectedAsync()
        {
            // Grab username from claims (or fallback to connection ID)
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value
               ?? Context.User?.Identity?.Name
               ?? Context.ConnectionId;

            ActiveUsers[Context.ConnectionId] = username;

            await Clients.All.SendAsync("ActiveUsersUpdated", ActiveUsers.Values);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ActiveUsers.TryRemove(Context.ConnectionId, out _);

            await Clients.All.SendAsync("ActiveUsersUpdated", ActiveUsers.Values);

            await base.OnDisconnectedAsync(exception);
        }

        // Helper method if you want to get active users programmatically
        public static IEnumerable<string> GetActiveUsers()
        {
            return ActiveUsers.Values;
        }
    }
}
