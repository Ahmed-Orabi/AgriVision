using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.Infrastructure.Services
{
    public class CropRecommendationHistoryService : ICropRecommendationHistoryService
    {
        private readonly AgriVisionDbContext _context;

        public CropRecommendationHistoryService(AgriVisionDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(string userId, CropRecommendationRequestDto request, CropRecommendationResponseDto response)
        {
            // ensure we always have top 3
            var top3 = response.Predictions
                .OrderByDescending(x => x.Confidence)
                .Take(3)
                .ToList();

            if (top3.Count < 3)
                throw new Exception("AI response does not contain 3 predictions.");

            var entity = new CropRecommendationHistory
            {
                UserId = userId,

                N = request.N,
                P = request.P,
                K = request.K,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                Ph = request.Ph,
                Rainfall = request.Rainfall,

                Crop1 = top3[0].Crop,
                Confidence1 = top3[0].Confidence,

                Crop2 = top3[1].Crop,
                Confidence2 = top3[1].Confidence,

                Crop3 = top3[2].Crop,
                Confidence3 = top3[2].Confidence,

                CreatedAtUtc = DateTime.UtcNow
            };

            _context.CropRecommendationHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CropRecommendationHistoryItemDto>> GetLatestAsync(string userId, int take = 6)
        {
            return await _context.CropRecommendationHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .Select(x => new CropRecommendationHistoryItemDto
                {
                    Id = x.Id,
                    BestCrop = x.Crop1,
                    BestConfidence = x.Confidence1,
                    CreatedAtUtc = x.CreatedAtUtc,
                    N = x.N,
                    Temperature = x.Temperature,
                    Rainfall = x.Rainfall
                })
                .ToListAsync();
        }
    }
}
