using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.Geocoding;
using AgriVision.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgriVision.Infrastructure.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeocodingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ReverseGeocodeResultDto?> ReverseAsync(double latitude, double longitude)
        {
            var baseUrl = _configuration["Geocoding:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "https://nominatim.openstreetmap.org/reverse";

            var url = $"{baseUrl}?format=jsonv2&lat={latitude}&lon={longitude}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(_configuration["Geocoding:UserAgent"] ?? "AgriVision/1.0");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Geocoding API error: {(int)response.StatusCode} {response.ReasonPhrase}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (!root.TryGetProperty("address", out var address))
                return new ReverseGeocodeResultDto();

            string? city = null;
            string? country = null;

            if (address.TryGetProperty("city", out var cityEl))
                city = cityEl.GetString();
            else if (address.TryGetProperty("town", out var townEl))
                city = townEl.GetString();
            else if (address.TryGetProperty("village", out var villageEl))
                city = villageEl.GetString();
            else if (address.TryGetProperty("county", out var countyEl))
                city = countyEl.GetString();
            else if (address.TryGetProperty("state", out var stateEl))
                city = stateEl.GetString();

            if (address.TryGetProperty("country", out var countryEl))
                country = countryEl.GetString();

            string? displayName = null;
            if (root.TryGetProperty("display_name", out var displayEl))
                displayName = displayEl.GetString();

            return new ReverseGeocodeResultDto
            {
                City = city,
                Country = country,
                DisplayName = displayName
            };
        }
    }
}
