using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;

namespace AgriVision.Application.Interfaces
{
    public interface IAiCropRecommendationService
    {
        Task<CropRecommendationResponseDto> RecommendAsync(CropRecommendationRequestDto dto);
    }
}
