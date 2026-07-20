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
    public class InsectDetectionService : IInsectDetectionService
    {
        private readonly AgriVisionDbContext _context;
        private readonly GradioProvider _gradioProvider;
        private readonly string _baseUrl;

        public InsectDetectionService(
            AgriVisionDbContext context,
            GradioProvider gradioProvider,
            IConfiguration configuration)
        {
            _context = context;
            _gradioProvider = gradioProvider;
            _baseUrl = configuration["ComputerVision:InsectDetection:BaseUrl"]
                ?? "https://ahmedorabi-farm-insect-detection.hf.space";
        }

        public async Task<InsectDetectionResultDTO> DetectInsect(InsectDetectionRequestDTO insectDetectionRequest, string userId, CancellationToken ct)
        {
            try
            {
                var uploadedImagePath = await _gradioProvider.UploadImageAsync(insectDetectionRequest.ImageFile, _baseUrl, ct);
                var eventId = await _gradioProvider.SubmitPredictionAsync(uploadedImagePath, _baseUrl, ct);
                var result = await _gradioProvider.GetPredictionResultAsync(_baseUrl, eventId, ct);
                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("Insect model returned an empty prediction result URL.");
                }

                var insectDetection = new InsectDetection
                {
                    UserId = userId,
                    DetectionResultURL = result,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InsectDetections.Add(insectDetection);
                await _context.SaveChangesAsync(ct);

                return new InsectDetectionResultDTO
                {
                    DetectionId = insectDetection.InsectDetectionId,
                    Url = result,
                    CreatedAt = insectDetection.CreatedAt
                };
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException(
                    $"Insect model request failed at '{_baseUrl}'. The Hugging Face Space might be down. Details: {ex.Message}",
                    ex,
                    ex.StatusCode);
            }
        }

        public async Task<InsectDetectionResultDTO?> GetDetectionById(int detectionId, string userId, CancellationToken ct)
        {
            var result = await _context.InsectDetections
                .Where(id => id.InsectDetectionId == detectionId && id.UserId == userId)
                .Select(id => new InsectDetectionResultDTO
                {
                    DetectionId = id.InsectDetectionId,
                    Url = id.DetectionResultURL,
                    CreatedAt = id.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            return result;
        }

        public async Task<IEnumerable<InsectDetectionResultDTO>> GetDetectionHistory(string userId, CancellationToken ct)
        {
            var detections = await _context.InsectDetections
                .Where(id => id.UserId == userId)
                .OrderByDescending(id => id.CreatedAt)
                .Select(id => new InsectDetectionResultDTO
                {
                    DetectionId = id.InsectDetectionId,
                    Url = id.DetectionResultURL,
                    CreatedAt = id.CreatedAt
                })
                .ToListAsync(ct);

            return detections;
        }
    }
}
