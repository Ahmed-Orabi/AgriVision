using Microsoft.AspNetCore.Http;

namespace AgriVision.Application.DTOs.CVDetections
{
    public class PlantDetectionRequestDTO
    {
        public required IFormFile ImageFile { get; set; }
    }
}
