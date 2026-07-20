using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Infrastructure.Services
{
    public class IrrigationRecommendationService : IIrrigationRecommendationService
    {
        private readonly AgriVisionDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _irrigationPredictionURL;
        public IrrigationRecommendationService(AgriVisionDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
            _irrigationPredictionURL = _config["AI:IrrigationRecommendation:BaseUrl"] ?? throw new InvalidOperationException("Irrigation prediction URL not configured.");
        }
        public async Task<IrrigationPredictionResponseDto> GetRecommendation(SoilSensorReadingDto soilReading)
        {
            var request = new IrrigationPredictionRequestDto
            {
                SoilType = soilReading.SoilType,
                Soil_pH = soilReading.Soil_pH,
                Soil_Moisture = soilReading.Soil_Moisture,
                Electrical_Conductivity = soilReading.Electrical_Conductivity,
                Temperature_C = soilReading.Temperature_C,
                Humidity = soilReading.Humidity,
                Rainfall_mm = soilReading.Rainfall_mm,
                Crop_Type = soilReading.Crop_Type,
                Crop_Growth_Stage = soilReading.Crop_Growth_Stage,
                Season = soilReading.Season,
                Feald_Area_hectar = soilReading.Feald_Area_hectar
            };
            var response = await _httpClient.PostAsJsonAsync(_irrigationPredictionURL, request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<IrrigationPredictionResponseDto>();
            if (result is null)
                throw new Exception("ML service returned null response");
            var irrigationHistory = new IrrigationRecommendationHistory
            {
                gSoil_type = soilReading.SoilType,
                Soil_pH = soilReading.Soil_pH,
                Soil_Moisture = soilReading.Soil_Moisture,
                Electrical_Conductivity = soilReading.Electrical_Conductivity,
                Temperature_C = soilReading.Temperature_C,
                Humidity = soilReading.Humidity,
                Rainfall_mm = soilReading.Rainfall_mm,
                Crop_Type = soilReading.Crop_Type,
                Crop_Growth_Stage = soilReading.Crop_Growth_Stage,
                Season = soilReading.Season,
                Field_Area_hectare = soilReading.Feald_Area_hectar,
                predicted_irrigation_need = result.Predicted_Irrigation_Need,
                message = result.Message
            };
            await _context.IrrigationRecommendationHistories.AddAsync(irrigationHistory);
            await _context.SaveChangesAsync();
            return result;
            //Console.WriteLine($"you have reached the GetRecommendation Function soilreading.soiltype = {soilReading.SoilType}");
            //throw new NotImplementedException("Irrigation recommendation service is not implemented yet. This is a placeholder to demonstrate the structure of the code. Please implement the actual logic to call the ML service and save the history to the database.");
        }
    }
}
