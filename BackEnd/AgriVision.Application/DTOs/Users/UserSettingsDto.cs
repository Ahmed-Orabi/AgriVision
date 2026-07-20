namespace AgriVision.Application.DTOs.Users
{
    public class UserSettingsDto
    {
        public bool NotificationsEnabled { get; set; }
        public bool RobotAutoMode { get; set; }
        public bool RobotFirmwareUpdates { get; set; }
        public bool RobotSafetyLock { get; set; }
        public bool RobotEmergencyAlerts { get; set; }
        public string MaintenanceWindow { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? AddressLine { get; set; }
    }
}
