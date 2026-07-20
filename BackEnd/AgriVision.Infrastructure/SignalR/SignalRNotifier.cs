using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace AgriVision.Infrastructure.Realtime
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRNotifier(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task<bool> TryNotifyAsync(Notification notification)
        {
            try
            {
                await _hub.Clients.User(notification.UserId)
                    .SendAsync("ReceiveNotification", new
                    {
                        notification.Id,
                        notification.Title,
                        notification.Body,
                        notification.CreatedAt
                    });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
