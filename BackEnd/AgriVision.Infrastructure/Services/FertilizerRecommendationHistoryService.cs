using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace AgriVision.Infrastructure.Services
{
    public class FertilizerRecommendationHistoryService
    : IFertilizerRecommendationHistoryService
    {
        private readonly AgriVisionDbContext _context;

        public FertilizerRecommendationHistoryService(AgriVisionDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(
            string userId,
            FertilizerRecommendationRequestDto request,
            FertilizerRecommendationResponseDto response)
        {
            var top = response.Predictions.Take(3).ToList();

            var entity = new FertilizerRecommendationHistory
            {
                UserId = userId,

                Temperature = request.Temperature,
                Moisture = request.Moisture,
                Rainfall = request.Rainfall,
                PH = request.PH,
                Nitrogen = request.Nitrogen,
                Phosphorous = request.Phosphorous,
                Potassium = request.Potassium,
                Carbon = request.Carbon,
                Soil = request.Soil,
                Crop = request.Crop,

                Fertilizer1 = top.ElementAtOrDefault(0)?.Fertilizer ?? "",
                Confidence1 = top.ElementAtOrDefault(0)?.Confidence ?? 0,

                Fertilizer2 = top.ElementAtOrDefault(1)?.Fertilizer ?? "",
                Confidence2 = top.ElementAtOrDefault(1)?.Confidence ?? 0,

                Fertilizer3 = top.ElementAtOrDefault(2)?.Fertilizer ?? "",
                Confidence3 = top.ElementAtOrDefault(2)?.Confidence ?? 0,
            };

            _context.FertilizerRecommendationHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FertilizerRecommendationHistoryItemDto>> GetLatestAsync(
            string userId,
            int take)
        {
            return await _context.FertilizerRecommendationHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .Select(x => new FertilizerRecommendationHistoryItemDto
                {
                    CreatedAtUtc = x.CreatedAtUtc,

                    Fertilizer1 = x.Fertilizer1,
                    Confidence1 = x.Confidence1,

                    Fertilizer2 = x.Fertilizer2,
                    Confidence2 = x.Confidence2,

                    Fertilizer3 = x.Fertilizer3,
                    Confidence3 = x.Confidence3,
                })
                .ToListAsync();
        }
    }
}
