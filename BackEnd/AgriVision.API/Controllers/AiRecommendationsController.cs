using System.Security.Claims;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using AgriVision.Application.Subscriptions;
using AgriVision.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [Route("api/v1/ai")]
    public class AiRecommendationsController : ControllerBase
    {
        private readonly IAiCropRecommendationService _cropService;
        private readonly ICropRecommendationHistoryService _cropHistoryService;
        private readonly AgriVisionDbContext _context;

        public AiRecommendationsController(
            IAiCropRecommendationService cropService,
            ICropRecommendationHistoryService cropHistoryService,
            AgriVisionDbContext context)
        {
            _cropService = cropService;
            _cropHistoryService = cropHistoryService;
            _context = context;
        }

        // =========================
        // Crop Recommendation
        // =========================

        [Authorize]
        [HttpPost("crop-recommendation")]
        public async Task<IActionResult> CropRecommendation(
            [FromBody] CropRecommendationRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            var limitCheck = await CheckMonthlyLimitAsync(userId, "crop");
            if (!string.IsNullOrEmpty(limitCheck))
                return StatusCode(StatusCodes.Status403Forbidden, limitCheck);

            var result = await _cropService.RecommendAsync(request);
            await _cropHistoryService.SaveAsync(userId, request, result);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("crop-recommendation/history")]
        public async Task<IActionResult> CropRecommendationHistory(
            [FromQuery] int take = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            var items = await _cropHistoryService.GetLatestAsync(userId, take);
            return Ok(items);
        }

        // =========================
        // Fertilizer Recommendation
        // =========================

        [Authorize]
        [HttpPost("fertilizer-recommendation")]
        public async Task<IActionResult> FertilizerRecommendation(
            [FromBody] FertilizerRecommendationRequestDto request,
            [FromServices] IFertilizerRecommendationService fertilizerService,
            [FromServices] IFertilizerRecommendationHistoryService fertilizerHistoryService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            var limitCheck = await CheckMonthlyLimitAsync(userId, "fertilizer");
            if (!string.IsNullOrEmpty(limitCheck))
                return StatusCode(StatusCodes.Status403Forbidden, limitCheck);

            var result = await fertilizerService.RecommendAsync(request);
            await fertilizerHistoryService.SaveAsync(userId, request, result);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("fertilizer-recommendation/history")]
        public async Task<IActionResult> FertilizerRecommendationHistory(
            [FromServices] IFertilizerRecommendationHistoryService fertilizerHistoryService,
            [FromQuery] int take = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId not found in token.");

            var items = await fertilizerHistoryService.GetLatestAsync(userId, take);
            return Ok(items);
        }

        private static (DateTime start, DateTime end) GetCurrentMonthRangeUtc()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            return (start, end);
        }

        private async Task<string?> CheckMonthlyLimitAsync(string userId, string feature)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return "User not found.";

            var limits = SubscriptionPlanRules.GetLimits(user.SubscriptionPlan);
            var (start, end) = GetCurrentMonthRangeUtc();

            if (feature == "crop")
            {
                if (limits.CropMonthlyLimit <= 0) return null;
                var used = await _context.CropRecommendationHistories
                    .CountAsync(x => x.UserId == userId && x.CreatedAtUtc >= start && x.CreatedAtUtc < end);
                if (used >= limits.CropMonthlyLimit)
                    return "Monthly Crop Recommendation limit reached for your subscription plan.";
            }

            if (feature == "fertilizer")
            {
                if (limits.FertilizerMonthlyLimit <= 0) return null;
                var used = await _context.FertilizerRecommendationHistories
                    .CountAsync(x => x.UserId == userId && x.CreatedAtUtc >= start && x.CreatedAtUtc < end);
                if (used >= limits.FertilizerMonthlyLimit)
                    return "Monthly Fertilizer Recommendation limit reached for your subscription plan.";
            }

            return null;
        }
    }
}
