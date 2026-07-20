using Microsoft.AspNetCore.Http;

namespace AgriVision.Application.DTOs.CVDetections
{
    public class InsectDetectionRequestDTO
    {
        public required IFormFile ImageFile { get; set; }
    }
}
