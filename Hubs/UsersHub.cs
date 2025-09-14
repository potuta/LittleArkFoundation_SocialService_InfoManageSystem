using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Hubs
{
    public class UsersHub : Hub
    {
        // Thread-safe dictionary of active users
        private static readonly ConcurrentDictionary<string, ActiveUserInfo> ActiveUsers = new();

        public override async Task OnConnectedAsync()
        {
            // Grab username from claims (or fallback to connection ID)
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value
               ?? Context.User?.Identity?.Name
               ?? Context.ConnectionId;

            var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value); 

            ActiveUsers[Context.ConnectionId] = new ActiveUserInfo 
            { 
                UserID = userId, 
                Username = username 
            }; 

            var uniqueUsers = ActiveUsers.Values
                .GroupBy(u => u.UserID) // group by the user ID
                .Select(g => g.First()) // pick one per user
                .ToList(); 

            await Clients.All.SendAsync("ActiveUsersUpdated", uniqueUsers);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ActiveUsers.TryRemove(Context.ConnectionId, out _);

            var uniqueUsers = ActiveUsers.Values
                             .GroupBy(u => u.UserID)
                             .Select(g => g.First())
                             .ToList();

            await Clients.All.SendAsync("ActiveUsersUpdated", uniqueUsers);

            await base.OnDisconnectedAsync(exception);
        }

        // Helper method if you want to get active users programmatically
        public static IEnumerable<ActiveUserInfo> GetActiveUsers()
        {
            return ActiveUsers.Values
                              .GroupBy(u => u.UserID)
                              .Select(g => g.First());
        }
    }

    public class ActiveUserInfo { 
        public int UserID { get; set; } 
        public string Username { get; set; } 
    }
}
