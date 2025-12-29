using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.TenantCRUD.DTOs;
using YopoBackend.DTOs;
using YopoBackend.Modules.TenantCRUD.Services;

namespace YopoBackend.Modules.TenantCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("07-Tenants")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [YopoBackend.Attributes.RequireModule(YopoBackend.Constants.ModuleConstants.TENANT_MODULE_ID)]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<TenantResponseDTO>), 200)]
        public async Task<ActionResult<PaginatedResponse<TenantResponseDTO>>> GetTenants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? floorId = null,
            [FromQuery] int? unitId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isPaid = null,
            [FromQuery] int? tenantId = null)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var (tenants, totalRecords) = await _tenantService.GetTenantsAsync(currentUserId.Value, page, pageSize, searchTerm, buildingId, floorId, unitId, isActive, isPaid, tenantId);

            var paginatedResponse = new PaginatedResponse<TenantResponseDTO>(tenants, totalRecords, page, pageSize);

            return Ok(paginatedResponse);
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

        [HttpPatch("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UpdateTenantStatus(int id, [FromBody] UpdateTenantStatusDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var ok = await _tenantService.UpdateTenantStatusAsync(id, dto, currentUserId.Value);
            if (!ok) return NotFound(new { message = $"Tenant with ID {id} not found or not accessible." });
            return NoContent();
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteTenant(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var ok = await _tenantService.DeleteTenantAsync(id, currentUserId.Value);
            if (!ok) return NotFound(new { message = $"Tenant with ID {id} not found or not accessible." });
            return Ok(new { message = $"Tenant with ID {id} has been successfully deleted." });
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
