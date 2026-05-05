using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.Energy.DTOs;
using YopoBackend.Modules.Energy.Services;

namespace YopoBackend.Modules.Energy.Controllers
{
    [ApiController]
    [Authorize]
    [RequireModule(ModuleConstants.ENERGY_MODULE_ID)]
    [Route("api/energy/locations")]
    [Tags("21-Energy")]
    public class EnergyLocationsController : ControllerBase
    {
        private readonly IEnergyService _energyService;

        public EnergyLocationsController(IEnergyService energyService)
        {
            _energyService = energyService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EnergyLocationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<EnergyLocationDto>>> GetLocations()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            return Ok(await _energyService.GetLocationsAsync(currentUserId.Value));
        }

        [HttpGet("{locationId}/overview")]
        [ProducesResponseType(typeof(BuildingOverviewDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<BuildingOverviewDto>> GetOverview(string locationId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                var result = await _energyService.GetOverviewAsync(locationId, currentUserId.Value);
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

        [HttpGet("{locationId}/dewa-meters")]
        [ProducesResponseType(typeof(List<DewaMeterDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<DewaMeterDto>>> GetDewaMeters(string locationId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "You are not authenticated." });
            }

            try
            {
                return Ok(await _energyService.GetDewaMetersAsync(locationId, currentUserId.Value));
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
