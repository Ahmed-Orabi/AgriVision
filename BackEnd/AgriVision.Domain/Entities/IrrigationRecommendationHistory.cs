using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.Enums;
namespace AgriVision.Domain.Entities
{
    public class IrrigationRecommendationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        //// user
        //public string UserId { get; set; } = default!;
        //public ApplicationUser User { get; set; } = default!;

        // input
        public SoilType gSoil_type { get; set; } = default!; // Unique values (4 total): ['Clay' 'Silt' 'Sandy' 'Loamy']
        public float Soil_pH { get; set; }
        public float Soil_Moisture { get; set; }
        public float Electrical_Conductivity { get; set; }
        public float Temperature_C { get; set; }
        public float Humidity { get; set; }
        public float Rainfall_mm { get; set; }
        public CropType Crop_Type { get; set; } = default!; // Unique values (6 total): ['Wheat' 'Maize' 'Cotton' 'Rice' 'Sugarcane' 'Potato']
        public CropGrowthStage Crop_Growth_Stage { get; set; } = default!; // Unique values (4 total): ['Vegetative' 'Flowering' 'Harvest' 'Sowing']
        public Season Season { get; set; } = default!; // Unique values (3 total): ['Rabi' 'Zaid' 'Kharif']
        public float Field_Area_hectare { get; set; } 

        // output 
        public string message { get; set; } = default!;
        public PredictedIrrigationNeed predicted_irrigation_need { get; set; } = default!; // Unique values (3 total): ['Low' 'Medium' 'High']

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
