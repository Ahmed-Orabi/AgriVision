using AgriVision.Application.DTOs.AI;
using AgriVision.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace AgriVision.Application.DTOs.AI
{
    public class IrrigationPredictionResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        [JsonPropertyName("input_received")]
        public SoilSensorReadingDto Input_Received { get; set; }

        [JsonPropertyName("predicted_irrigation_need")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PredictedIrrigationNeed Predicted_Irrigation_Need { get; set; }
    }
}
