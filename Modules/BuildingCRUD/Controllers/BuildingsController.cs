using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Services;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using System.Security.Claims;

namespace YopoBackend.Modules.BuildingCRUD.Controllers
{
    /// <summary>
    /// Controller for Building CRUD operations.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.BUILDING_MODULE_ID)]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingsController"/> class.
        /// </summary>
        /// <param name="buildingService">The building service.</param>
        public BuildingsController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        /// <summary>
        /// Gets all buildings based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all buildings the current user has access to.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BuildingDto>>> GetAllBuildings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var buildings = await _buildingService.GetAllBuildingsAsync(userId);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active buildings based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all active buildings the current user has access to.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BuildingDto>>> GetActiveBuildings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var buildings = await _buildingService.GetActiveBuildingsAsync(userId);
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a building by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building.</param>
        /// <returns>The building with the specified ID if the user has access to it.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingDto>> GetBuilding(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var building = await _buildingService.GetBuildingByIdAsync(id, userId);
                if (building == null)
                {
                    return NotFound($"Building with ID {id} not found or you don't have access to it.");
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="createBuildingDto">The data for creating the building.</param>
        /// <returns>The created building.</returns>
        [HttpPost]
        public async Task<ActionResult<BuildingDto>> CreateBuilding(CreateBuildingDto createBuildingDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Check if building name already exists
                if (await _buildingService.BuildingExistsAsync(createBuildingDto.Name))
                {
                    return BadRequest($"A building with the name '{createBuildingDto.Name}' already exists.");
                }

                var createdBuilding = await _buildingService.CreateBuildingAsync(createBuildingDto, userId);
                return CreatedAtAction(nameof(GetBuilding), new { id = createdBuilding.Id }, createdBuilding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing building, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building to update.</param>
        /// <param name="updateBuildingDto">The data for updating the building.</param>
        /// <returns>The updated building.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<BuildingDto>> UpdateBuilding(int id, UpdateBuildingDto updateBuildingDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Check if building name already exists (excluding current building)
                if (await _buildingService.BuildingExistsAsync(updateBuildingDto.Name, id))
                {
                    return BadRequest($"A building with the name '{updateBuildingDto.Name}' already exists.");
                }

                var updatedBuilding = await _buildingService.UpdateBuildingAsync(id, updateBuildingDto, userId);
                if (updatedBuilding == null)
                {
                    return NotFound($"Building with ID {id} not found or you don't have access to modify it.");
                }

                return Ok(updatedBuilding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a building, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building to delete.</param>
        /// <returns>A confirmation of deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBuilding(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var result = await _buildingService.DeleteBuildingAsync(id, userId);
                if (!result)
                {
                    return NotFound($"Building with ID {id} not found or you don't have access to delete it.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a building name is available.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name is available, false if it already exists.</returns>
        [HttpGet("check-name")]
        public async Task<ActionResult<bool>> CheckBuildingName([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name parameter is required.");
                }

                var exists = await _buildingService.BuildingExistsAsync(name, excludeId);
                return Ok(new { available = !exists, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
