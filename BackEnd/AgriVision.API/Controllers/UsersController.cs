using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgriVision.Application.DTOs.Subscriptions;
using AgriVision.Application.DTOs.Users;
using AgriVision.Application.Subscriptions;
using AgriVision.Domain.Entities;
using AgriVision.Domain.Enums;
using AgriVision.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AgriVisionDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, AgriVisionDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            var settings = await GetOrCreateSettingsAsync(userId);
            var stats = await BuildStatsAsync(userId);
            var subscription = await BuildSubscriptionAsync(userId, user.SubscriptionPlan, stats);

            var profile = MapProfile(user, settings);
            var response = new UserProfileSummaryDto
            {
                Profile = profile,
                Stats = stats,
                Subscription = subscription
            };

            return Ok(response);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UserProfileUpdateDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
                if (!emailResult.Succeeded)
                    return BadRequest(emailResult.Errors);

                var userNameResult = await _userManager.SetUserNameAsync(user, dto.Email);
                if (!userNameResult.Succeeded)
                    return BadRequest(userNameResult.Errors);
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            var settings = await GetOrCreateSettingsAsync(userId);
            if (dto.Settings != null)
            {
                settings.NotificationsEnabled = dto.Settings.NotificationsEnabled;
                settings.RobotAutoMode = dto.Settings.RobotAutoMode;
                settings.RobotFirmwareUpdates = dto.Settings.RobotFirmwareUpdates;
                settings.RobotSafetyLock = dto.Settings.RobotSafetyLock;
                settings.RobotEmergencyAlerts = dto.Settings.RobotEmergencyAlerts;
                settings.MaintenanceWindow = string.IsNullOrWhiteSpace(dto.Settings.MaintenanceWindow)
                    ? settings.MaintenanceWindow
                    : dto.Settings.MaintenanceWindow;
                settings.Country = string.IsNullOrWhiteSpace(dto.Settings.Country)
                    ? settings.Country
                    : dto.Settings.Country;
                settings.City = string.IsNullOrWhiteSpace(dto.Settings.City)
                    ? settings.City
                    : dto.Settings.City;
                settings.AddressLine = string.IsNullOrWhiteSpace(dto.Settings.AddressLine)
                    ? settings.AddressLine
                    : dto.Settings.AddressLine;
                settings.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var stats = await BuildStatsAsync(userId);
            var subscription = await BuildSubscriptionAsync(userId, user.SubscriptionPlan, stats);
            var profile = MapProfile(user, settings);

            return Ok(new UserProfileSummaryDto
            {
                Profile = profile,
                Stats = stats,
                Subscription = subscription
            });
        }

        private async Task<UserSettings> GetOrCreateSettingsAsync(string userId)
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId);
            if (settings != null) return settings;

            settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        private static UserProfileDto MapProfile(ApplicationUser user, UserSettings settings)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                LastLoginAtUtc = user.LastLoginAtUtc,
                SubscriptionPlan = user.SubscriptionPlan.ToString(),
                Settings = new UserSettingsDto
                {
                    NotificationsEnabled = settings.NotificationsEnabled,
                    RobotAutoMode = settings.RobotAutoMode,
                    RobotFirmwareUpdates = settings.RobotFirmwareUpdates,
                    RobotSafetyLock = settings.RobotSafetyLock,
                    RobotEmergencyAlerts = settings.RobotEmergencyAlerts,
                    MaintenanceWindow = settings.MaintenanceWindow,
                    Country = settings.Country,
                    City = settings.City,
                    AddressLine = settings.AddressLine
                }
            };
        }

        private async Task<UserStatsDto> BuildStatsAsync(string userId)
        {
            var farmsCount = await _context.Farms.CountAsync(f => f.OwnerId == userId);
            var sensorsCount = await _context.Sensors.CountAsync(s => s.Farm.OwnerId == userId);
            var (cropCount, fertCount, _) = await GetMonthlyAiUsageAsync(userId);

            return new UserStatsDto
            {
                FarmsCount = farmsCount,
                SensorsCount = sensorsCount,
                RobotsCount = 0,
                AiRequestsCount = cropCount + fertCount
            };
        }

        private async Task<SubscriptionStatusDto> BuildSubscriptionAsync(
            string userId,
            SubscriptionPlan userPlan,
            UserStatsDto stats)
        {
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            var plan = subscription?.Plan ?? userPlan;
            var lookup = await _context.SubscriptionPlanLookups
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Plan == plan);

            var limits = SubscriptionPlanRules.GetLimits(plan);
            var (cropUsed, fertUsed, diseaseUsed) = await GetMonthlyAiUsageAsync(userId);
            var (sensorLimit, _) = GetPlanLimits(plan);
            var aiLimit = SubscriptionPlanRules.GetTotalAiLimit(limits);
            var aiUsed = cropUsed + fertUsed + diseaseUsed;
            var isActive = subscription?.IsActive ?? true;
            var startDate = isActive ? subscription?.StartDate : null;
            var endDate = isActive ? subscription?.EndDate : null;
            var currentPeriodEnd = isActive ? (subscription?.CurrentPeriodEnd ?? subscription?.EndDate) : null;

            return new SubscriptionStatusDto
            {
                Plan = plan.ToString(),
                Name = lookup?.Name ?? plan.ToString(),
                Price = subscription?.Price ?? lookup?.Price ?? 0m,
                StartDate = startDate,
                EndDate = endDate,
                CurrentPeriodEnd = currentPeriodEnd,
                IsActive = isActive,
                FarmsUsed = stats.FarmsCount,
                FarmsLimit = limits.FarmLimit,
                SensorsUsed = stats.SensorsCount,
                SensorsLimit = sensorLimit,
                AiRequestsUsed = aiUsed,
                AiRequestsLimit = aiLimit,
                CropRequestsUsed = cropUsed,
                CropRequestsLimit = limits.CropMonthlyLimit,
                FertilizerRequestsUsed = fertUsed,
                FertilizerRequestsLimit = limits.FertilizerMonthlyLimit,
                DiseaseRequestsUsed = diseaseUsed,
                DiseaseRequestsLimit = limits.DiseaseMonthlyLimit,
                DigitalTwinEnabled = limits.DigitalTwinEnabled
            };
        }

        private static (int sensors, int aiRequests) GetPlanLimits(SubscriptionPlan plan)
        {
            return plan switch
            {   
                SubscriptionPlan.Free => (5, 20),
                SubscriptionPlan.Basic => (25, 200),
                SubscriptionPlan.Advanced => (100, 1000),
                _ => (25, 200)
            };
        }

        private static (DateTime start, DateTime end) GetCurrentMonthRangeUtc()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            return (start, end);
        }

        private async Task<(int cropUsed, int fertUsed, int diseaseUsed)> GetMonthlyAiUsageAsync(string userId)
        {
            var (start, end) = GetCurrentMonthRangeUtc();
            var cropUsed = await _context.CropRecommendationHistories
                .CountAsync(x => x.UserId == userId && x.CreatedAtUtc >= start && x.CreatedAtUtc < end);
            var fertUsed = await _context.FertilizerRecommendationHistories
                .CountAsync(x => x.UserId == userId && x.CreatedAtUtc >= start && x.CreatedAtUtc < end);

            return (cropUsed, fertUsed, 0);
        }
    }
}
