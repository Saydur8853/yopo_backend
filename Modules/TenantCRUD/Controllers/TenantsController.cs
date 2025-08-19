using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.TenantCRUD.DTOs;
using YopoBackend.Modules.TenantCRUD.Services;

namespace YopoBackend.Modules.TenantCRUD.Controllers
{
    /// <summary>
    /// Controller for managing tenant operations.
    /// Module: TenantCRUD (Module ID: 5)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.TENANT_MODULE_ID)]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantsController> _logger;

        /// <summary>
        /// Initializes a new instance of the TenantsController class.
        /// </summary>
        /// <param name="tenantService">The tenant service.</param>
        /// <param name="logger">The logger.</param>
        public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all tenants with pagination and optional search criteria.
        /// </summary>
        /// <param name="searchDto">Search criteria and pagination parameters.</param>
        /// <returns>A paginated list of tenants.</returns>
        [HttpGet]
        public async Task<ActionResult<TenantListDto>> GetTenants([FromQuery] TenantSearchDto searchDto)
        {
            try
            {
                var result = await _tenantService.GetAllTenantsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants");
                return StatusCode(500, new { message = "An error occurred while retrieving tenants." });
            }
        }

        /// <summary>
        /// Gets all active tenants.
        /// </summary>
        /// <returns>A list of active tenants.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetActiveTenants()
        {
            try
            {
                var result = await _tenantService.GetActiveTenantsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active tenants");
                return StatusCode(500, new { message = "An error occurred while retrieving active tenants." });
            }
        }

        /// <summary>
        /// Gets a tenant by its ID.
        /// </summary>
        /// <param name="id">The tenant ID.</param>
        /// <returns>The tenant with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TenantDto>> GetTenant(int id)
        {
            try
            {
                var result = await _tenantService.GetTenantByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Tenant with ID {id} not found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the tenant." });
            }
        }

        /// <summary>
        /// Gets tenants by building ID.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <returns>A list of tenants in the specified building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenantsByBuilding(int buildingId)
        {
            try
            {
                var result = await _tenantService.GetTenantsByBuildingIdAsync(buildingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants for building {BuildingId}", buildingId);
                return StatusCode(500, new { message = "An error occurred while retrieving tenants for the building." });
            }
        }

        /// <summary>
        /// Gets tenants by building and floor.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="floor">The floor number.</param>
        /// <returns>A list of tenants on the specified floor.</returns>
        [HttpGet("building/{buildingId}/floor/{floor}")]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenantsByFloor(int buildingId, int floor)
        {
            try
            {
                var result = await _tenantService.GetTenantsByFloorAsync(buildingId, floor);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants for building {BuildingId}, floor {Floor}", buildingId, floor);
                return StatusCode(500, new { message = "An error occurred while retrieving tenants for the floor." });
            }
        }

        /// <summary>
        /// Gets tenants with expiring contracts.
        /// </summary>
        /// <param name="days">Number of days to look ahead (default: 30).</param>
        /// <returns>A list of tenants with expiring contracts.</returns>
        [HttpGet("expiring-contracts")]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenantsWithExpiringContracts([FromQuery] int days = 30)
        {
            try
            {
                var result = await _tenantService.GetTenantsWithExpiringContractsAsync(days);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants with expiring contracts");
                return StatusCode(500, new { message = "An error occurred while retrieving tenants with expiring contracts." });
            }
        }

        /// <summary>
        /// Gets tenants who haven't paid their dues.
        /// </summary>
        /// <returns>A list of unpaid tenants.</returns>
        [HttpGet("unpaid")]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetUnpaidTenants()
        {
            try
            {
                var result = await _tenantService.GetUnpaidTenantsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unpaid tenants");
                return StatusCode(500, new { message = "An error occurred while retrieving unpaid tenants." });
            }
        }

        /// <summary>
        /// Gets tenant statistics.
        /// </summary>
        /// <param name="buildingId">Optional building ID to filter by.</param>
        /// <returns>Tenant statistics.</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetTenantStatistics([FromQuery] int? buildingId = null)
        {
            try
            {
                var result = await _tenantService.GetTenantStatisticsAsync(buildingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving tenant statistics." });
            }
        }

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="createTenantDto">The tenant creation data.</param>
        /// <returns>The created tenant.</returns>
        [HttpPost]
        public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantDto createTenantDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tenantService.CreateTenantAsync(createTenantDto);
                return CreatedAtAction(nameof(GetTenant), new { id = result.TenantId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating tenant");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return StatusCode(500, new { message = "An error occurred while creating the tenant." });
            }
        }

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        /// <param name="id">The tenant ID.</param>
        /// <param name="updateTenantDto">The tenant update data.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<TenantDto>> UpdateTenant(int id, [FromBody] UpdateTenantDto updateTenantDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tenantService.UpdateTenantAsync(id, updateTenantDto);
                if (result == null)
                {
                    return NotFound(new { message = $"Tenant with ID {id} not found." });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating tenant {TenantId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant with ID {TenantId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the tenant." });
            }
        }

        /// <summary>
        /// Updates the payment status for a tenant.
        /// </summary>
        /// <param name="id">The tenant ID.</param>
        /// <param name="paid">The payment status.</param>
        /// <returns>Success status.</returns>
        [HttpPatch("{id}/payment-status")]
        public async Task<ActionResult> UpdatePaymentStatus(int id, [FromBody] bool paid)
        {
            try
            {
                var result = await _tenantService.UpdatePaymentStatusAsync(id, paid);
                if (!result)
                {
                    return NotFound(new { message = $"Tenant with ID {id} not found." });
                }

                return Ok(new { message = "Payment status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for tenant {TenantId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the payment status." });
            }
        }

        /// <summary>
        /// Deletes a tenant.
        /// </summary>
        /// <param name="id">The tenant ID.</param>
        /// <returns>Success status.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTenant(int id)
        {
            try
            {
                var result = await _tenantService.DeleteTenantAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Tenant with ID {id} not found." });
                }

                return Ok(new { message = "Tenant deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant with ID {TenantId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the tenant." });
            }
        }

        /// <summary>
        /// Checks if a tenant exists in a specific unit.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="unitNo">The unit number.</param>
        /// <returns>Boolean indicating if tenant exists in unit.</returns>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> CheckTenantExistsInUnit([FromQuery] int buildingId, [FromQuery] string unitNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(unitNo))
                {
                    return BadRequest(new { message = "Unit number is required." });
                }

                var result = await _tenantService.TenantExistsInUnitAsync(buildingId, unitNo);
                return Ok(new { exists = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tenant existence in unit {UnitNo} of building {BuildingId}", unitNo, buildingId);
                return StatusCode(500, new { message = "An error occurred while checking tenant existence." });
            }
        }
    }
}
