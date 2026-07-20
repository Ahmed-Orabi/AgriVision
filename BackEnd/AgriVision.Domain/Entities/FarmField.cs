using System;

namespace AgriVision.Domain.Entities
{
    public class FarmField
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Field KeyId

        public string Name { get; set; } = default!; // Field Name

        public string Crop { get; set; } = default!; // Crop Name

        public double XStart { get; set; } // Field Length Start

        public double XEnd { get; set; } // Field Length End

        public double YStart { get; set; } // Field Width Start

        public double YEnd { get; set; } // Field Width End

        public string Status { get; set; } = "Healthy"; // Field Status

        public double? Temperature { get; set; } // Latest Temperature

        public double? Moisture { get; set; } // Latest Moisture

        public double? SoilTemperature { get; set; } // Latest Soil Temperature

        public double? SoilMoisture { get; set; } // Latest Soil Moisture

        public DateTime? UpdatedAtUtc { get; set; } // Latest Update Time

        public Guid FarmId { get; set; } // Farm Id (FK)

        public Farm Farm { get; set; } = default!; // Farm Navigation
    }
}
