using AgriVision.Application.DTOs.CVDetections;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Infrastructure.Data;
using AgriVision.Infrastructure.Integrations.Gradio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace AgriVision.Infrastructure.Services
{
    public class PlantDetectionService : IPlantDetectionService
    {
        private readonly AgriVisionDbContext _context;
        private readonly GradioProvider _gradioProvider;
        private readonly string _baseUrl;

        public PlantDetectionService(
            AgriVisionDbContext context,
            GradioProvider gradioProvider,
            IConfiguration configuration)
        {
            _context = context;
            _gradioProvider = gradioProvider;
            _baseUrl = configuration["ComputerVision:PlantDetection:BaseUrl"]
                ?? "https://ahmedorabi-plant-village-object-detection.hf.space";
        }

        public async Task<PlantDetectionResultDTO> DetectPlant(PlantDetectionRequestDTO plantDetectionRequestDTO, string userId, CancellationToken ct)
        {
            try
            {
                var uploadedImagePath = await _gradioProvider.UploadImageAsync(plantDetectionRequestDTO.ImageFile, _baseUrl, ct);
                var eventId = await _gradioProvider.SubmitPredictionAsync(uploadedImagePath, _baseUrl, ct);
                var result = await _gradioProvider.GetPredictionResultAsync(_baseUrl, eventId, ct);
                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("Plant model returned an empty prediction result URL.");
                }

                var plantDetection = new PlantDetection
                {
                    UserId = userId,
                    DetectionResultURL = result,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlantDetections.Add(plantDetection);
                await _context.SaveChangesAsync(ct);

                return new PlantDetectionResultDTO
                {
                    DetectionId = plantDetection.PlantDetectionId,
                    Url = result,
                    CreatedAt = plantDetection.CreatedAt
                };
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException(
                    $"Plant model request failed at '{_baseUrl}'. The Hugging Face Space might be down. Details: {ex.Message}",
                    ex,
                    ex.StatusCode);
            }
        }

        public async Task<PlantDetectionResultDTO?> GetDetectionById(int detectionId, string userId, CancellationToken ct)
        {
            var result = await _context.PlantDetections
                .Where(pd => pd.PlantDetectionId == detectionId && pd.UserId == userId)
                .Select(pd => new PlantDetectionResultDTO
                {
                    DetectionId = pd.PlantDetectionId,
                    Url = pd.DetectionResultURL,
                    CreatedAt = pd.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            return result;
        }

        public async Task<IEnumerable<PlantDetectionResultDTO>> GetDetectionHistory(string userId, CancellationToken ct)
        {
            var detections = await _context.PlantDetections
                .Where(pd => pd.UserId == userId)
                .OrderByDescending(pd => pd.CreatedAt)
                .Select(pd => new PlantDetectionResultDTO
                {
                    DetectionId = pd.PlantDetectionId,
                    Url = pd.DetectionResultURL,
                    CreatedAt = pd.CreatedAt
                })
                .ToListAsync(ct);

            return detections;
        }
    }
}
