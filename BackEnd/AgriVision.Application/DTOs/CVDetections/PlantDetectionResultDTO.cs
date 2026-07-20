using System.Text.Json.Serialization;

namespace AgriVision.Application.DTOs.CVDetections
{
    public class PlantDetectionResultDTO
    {
        [JsonPropertyName("detectionId")]
        public int DetectionId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
