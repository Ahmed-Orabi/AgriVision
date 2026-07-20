using System;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Infrastructure.Data;

namespace AgriVision.Infrastructure.Services
{
    public class NotificationService
    {
        private readonly AgriVisionDbContext _context;
        private readonly IRealtimeNotifier _notifier;

        public NotificationService(AgriVisionDbContext context, IRealtimeNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        public async Task CreateAsync(string userId, string title, string body)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var delivered = await _notifier.TryNotifyAsync(notification);
            if (delivered)
            {
                notification.DeliveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
