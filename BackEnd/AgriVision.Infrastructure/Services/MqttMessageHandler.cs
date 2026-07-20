using AgriVision.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;

namespace AgriVision.Infrastructure.Services
{
    public class MqttMessageHandler : IMqttMessageHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public MqttMessageHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleMessageAsync(string topic, byte[] payload)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            switch (topic)
            {
                case "agrivision/gateway/soilreadings":
                    // Handle readings topic
                    var soil_readings = JsonSerializer.Deserialize<SoilSensorReadingDto>(payload);
                    if(soil_readings is null)
                    {
                        Console.WriteLine("Failed to deserialize soil readings.");
                        return;
                    }
                    Console.WriteLine($"Received readings: {soil_readings}");
                    var irrigationService = scope.ServiceProvider.GetRequiredService<IIrrigationRecommendationService>();
                    await irrigationService.GetRecommendation(soil_readings);
                    break;

                default:
                    Console.WriteLine($"Received message on unknown topic: {topic}");
                    break;
            }
        }
    }
}
