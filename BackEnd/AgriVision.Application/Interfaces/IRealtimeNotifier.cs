using AgriVision.Domain.Entities;

namespace AgriVision.Application.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task<bool> TryNotifyAsync(Notification notification);
    }
}
