using System.Collections.Generic;

namespace AgriVision.Application.DTOs.AI
{
    public class CropRecommendationResponseDto
    {
        public List<CropPredictionDto> Predictions { get; set; } = new();
        public object? Raw { get; set; } // optional debugging
    }

    public class CropPredictionDto
    {
        public string Crop { get; set; } = string.Empty;
        public double Confidence { get; set; } // probability
    }
}
