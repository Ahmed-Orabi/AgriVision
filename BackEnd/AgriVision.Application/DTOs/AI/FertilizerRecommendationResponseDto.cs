using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Application.DTOs.AI
{
    public class FertilizerRecommendationResponseDto
    {
        public List<FertilizerPredictionDto> Predictions { get; set; } = new();
        public object? Raw { get; set; }
    }

    public class FertilizerPredictionDto
    {
        public string Fertilizer { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

}
