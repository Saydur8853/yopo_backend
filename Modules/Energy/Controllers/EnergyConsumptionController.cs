using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.Energy.DTOs;
using YopoBackend.Modules.Energy.Services;

namespace YopoBackend.Modules.Energy.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/energy/locations/{locationId}/energy")]
    [Tags("21-Energy")]
    public class EnergyConsumptionController : ControllerBase
    {
        private readonly IEnergyService _energyService;

        public EnergyConsumptionController(IEnergyService energyService)
        {
            _energyService = energyService;
        }

        [HttpGet("consumption")]
        [ProducesResponseType(typeof(EnergyConsumptionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<EnergyConsumptionDto>> GetConsumption(string locationId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                var result = await _energyService.GetEnergyConsumptionAsync(locationId, currentUserId.Value);
                if (result == null)
                {
                    return NotFound(new { message = $"Location '{locationId}' not found." });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("current-power")]
        [ProducesResponseType(typeof(double), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<double>> GetCurrentPower(string locationId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                return Ok(await _energyService.GetCurrentPowerAsync(locationId, currentUserId.Value));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("hourly")]
        [ProducesResponseType(typeof(List<HourlyDataDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<HourlyDataDto>>> GetHourly(string locationId, [FromQuery] DateTime? date)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                return Ok(await _energyService.GetHourlyAsync(locationId, date, currentUserId.Value));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("monthly")]
        [ProducesResponseType(typeof(List<MonthlyDataDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<MonthlyDataDto>>> GetMonthly(string locationId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                return Ok(await _energyService.GetMonthlyAsync(locationId, currentUserId.Value));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("topics/live")]
        [ProducesResponseType(typeof(EnergyTopicLiveResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<EnergyTopicLiveResponseDto>> GetLiveTopics(
            string locationId,
            [FromQuery] int limit = 20)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                var result = await _energyService.GetLiveTopicsAsync(locationId, currentUserId.Value, limit);
                if (result == null)
                {
                    return NotFound(new { message = $"Location '{locationId}' not found." });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
