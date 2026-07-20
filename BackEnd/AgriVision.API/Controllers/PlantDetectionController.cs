using System.Net.Http;
using System.Security.Claims;
using AgriVision.Application.DTOs.CVDetections;
using AgriVision.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PlantDetectionController : ControllerBase
    {
        private readonly IPlantDetectionService _plantDetection;

        public PlantDetectionController(IPlantDetectionService plantDetection)
        {
            _plantDetection = plantDetection;
        }

        [HttpPost("detect")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PlantDetectionResultDTO>> DetectPlant([FromForm] PlantDetectionRequestDTO image, CancellationToken ct)
        {
            if (image.ImageFile == null || image.ImageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _plantDetection.DetectPlant(image, userId, ct);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Plant detection service is temporarily unavailable. Details: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Plant detection service returned an invalid response. Details: {ex.Message}");
            }
        }

        [HttpGet("getDetections")]
        public async Task<ActionResult<IEnumerable<PlantDetectionResultDTO>>> GetDetections(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var results = await _plantDetection.GetDetectionHistory(userId, ct);
            if (!results.Any())
            {
                return NotFound("No plant detection records found.");
            }

            return Ok(results);
        }

        [HttpGet("getDetections/{detectionId}")]
        public async Task<ActionResult<PlantDetectionResultDTO>> GetDetectionById([FromRoute] int detectionId, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _plantDetection.GetDetectionById(detectionId, userId, ct);
            if (result == null)
            {
                return NotFound("Plant detection record not found.");
            }

            return Ok(result);
        }
    }
}
