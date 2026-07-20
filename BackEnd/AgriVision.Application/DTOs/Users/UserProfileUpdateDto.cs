namespace AgriVision.Application.DTOs.Users
{
    public class UserProfileUpdateDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserSettingsDto? Settings { get; set; }
    }
}
