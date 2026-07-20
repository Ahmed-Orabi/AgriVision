using System;

namespace AgriVision.Application.DTOs.Users
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty;
        public UserSettingsDto Settings { get; set; } = new();
    }
}
