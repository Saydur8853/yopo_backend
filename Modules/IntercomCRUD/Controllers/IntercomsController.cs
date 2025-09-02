using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.IntercomCRUD.DTOs;
using YopoBackend.Modules.IntercomCRUD.Services;
using System.Security.Claims;

namespace YopoBackend.Modules.IntercomCRUD.Controllers
{
    /// <summary>
    /// Controller for managing Intercom system operations.
    /// Module ID: 9 (IntercomCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.INTERCOM_MODULE_ID)]
    public class IntercomsController : ControllerBase
    {
        private readonly IIntercomService _intercomService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomsController"/> class.
        /// </summary>
        /// <param name="intercomService">The intercom service.</param>
        public IntercomsController(IIntercomService intercomService)
        {
            _intercomService = intercomService;
        }

        /// <summary>
        /// Gets all intercom systems that the current user has access to.
        /// </summary>
        /// <returns>A list of all intercom systems.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetAllIntercoms()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetAllIntercomsAsync(userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all active intercom systems that the current user has access to.
        /// </summary>
        /// <returns>A list of active intercom systems.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetActiveIntercoms()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetActiveIntercomsAsync(userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active intercoms.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all installed intercom systems that the current user has access to.
        /// </summary>
        /// <returns>A list of installed intercom systems.</returns>
        [HttpGet("installed")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetInstalledIntercoms()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetInstalledIntercomsAsync(userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving installed intercoms.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all intercom systems that have CCTV integration.
        /// </summary>
        /// <returns>A list of intercom systems with CCTV integration.</returns>
        [HttpGet("with-cctv")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetIntercomsWithCCTV()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetIntercomsWithCCTVAsync(userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms with CCTV.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all intercom systems that have PIN pad functionality.
        /// </summary>
        /// <returns>A list of intercom systems with PIN pad functionality.</returns>
        [HttpGet("with-pin-pad")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetIntercomsWithPinPad()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetIntercomsWithPinPadAsync(userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms with PIN pad.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets intercom systems that require maintenance.
        /// </summary>
        /// <param name="monthsThreshold">Number of months since last service to consider requiring maintenance (default: 12).</param>
        /// <returns>A list of intercom systems that may require maintenance.</returns>
        [HttpGet("maintenance-required")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetIntercomsRequiringMaintenance([FromQuery] int monthsThreshold = 12)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetIntercomsRequiringMaintenanceAsync(userId, monthsThreshold);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms requiring maintenance.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets intercom systems with warranty expiring soon.
        /// </summary>
        /// <param name="daysThreshold">Number of days ahead to check for warranty expiry (default: 90).</param>
        /// <returns>A list of intercom systems with warranty expiring soon.</returns>
        [HttpGet("warranty-expiring")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetIntercomsWithExpiringWarranty([FromQuery] int daysThreshold = 90)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetIntercomsWithExpiringWarrantyAsync(userId, daysThreshold);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms with expiring warranty.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific intercom system by ID.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <returns>The intercom system with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<IntercomDto>> GetIntercomById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercom = await _intercomService.GetIntercomByIdAsync(id, userId);
                
                if (intercom == null)
                {
                    return NotFound(new { message = "Intercom system not found or you don't have access to it." });
                }
                
                return Ok(intercom);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the intercom system.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all intercom systems for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <returns>A list of intercom systems in the specified building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<IEnumerable<IntercomListDto>>> GetIntercomsByBuildingId(int buildingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var intercoms = await _intercomService.GetIntercomsByBuildingIdAsync(buildingId, userId);
                return Ok(intercoms);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercoms for the building.", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new intercom system.
        /// </summary>
        /// <param name="createIntercomDto">The data for creating the intercom system.</param>
        /// <returns>The created intercom system.</returns>
        [HttpPost]
        public async Task<ActionResult<IntercomDto>> CreateIntercom([FromBody] CreateIntercomDto createIntercomDto)
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
                
                // Validate building access
                var hasAccess = await _intercomService.ValidateBuildingAccessAsync(createIntercomDto.BuildingId, userId);
                if (!hasAccess)
                {
                    return Unauthorized(new { message = "You don't have access to this building." });
                }
                
                // Check if intercom name already exists in the building
                var nameExists = await _intercomService.IntercomExistsInBuildingAsync(createIntercomDto.Name, createIntercomDto.BuildingId);
                if (nameExists)
                {
                    return BadRequest(new { message = "An intercom system with this name already exists in the specified building." });
                }

                var createdIntercom = await _intercomService.CreateIntercomAsync(createIntercomDto, userId);
                return CreatedAtAction(nameof(GetIntercomById), new { id = createdIntercom.IntercomId }, createdIntercom);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the intercom system.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system to update.</param>
        /// <param name="updateIntercomDto">The data for updating the intercom system.</param>
        /// <returns>The updated intercom system.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<IntercomDto>> UpdateIntercom(int id, [FromBody] UpdateIntercomDto updateIntercomDto)
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
                
                // Validate building access
                var hasAccess = await _intercomService.ValidateBuildingAccessAsync(updateIntercomDto.BuildingId, userId);
                if (!hasAccess)
                {
                    return Unauthorized(new { message = "You don't have access to this building." });
                }
                
                // Check if intercom name already exists in the building (excluding current intercom)
                var nameExists = await _intercomService.IntercomExistsInBuildingAsync(updateIntercomDto.Name, updateIntercomDto.BuildingId, id);
                if (nameExists)
                {
                    return BadRequest(new { message = "An intercom system with this name already exists in the specified building." });
                }

                var updatedIntercom = await _intercomService.UpdateIntercomAsync(id, updateIntercomDto, userId);
                
                if (updatedIntercom == null)
                {
                    return NotFound(new { message = "Intercom system not found or you don't have access to it." });
                }
                
                return Ok(updatedIntercom);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the intercom system.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system to delete.</param>
        /// <returns>A success message if the deletion was successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIntercom(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var deleted = await _intercomService.DeleteIntercomAsync(id, userId);
                
                if (!deleted)
                {
                    return NotFound(new { message = "Intercom system not found or you don't have access to it." });
                }
                
                return Ok(new { message = "Intercom system deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the intercom system.", details = ex.Message });
            }
        }

        /// <summary>
        /// Toggles the active status of an intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <returns>The updated intercom system.</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<IntercomDto>> ToggleIntercomStatus(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var updatedIntercom = await _intercomService.ToggleIntercomStatusAsync(id, userId);
                
                if (updatedIntercom == null)
                {
                    return NotFound(new { message = "Intercom system not found or you don't have access to it." });
                }
                
                return Ok(updatedIntercom);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while toggling the intercom status.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the service date of an intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <param name="serviceDate">The new service date.</param>
        /// <returns>The updated intercom system.</returns>
        [HttpPatch("{id}/service-date")]
        public async Task<ActionResult<IntercomDto>> UpdateServiceDate(int id, [FromBody] DateTime serviceDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }
                
                var updatedIntercom = await _intercomService.UpdateServiceDateAsync(id, serviceDate, userId);
                
                if (updatedIntercom == null)
                {
                    return NotFound(new { message = "Intercom system not found or you don't have access to it." });
                }
                
                return Ok(updatedIntercom);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the service date.", details = ex.Message });
            }
        }

        /// <summary>
        /// Checks if an intercom system name exists in a specific building.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="buildingId">The building ID to check within.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists, false otherwise.</returns>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> CheckIntercomNameExists([FromQuery] string name, [FromQuery] int buildingId, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Name parameter is required." });
                }

                var exists = await _intercomService.IntercomExistsInBuildingAsync(name, buildingId, excludeId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking intercom name existence.", details = ex.Message });
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
                
                var hasAccess = await _intercomService.ValidateBuildingAccessAsync(buildingId, userId);
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
    }
}
