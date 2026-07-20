using AgriVision.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.Infrastructure.Realtime
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly AgriVisionDbContext _context;

        public NotificationHub(AgriVisionDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(userId))
            {
                Console.WriteLine("User Id Not Found");
                Context.Abort();
                return;
            }

            var undelivered = await _context.Notifications
                .Where(n => n.UserId == userId && n.DeliveredAt == null)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var notification in undelivered)
            {
                await Clients.User(userId)
                    .SendAsync("ReceiveNotification", new
                    {
                        notification.Id,
                        notification.Title,
                        notification.Body,
                        notification.CreatedAt
                    });

                notification.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await base.OnConnectedAsync();
        }
    }
}
