using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.CCTVcrud.DTOs;
using YopoBackend.Modules.CCTVcrud.Services;
using System.Security.Claims;

namespace YopoBackend.Modules.CCTVcrud.Controllers
{
    /// <summary>
    /// Controller for managing CCTV camera operations.
    /// Module ID: 8 (CCTVcrud)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.CCTVcrudModuleId)]
    public class CCTVsController : ControllerBase
    {
        private readonly ICCTVService _cctvService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCTVsController"/> class.
        /// </summary>
        /// <param name="cctvService">The CCTV service.</param>
        public CCTVsController(ICCTVService cctvService)
        {
            _cctvService = cctvService;
        }

        /// <summary>
        /// Gets all CCTV cameras that the current user has access to.
        /// </summary>
        /// <returns>A list of all CCTV cameras.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CCTVDto>>> GetAllCCTVs()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctvs = await _cctvService.GetAllCCTVsAsync(userId);
                return Ok(cctvs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving CCTVs.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all active CCTV cameras that the current user has access to.
        /// </summary>
        /// <returns>A list of active CCTV cameras.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CCTVDto>>> GetActiveCCTVs()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctvs = await _cctvService.GetActiveCCTVsAsync(userId);
                return Ok(cctvs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active CCTVs.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all public CCTV cameras (accessible to all users).
        /// </summary>
        /// <returns>A list of public CCTV cameras.</returns>
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<CCTVSummaryDto>>> GetPublicCCTVs()
        {
            try
            {
                var cctvs = await _cctvService.GetPublicCCTVsAsync();
                return Ok(cctvs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving public CCTVs.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets CCTV cameras with stream URLs for monitoring dashboard.
        /// </summary>
        /// <param name="buildingId">Optional building ID to filter by.</param>
        /// <returns>A list of CCTV cameras with stream information.</returns>
        [HttpGet("monitoring")]
        public async Task<ActionResult<IEnumerable<CCTVDto>>> GetCCTVsForMonitoring([FromQuery] int? buildingId = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctvs = await _cctvService.GetCCTVsForMonitoringAsync(userId, buildingId);
                return Ok(cctvs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving CCTVs for monitoring.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific CCTV camera by ID.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera.</param>
        /// <returns>The CCTV camera with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CCTVDto>> GetCCTVById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctv = await _cctvService.GetCCTVByIdAsync(id, userId);
                
                if (cctv == null)
                {
                    return NotFound(new { message = "CCTV camera not found or you don't have access to it." });
                }
                
                return Ok(cctv);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the CCTV camera.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all CCTV cameras for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <returns>A list of CCTV cameras in the specified building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<IEnumerable<CCTVDto>>> GetCCTVsByBuildingId(int buildingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctvs = await _cctvService.GetCCTVsByBuildingIdAsync(buildingId, userId);
                return Ok(cctvs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving CCTVs for the building.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all CCTV cameras assigned to a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A list of CCTV cameras assigned to the specified tenant.</returns>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<IEnumerable<CCTVDto>>> GetCCTVsByTenantId(int tenantId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var cctvs = await _cctvService.GetCCTVsByTenantIdAsync(tenantId, userId);
                return Ok(cctvs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving CCTVs for the tenant.", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new CCTV camera.
        /// </summary>
        /// <param name="createCCTVDto">The data for creating the CCTV camera.</param>
        /// <returns>The created CCTV camera.</returns>
        [HttpPost]
        public async Task<ActionResult<CCTVDto>> CreateCCTV([FromBody] CreateCCTVDto createCCTVDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                // Check if CCTV name already exists in the building
                var nameExists = await _cctvService.CCTVExistsInBuildingAsync(createCCTVDto.Name, createCCTVDto.BuildingId);
                if (nameExists)
                {
                    return BadRequest(new { message = "A CCTV camera with this name already exists in the specified building." });
                }

                var createdCCTV = await _cctvService.CreateCCTVAsync(createCCTVDto, userId);
                return CreatedAtAction(nameof(GetCCTVById), new { id = createdCCTV.CctvId }, createdCCTV);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the CCTV camera.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing CCTV camera.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera to update.</param>
        /// <param name="updateCCTVDto">The data for updating the CCTV camera.</param>
        /// <returns>The updated CCTV camera.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CCTVDto>> UpdateCCTV(int id, [FromBody] UpdateCCTVDto updateCCTVDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                // Check if CCTV name already exists in the building (excluding current CCTV)
                var nameExists = await _cctvService.CCTVExistsInBuildingAsync(updateCCTVDto.Name, updateCCTVDto.BuildingId, id);
                if (nameExists)
                {
                    return BadRequest(new { message = "A CCTV camera with this name already exists in the specified building." });
                }

                var updatedCCTV = await _cctvService.UpdateCCTVAsync(id, updateCCTVDto, userId);
                
                if (updatedCCTV == null)
                {
                    return NotFound(new { message = "CCTV camera not found or you don't have access to it." });
                }
                
                return Ok(updatedCCTV);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the CCTV camera.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a CCTV camera.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera to delete.</param>
        /// <returns>A success message if the deletion was successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCCTV(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var deleted = await _cctvService.DeleteCCTVAsync(id, userId);
                
                if (!deleted)
                {
                    return NotFound(new { message = "CCTV camera not found or you don't have access to it." });
                }
                
                return Ok(new { message = "CCTV camera deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the CCTV camera.", details = ex.Message });
            }
        }

        /// <summary>
        /// Checks if a CCTV camera name exists in a specific building.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="buildingId">The building ID to check within.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists, false otherwise.</returns>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> CheckCCTVNameExists([FromQuery] string name, [FromQuery] int buildingId, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Name parameter is required." });
                }

                var exists = await _cctvService.CCTVExistsInBuildingAsync(name, buildingId, excludeId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking CCTV name existence.", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates if the current user has access to a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID to validate.</param>
        /// <returns>True if user has access, false otherwise.</returns>
        [HttpGet("validate-building-access/{buildingId}")]
        public async Task<ActionResult<bool>> ValidateBuildingAccess(int buildingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var hasAccess = await _cctvService.ValidateBuildingAccessAsync(buildingId, userId);
                return Ok(new { hasAccess });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while validating building access.", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates if the current user has access to a specific tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID to validate.</param>
        /// <returns>True if user has access, false otherwise.</returns>
        [HttpGet("validate-tenant-access/{tenantId}")]
        public async Task<ActionResult<bool>> ValidateTenantAccess(int tenantId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var hasAccess = await _cctvService.ValidateTenantAccessAsync(tenantId, userId);
                return Ok(new { hasAccess });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while validating tenant access.", details = ex.Message });
            }
        }
    }
}
