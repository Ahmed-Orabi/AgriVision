using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.Weather;
using AgriVision.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgriVision.Infrastructure.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<WeatherSnapshotDto> GetCurrentAsync(double latitude, double longitude)
        {
            var baseUrl = _configuration["Weather:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "https://api.open-meteo.com/v1/forecast";

            var url = $"{baseUrl}?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m&timezone=auto";

            var response = await _httpClient.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Weather API error: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("current", out var current))
                return new WeatherSnapshotDto();

            double? temp = null;
            double? humidity = null;

            if (current.TryGetProperty("temperature_2m", out var tempEl) && tempEl.ValueKind == JsonValueKind.Number)
                temp = tempEl.GetDouble();

            if (current.TryGetProperty("relative_humidity_2m", out var humEl) && humEl.ValueKind == JsonValueKind.Number)
                humidity = humEl.GetDouble();

            return new WeatherSnapshotDto
            {
                Temperature = temp,
                Humidity = humidity
            };
        }
    }
}
