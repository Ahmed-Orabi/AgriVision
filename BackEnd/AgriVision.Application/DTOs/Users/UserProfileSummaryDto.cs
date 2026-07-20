using AgriVision.Application.DTOs.Subscriptions;

namespace AgriVision.Application.DTOs.Users
{
    public class UserProfileSummaryDto
    {
        public UserProfileDto Profile { get; set; } = new();
        public UserStatsDto Stats { get; set; } = new();
        public SubscriptionStatusDto Subscription { get; set; } = new();
    }
}
