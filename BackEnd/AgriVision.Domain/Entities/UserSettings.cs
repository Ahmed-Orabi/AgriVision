using System;

namespace AgriVision.Domain.Entities
{
    public class UserSettings
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;
        public bool NotificationsEnabled { get; set; } = true;
        public bool RobotAutoMode { get; set; } = true;
        public bool RobotFirmwareUpdates { get; set; } = true;
        public bool RobotSafetyLock { get; set; }
        public bool RobotEmergencyAlerts { get; set; } = true;
        public string MaintenanceWindow { get; set; } = "Saturday 6:00 PM";
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? AddressLine { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
