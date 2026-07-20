using System.Text.Json.Serialization;

namespace AgriVision.Application.DTOs.AI
{
    public class CropRecommendationRequestDto
    {
        [JsonPropertyName("N")]
        public int N { get; set; }

        [JsonPropertyName("P")]
        public int P { get; set; }

        [JsonPropertyName("K")]
        public int K { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        [JsonPropertyName("ph")]
        public double Ph { get; set; }

        [JsonPropertyName("rainfall")]
        public double Rainfall { get; set; }
    }
}
