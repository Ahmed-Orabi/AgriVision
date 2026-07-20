namespace AgriVision.Application.DTOs.AI
{
    public class FertilizerRecommendationHistoryItemDto
    {
        public DateTime CreatedAtUtc { get; set; }

        public string Fertilizer1 { get; set; } = string.Empty;
        public double Confidence1 { get; set; }

        public string Fertilizer2 { get; set; } = string.Empty;
        public double Confidence2 { get; set; }

        public string Fertilizer3 { get; set; } = string.Empty;
        public double Confidence3 { get; set; }
    }
}
