using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.TenantCRUD.DTOs;
using YopoBackend.Modules.TenantCRUD.Services;

namespace YopoBackend.Modules.TenantCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("08-Tenants")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(TenantListResponseDTO), 200)]
        public async Task<ActionResult<TenantListResponseDTO>> GetTenants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? floorId = null,
            [FromQuery] int? unitId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isPaid = null)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var result = await _tenantService.GetTenantsAsync(currentUserId.Value, page, pageSize, searchTerm, buildingId, floorId, unitId, isActive, isPaid);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TenantResponseDTO), 201)]
        public async Task<ActionResult<TenantResponseDTO>> CreateTenant([FromBody] CreateTenantDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            if (!ModelState.IsValid) return BadRequest(ModelState);
            var tenant = await _tenantService.CreateTenantAsync(dto, currentUserId.Value);
            return CreatedAtAction(nameof(GetTenants), new { tenantId = tenant.TenantId }, tenant);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TenantResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TenantResponseDTO>> UpdateTenant(int id, [FromBody] UpdateTenantDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var tenant = await _tenantService.UpdateTenantAsync(id, dto, currentUserId.Value);
            if (tenant == null) return NotFound(new { message = $"Tenant with ID {id} not found or not accessible." });
            return Ok(tenant);
        }

        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeactivateTenant(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var ok = await _tenantService.DeactivateTenantAsync(id, currentUserId.Value);
            if (!ok) return NotFound(new { message = $"Tenant with ID {id} not found or not accessible." });
            return NoContent();
        }

        [HttpPatch("{id}/activate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ActivateTenant(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var ok = await _tenantService.ActivateTenantAsync(id, currentUserId.Value);
            if (!ok) return NotFound(new { message = $"Tenant with ID {id} not found or not accessible." });
            return NoContent();
        }

        [HttpPost("invite")]
        [ProducesResponseType(202)]
        public async Task<ActionResult> InviteTenant([FromBody] InviteTenantDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _tenantService.InviteTenantAsync(dto, currentUserId.Value);
            if (ok) return Accepted(new { message = "Invitation created." });
            return StatusCode(500, new { message = "Failed to create invitation." });
        }


        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}