using AgriVision.Application.DTOs.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Application.Interfaces
{
    public interface IIrrigationRecommendationService
    {
        Task<IrrigationPredictionResponseDto> GetRecommendation(SoilSensorReadingDto soilReading);
    }
}
