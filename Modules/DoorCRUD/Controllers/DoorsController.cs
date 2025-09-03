using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Modules.DoorCRUD.DTOs;
using YopoBackend.Modules.DoorCRUD.Services;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using System.Security.Claims;

namespace YopoBackend.Modules.DoorCRUD.Controllers
{
    /// <summary>
    /// Controller for Door CRUD operations.
    /// Module ID: 12 (DoorCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.DOOR_MODULE_ID)]
    public class DoorsController : ControllerBase
    {
        private readonly IDoorService _doorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorsController"/> class.
        /// </summary>
        /// <param name="doorService">The door service.</param>
        public DoorsController(IDoorService doorService)
        {
            _doorService = doorService;
        }

        /// <summary>
        /// Gets all doors based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all doors the current user has access to.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoorDto>>> GetAllDoors()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var doors = await _doorService.GetAllDoorsAsync(userId);
                return Ok(doors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active doors based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all active doors the current user has access to.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<DoorDto>>> GetActiveDoors()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var doors = await _doorService.GetActiveDoorsAsync(userId);
                return Ok(doors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets doors by building ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <returns>A list of doors for the specified building if the user has access.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<IEnumerable<DoorDto>>> GetDoorsByBuilding(int buildingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var doors = await _doorService.GetDoorsByBuildingIdAsync(buildingId, userId);
                return Ok(doors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a door by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door.</param>
        /// <returns>The door with the specified ID if the user has access to it.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DoorDto>> GetDoor(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var door = await _doorService.GetDoorByIdAsync(id, userId);
                if (door == null)
                {
                    return NotFound($"Door with ID {id} not found or you don't have access to it.");
                }

                return Ok(door);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new door.
        /// </summary>
        /// <param name="createDoorDto">The data for creating the door.</param>
        /// <returns>The created door.</returns>
        [HttpPost]
        public async Task<ActionResult<DoorDto>> CreateDoor(CreateDoorDto createDoorDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Check if door already exists in the same building with same type and location
                if (await _doorService.DoorExistsAsync(createDoorDto.BuildingId, createDoorDto.Type, createDoorDto.Location))
                {
                    return BadRequest($"A door with type '{createDoorDto.Type}' and location '{createDoorDto.Location}' already exists in this building.");
                }

                var createdDoor = await _doorService.CreateDoorAsync(createDoorDto, userId);
                return CreatedAtAction(nameof(GetDoor), new { id = createdDoor.DoorId }, createdDoor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing door, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door to update.</param>
        /// <param name="updateDoorDto">The data for updating the door.</param>
        /// <returns>The updated door.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<DoorDto>> UpdateDoor(int id, UpdateDoorDto updateDoorDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Check if door already exists in the same building with same type and location (excluding current door)
                if (await _doorService.DoorExistsAsync(updateDoorDto.BuildingId, updateDoorDto.Type, updateDoorDto.Location, id))
                {
                    return BadRequest($"A door with type '{updateDoorDto.Type}' and location '{updateDoorDto.Location}' already exists in this building.");
                }

                var updatedDoor = await _doorService.UpdateDoorAsync(id, updateDoorDto, userId);
                if (updatedDoor == null)
                {
                    return NotFound($"Door with ID {id} not found or you don't have access to modify it.");
                }

                return Ok(updatedDoor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a door, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door to delete.</param>
        /// <returns>A confirmation of deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDoor(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var result = await _doorService.DeleteDoorAsync(id, userId);
                if (!result)
                {
                    return NotFound($"Door with ID {id} not found or you don't have access to delete it.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a door with the same type and location already exists in a building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="type">The door type.</param>
        /// <param name="location">The door location.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the type/location combination is available, false if it already exists.</returns>
        [HttpGet("check-door")]
        public async Task<ActionResult<bool>> CheckDoor([FromQuery] int buildingId, [FromQuery] string type, [FromQuery] string? location = null, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    return BadRequest("Type parameter is required.");
                }

                var exists = await _doorService.DoorExistsAsync(buildingId, type, location, excludeId);
                return Ok(new { available = !exists, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
