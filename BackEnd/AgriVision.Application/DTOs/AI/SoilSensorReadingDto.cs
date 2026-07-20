using AgriVision.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace AgriVision.Application.DTOs.AI
{
    public class SoilSensorReadingDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SoilType SoilType { get; set; }
        
        public float Soil_pH { get; set; }
        public float Soil_Moisture { get; set; }
        public float Electrical_Conductivity { get; set; }
        public float Temperature_C { get; set; }
        public float Humidity { get; set; }
        public float Rainfall_mm { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CropType Crop_Type { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CropGrowthStage Crop_Growth_Stage { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Season Season { get; set; }
        
        public float Feald_Area_hectar { get; set; }
    }
}
