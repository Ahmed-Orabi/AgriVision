using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/config")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("frontend")]
        public IActionResult GetFrontendConfig()
        {
            var baseUrl = _configuration["Frontend:BaseUrl"] ?? string.Empty;
            return Ok(new { baseUrl = baseUrl.TrimEnd('/') });
        }
    }
}
