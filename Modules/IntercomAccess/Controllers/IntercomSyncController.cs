using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.Auth;
using YopoBackend.Modules.IntercomAccess.Services;

namespace YopoBackend.Modules.IntercomAccess.Controllers
{
    [ApiController]
    [Route("api/intercom")]
    [Produces("application/json")]
    [Tags("09-Intercom Access")]
    public class IntercomSyncController : ControllerBase
    {
        private readonly IIntercomSyncService _service;

        public IntercomSyncController(IIntercomSyncService service)
        {
            _service = service;
        }

        [HttpGet("pending-tenants")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPendingTenants([FromQuery] int buildingId)
        {
            if (buildingId <= 0) return BadRequest(new { message = "buildingId is required." });

            var items = await _service.GetPendingTenantsAsync(buildingId);
            return Ok(items);
        }

        [HttpPost("confirm-sync")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmSync([FromQuery] int buildingId)
        {
            if (buildingId <= 0) return BadRequest(new { message = "buildingId is required." });

            var deleted = await _service.ConfirmSyncAsync(buildingId);
            return Ok(new { message = "Sync confirmed.", deleted });
        }

        [HttpPost("backfill-pending")]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> BackfillPending([FromQuery] int? buildingId = null)
        {
            if (buildingId.HasValue && buildingId.Value <= 0)
            {
                return BadRequest(new { message = "buildingId must be positive." });
            }

            var (inserted, updated, skipped) = await _service.BackfillPendingAsync(buildingId);
            return Ok(new { message = "Backfill complete.", inserted, updated, skipped });
        }
    }
}
