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
    public class InsectDetectionController : ControllerBase
    {
        private readonly IInsectDetectionService _insectDetection;

        public InsectDetectionController(IInsectDetectionService insectDetection)
        {
            _insectDetection = insectDetection;
        }

        [HttpPost("detect")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<InsectDetectionResultDTO>> DetectInsect([FromForm] InsectDetectionRequestDTO image, CancellationToken ct)
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
                var result = await _insectDetection.DetectInsect(image, userId, ct);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Insect detection service is temporarily unavailable. Details: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Insect detection service returned an invalid response. Details: {ex.Message}");
            }
        }

        [HttpGet("getDetections")]
        public async Task<ActionResult<IEnumerable<InsectDetectionResultDTO>>> GetDetections(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var results = await _insectDetection.GetDetectionHistory(userId, ct);
            if (!results.Any())
            {
                return NotFound("No insect detection records found.");
            }

            return Ok(results);
        }

        [HttpGet("getDetections/{detectionId}")]
        public async Task<ActionResult<InsectDetectionResultDTO>> GetDetectionById([FromRoute] int detectionId, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _insectDetection.GetDetectionById(detectionId, userId, ct);
            if (result == null)
            {
                return NotFound("Insect detection record not found.");
            }

            return Ok(result);
        }
    }
}

