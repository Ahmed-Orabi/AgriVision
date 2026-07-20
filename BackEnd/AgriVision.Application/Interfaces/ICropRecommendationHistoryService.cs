using System.Collections.Generic;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;

namespace AgriVision.Application.Interfaces
{
    public interface ICropRecommendationHistoryService
    {
        Task SaveAsync(string userId, CropRecommendationRequestDto request, CropRecommendationResponseDto response);
        Task<List<CropRecommendationHistoryItemDto>> GetLatestAsync(string userId, int take = 6);
    }
}
