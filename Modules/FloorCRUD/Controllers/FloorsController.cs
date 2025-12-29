using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Modules.FloorCRUD.DTOs;
using YopoBackend.DTOs;
using YopoBackend.Modules.FloorCRUD.Services;
using YopoBackend.Data;
using YopoBackend.Constants;

namespace YopoBackend.Modules.FloorCRUD.Controllers
{
    /// <summary>
    /// Controller for managing floors under buildings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("04-Floors")]
    public class FloorsController : ControllerBase
    {
        private readonly IFloorService _floorService;
        private readonly ApplicationDbContext _context;

        public FloorsController(IFloorService floorService, ApplicationDbContext context)
        {
            _floorService = floorService;
            _context = context;
        }

        /// <summary>
        /// Retrieves all floors under a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<FloorResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaginatedResponse<FloorResponseDTO>>> GetFloors([FromQuery] int? buildingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();

            if (!buildingId.HasValue && !currentUserId.HasValue)
            {
                return BadRequest(new { message = "Query parameter 'buildingId' is required when not authenticated." });
            }

            var (floors, totalRecords) = await _floorService.GetFloorsAsync(buildingId, currentUserId, page, pageSize);

            var paginatedResponse = new PaginatedResponse<FloorResponseDTO>(floors, totalRecords, page, pageSize);

            return Ok(paginatedResponse);
        }

        /// <summary>
        /// Adds a new floor record.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FloorResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FloorResponseDTO>> CreateFloor([FromBody] CreateFloorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            // Enforce Property Manager only
            var isPm = await IsPropertyManagerAsync(currentUserId.Value);
            if (!isPm)
                return StatusCode(403, new { message = "Sorry! Floor create privilege is only for Property Managers." });

            // Enforce Building module permission
            var hasBuildingModule = await HasModulePermissionAsync(currentUserId.Value, ModuleConstants.BUILDING_MODULE_ID);
            if (!hasBuildingModule)
                return StatusCode(403, new { message = "Sorry! You need Building module permission to create floors." });

            var created = await _floorService.CreateFloorAsync(dto);
            if (created == null)
            {
                return NotFound(new { message = $"Building with ID {dto.BuildingId} not found." });
            }

            return CreatedAtAction(nameof(GetFloors), new { buildingId = created.BuildingId }, created);
        }

        /// <summary>
        /// Updates an existing floor's details.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FloorResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FloorResponseDTO>> UpdateFloor(int id, [FromBody] UpdateFloorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var isPm = await IsPropertyManagerAsync(currentUserId.Value);
            if (!isPm)
                return StatusCode(403, new { message = "Sorry! Floor update privilege is only for Property Managers." });

            var hasBuildingModule = await HasModulePermissionAsync(currentUserId.Value, ModuleConstants.BUILDING_MODULE_ID);
            if (!hasBuildingModule)
                return StatusCode(403, new { message = "Sorry! You need Building module permission to update floors." });

            var updated = await _floorService.UpdateFloorAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Floor with ID {id} not found." });

            return Ok(updated);
        }

        /// <summary>
        /// Deletes a specific floor record.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "User authentication required." });

            var isPm = await IsPropertyManagerAsync(currentUserId.Value);
            if (!isPm)
                return StatusCode(403, new { message = "Sorry! Floor delete privilege is only for Property Managers." });

            var hasBuildingModule = await HasModulePermissionAsync(currentUserId.Value, ModuleConstants.BUILDING_MODULE_ID);
            if (!hasBuildingModule)
                return StatusCode(403, new { message = "Sorry! You need Building module permission to delete floors." });

            var (deleted, floorName, buildingName) = await _floorService.DeleteFloorAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Floor with ID {id} not found." });

            return Ok(new { message = $"Successfully deleted '{floorName}' from '{buildingName}'" });
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

        private async Task<bool> IsPropertyManagerAsync(int userId)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == userId && u.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && u.IsActive);
        }

        private async Task<bool> HasModulePermissionAsync(int userId, int moduleId)
        {
            // Check if the user's user type has the module permission active
            return await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.UserTypes,
                      u => u.UserTypeId,
                      ut => ut.Id,
                      (u, ut) => ut)
                .Join(_context.UserTypeModulePermissions.Where(p => p.IsActive),
                      ut => ut.Id,
                      p => p.UserTypeId,
                      (ut, p) => p)
                .AnyAsync(p => p.ModuleId == moduleId);
        }
    }
}
