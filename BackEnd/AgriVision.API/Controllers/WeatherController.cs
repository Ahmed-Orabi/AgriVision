using AgriVision.Application.DTOs.Weather;
using AgriVision.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<ActionResult<WeatherSnapshotDto>> Get([FromQuery] double latitude, [FromQuery] double longitude)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                return BadRequest("Invalid coordinates");

            var result = await _weatherService.GetCurrentAsync(latitude, longitude);
            return Ok(result);
        }
    }
}
