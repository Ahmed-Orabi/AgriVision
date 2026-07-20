using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgriVision.Infrastructure.Services
{
    public class HuggingFaceFertilizerRecommendationService : IFertilizerRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public HuggingFaceFertilizerRecommendationService(
            HttpClient httpClient,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<FertilizerRecommendationResponseDto> RecommendAsync(
            FertilizerRecommendationRequestDto dto)
        {
            var baseUrl = _config["AI:FertilizerRecommendation:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("AI:FertilizerRecommendation:BaseUrl is not configured");

            var url = $"{baseUrl}/submit_prediction";

            var apiKey = _config["HuggingFaceSpaces:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("HuggingFaceSpaces:ApiKey is missing (Private Space).");

            var payload = new List<FertilizerRecommendationRequestDto> { dto };
            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"HF Fertilizer Error:\n" +
                    $"URL: {url}\n" +
                    $"Status: {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                    $"Body: {body}"
                );
            }

            /*
              Expected HF response (example):
              [
                { "fertilizer": "NPK 20-20-20", "confidence": 0.62 },
                { "fertilizer": "Urea", "confidence": 0.25 },
                { "fertilizer": "DAP", "confidence": 0.13 }
              ]
            */

            var rawPredictions = JsonSerializer.Deserialize<List<FertilizerPredictionRaw>>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();

            var predictions = rawPredictions
                .Select(x => new FertilizerPredictionDto
                {
                    Fertilizer = x.Name ?? string.Empty,
                    Confidence = x.Probability
                })
                .OrderByDescending(x => x.Confidence)
                .Take(3)
                .ToList();


            return new FertilizerRecommendationResponseDto
            {
                Predictions = predictions,
                Raw = TryParseRaw(body)
            };
        }

        // =========================
        // Helpers
        // =========================

        private static object? TryParseRaw(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return json;
            }
        }

        private class FertilizerPredictionRaw
        {
            public string? Name { get; set; }
            public double Probability { get; set; }
        }

    }
}
