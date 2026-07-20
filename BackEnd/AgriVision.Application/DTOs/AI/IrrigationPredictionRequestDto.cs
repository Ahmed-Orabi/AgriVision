using AgriVision.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgriVision.Application.DTOs.AI
{
    public class IrrigationPredictionRequestDto
    {
        [JsonPropertyName("gSoil_Type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SoilType SoilType { get; set; }

        [JsonPropertyName("Soil_pH")]
        public float Soil_pH { get; set; }

        [JsonPropertyName("Soil_Moisture")]
        public float Soil_Moisture { get; set; }

        [JsonPropertyName("Electrical_Conductivity")]
        public float Electrical_Conductivity { get; set; }

        [JsonPropertyName("Temperature_C")]
        public float Temperature_C { get; set; }

        [JsonPropertyName("Humidity")]
        public float Humidity { get; set; }

        [JsonPropertyName("Rainfall_mm")]
        public float Rainfall_mm { get; set; }

        [JsonPropertyName("Crop_Type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CropType Crop_Type { get; set; }

        [JsonPropertyName("Crop_Growth_Stage")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CropGrowthStage Crop_Growth_Stage { get; set; }

        [JsonPropertyName("Season")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Season Season { get; set; }

        [JsonPropertyName("Field_Area_hectare")]
        public float Feald_Area_hectar { get; set; }
    }
}
