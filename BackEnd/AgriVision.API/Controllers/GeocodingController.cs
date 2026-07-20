using AgriVision.Application.DTOs.Geocoding;
using AgriVision.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/geocode")]
    public class GeocodingController : ControllerBase
    {
        private readonly IGeocodingService _geocodingService;

        public GeocodingController(IGeocodingService geocodingService)
        {
            _geocodingService = geocodingService;
        }

        [HttpGet("reverse")]
        public async Task<ActionResult<ReverseGeocodeResultDto>> Reverse([FromQuery] double latitude, [FromQuery] double longitude)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                return BadRequest("Invalid coordinates");

            var result = await _geocodingService.ReverseAsync(latitude, longitude);
            return Ok(result ?? new ReverseGeocodeResultDto());
        }
    }
}
