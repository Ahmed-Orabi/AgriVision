using AgriVision.Application.DTOs.CVDetections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Application.Interfaces
{
    public interface IPlantDetectionService
    {
        Task<PlantDetectionResultDTO> DetectPlant(PlantDetectionRequestDTO plantDetectionRequestDTO, string userId, CancellationToken ct);
        Task<IEnumerable<PlantDetectionResultDTO>> GetDetectionHistory(string userId, CancellationToken ct);
        Task<PlantDetectionResultDTO?> GetDetectionById(int detectionId, string userId, CancellationToken ct);
    }
}