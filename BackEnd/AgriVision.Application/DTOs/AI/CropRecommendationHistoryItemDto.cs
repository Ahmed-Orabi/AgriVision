using System;

namespace AgriVision.Application.DTOs.AI
{
    public class CropRecommendationHistoryItemDto
    {
        public Guid Id { get; set; }
        public string BestCrop { get; set; } = default!;
        public double BestConfidence { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public int N { get; set; }
        public double Temperature { get; set; }
        public double Rainfall { get; set; }
    }
}
