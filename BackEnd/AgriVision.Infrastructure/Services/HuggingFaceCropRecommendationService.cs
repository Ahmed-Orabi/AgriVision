using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.AI;
using AgriVision.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgriVision.Infrastructure.Services
{
    public class HuggingFaceCropRecommendationService : IAiCropRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HuggingFaceCropRecommendationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<CropRecommendationResponseDto> RecommendAsync(CropRecommendationRequestDto dto)
        {
            var baseUrl = _configuration["AI:CropRecommendation:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("AI:CropRecommendation:BaseUrl is not configured");

            var url = $"{baseUrl}/submit_prediction";
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("HuggingFaceSpaces:CropRecommendationUrl is not configured in appsettings.json");

            var apiKey = _configuration["HuggingFaceSpaces:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("HuggingFaceSpaces:ApiKey is missing (Space is private).");

            // Space expects ARRAY in request body
            var payload = new List<CropRecommendationRequestDto> { dto };
            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Private Space token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"HF Space Error:\n" +
                    $"URL: {url}\n" +
                    $"Status: {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                    $"Body: {body}"
                );
            }

            // HF Space response format:
            // [
            //   { "name": "mango", "probability": 100.0 },
            //   { "name": "wheat", "probability": 0.0 },
            //   { "name": "rice", "probability": 0.0 }
            // ]

            var result = new CropRecommendationResponseDto
            {
                Raw = TryParseRaw(body),
                Predictions = MapPredictions(body)
            };

            // top 3
            result.Predictions = result.Predictions
                .OrderByDescending(x => x.Confidence)
                .Take(3)
                .ToList();

            return result;
        }

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

        private static List<CropPredictionDto> MapPredictions(string json)
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<SpacePrediction>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list == null) return new List<CropPredictionDto>();

                return list.Select(x => new CropPredictionDto
                {
                    Crop = x.Name ?? "",
                    Confidence = x.Probability
                }).ToList();
            }
            catch
            {
                return new List<CropPredictionDto>();
            }
        }

        private class SpacePrediction
        {
            public string? Name { get; set; }
            public double Probability { get; set; }
        }
    }
}
