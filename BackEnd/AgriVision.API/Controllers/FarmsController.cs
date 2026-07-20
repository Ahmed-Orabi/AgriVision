using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgriVision.Application.DTOs.Farms;
using AgriVision.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/farms")]
    public class FarmsController : ControllerBase
    {
        private readonly IFarmService _farmService;

        public FarmsController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var farms = await _farmService.GetUserFarmsAsync(userId);
            return Ok(farms);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var farm = await _farmService.GetByIdAsync(userId, id);
            if (farm == null) return NotFound();

            return Ok(farm);
        }

        [HttpGet("{id:guid}/state")]
        public async Task<IActionResult> GetState(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var farm = await _farmService.GetStateAsync(userId, id);
            if (farm == null) return NotFound();

            return Ok(farm);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FarmCreateRequestDto request)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            try
            {
                var id = await _farmService.CreateAsync(userId, request);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FarmUpdateRequestDto request)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            try
            {
                var updated = await _farmService.UpdateAsync(userId, id, request);
                if (!updated) return NotFound();
                return Ok(new { message = "Farm updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var deleted = await _farmService.DeleteAsync(userId, id);
            if (!deleted) return NotFound();

            return Ok(new { message = "Farm deleted" });
        }
    }
}
