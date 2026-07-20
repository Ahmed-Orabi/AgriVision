using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;

namespace AgriVision.Application.Interfaces
{
    public interface IFertilizerRecommendationService
    {
        Task<FertilizerRecommendationResponseDto> RecommendAsync(FertilizerRecommendationRequestDto request);
    }

}
