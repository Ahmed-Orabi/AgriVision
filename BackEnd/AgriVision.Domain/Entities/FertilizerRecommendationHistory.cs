namespace AgriVision.Domain.Entities
{
    public class FertilizerRecommendationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // user
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        // input
        public double Temperature { get; set; }
        public double Moisture { get; set; }
        public double Rainfall { get; set; }
        public double PH { get; set; }

        public double Nitrogen { get; set; }
        public double Phosphorous { get; set; }
        public double Potassium { get; set; }
        public double Carbon { get; set; }

        public string Soil { get; set; } = default!;
        public string Crop { get; set; } = default!;

        // output (top 3)
        public string Fertilizer1 { get; set; } = default!;
        public double Confidence1 { get; set; }

        public string Fertilizer2 { get; set; } = default!;
        public double Confidence2 { get; set; }

        public string Fertilizer3 { get; set; } = default!;
        public double Confidence3 { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
