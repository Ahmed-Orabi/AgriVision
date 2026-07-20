using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;

namespace AgriVision.Application.Interfaces
{
    public interface IFertilizerRecommendationHistoryService
    {
        Task SaveAsync(
            string userId,
            FertilizerRecommendationRequestDto request,
            FertilizerRecommendationResponseDto response);

        Task<List<FertilizerRecommendationHistoryItemDto>> GetLatestAsync(
            string userId,
            int take);
    }
}
