using AgriVision.Application.DTOs.CVDetections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Application.Interfaces
{
    public interface IInsectDetectionService
    {
        Task<InsectDetectionResultDTO> DetectInsect(InsectDetectionRequestDTO insectDetectionRequest, string userId, CancellationToken ct);
        Task<IEnumerable<InsectDetectionResultDTO>> GetDetectionHistory(string userId, CancellationToken ct);
        Task<InsectDetectionResultDTO?> GetDetectionById(int detectionId, string userId, CancellationToken ct);
    }
}