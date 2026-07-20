using System;
using System.Collections.Generic;

namespace AgriVision.Domain.Entities
{
    public class CropRecommendationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique identifier for this recommendation history record

        // user
        public string UserId { get; set; } = default!; // The ID of the user who requested the crop recommendation

        public ApplicationUser User { get; set; } = default!;  // Navigation property to the user entity (EF Core relationship)

        // input
        public int N { get; set; } // Nitrogen value (N) used as input for the prediction

        public int P { get; set; } // Phosphorus value (P) used as input for the prediction

        public int K { get; set; } // Potassium value (K) used as input for the prediction

        public double Temperature { get; set; } // Temperature value (°C) used as input for the prediction

        public double Humidity { get; set; } // Humidity percentage (%) used as input for the prediction

        public double Ph { get; set; } // Soil pH value used as input for the prediction

        public double Rainfall { get; set; } // Rainfall amount (mm) used as input for the prediction

        // output (top 3)
        public string Crop1 { get; set; } = default!; // The first (best) predicted crop result

        public double Confidence1 { get; set; } // Confidence score (%) for the first predicted crop

        public string Crop2 { get; set; } = default!; // The second predicted crop result

        public double Confidence2 { get; set; } // Confidence score (%) for the second predicted crop

        public string Crop3 { get; set; } = default!; // The third predicted crop result

        public double Confidence3 { get; set; } // Confidence score (%) for the third predicted crop

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow; // Timestamp (UTC) when this recommendation record was created
    }
}
