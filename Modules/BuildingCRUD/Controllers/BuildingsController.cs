using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Services;

namespace YopoBackend.Modules.BuildingCRUD.Controllers
{
    /// <summary>
    /// Controller for Building CRUD operations.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
        /// Gets all buildings.
        /// </summary>
        /// <returns>A list of all buildings.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BuildingDto>>> GetAllBuildings()
        {
            try
            {
                var buildings = await _buildingService.GetAllBuildingsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active buildings.
        /// </summary>
        /// <returns>A list of all active buildings.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BuildingDto>>> GetActiveBuildings()
        {
            try
            {
                var buildings = await _buildingService.GetActiveBuildingsAsync();
                return Ok(buildings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a building by ID.
        /// </summary>
        /// <param name="id">The ID of the building.</param>
        /// <returns>The building with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingDto>> GetBuilding(int id)
        {
            try
            {
                var building = await _buildingService.GetBuildingByIdAsync(id);
                if (building == null)
                {
                    return NotFound($"Building with ID {id} not found.");
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
                // Check if building name already exists
                if (await _buildingService.BuildingExistsAsync(createBuildingDto.Name))
                {
                    return BadRequest($"A building with the name '{createBuildingDto.Name}' already exists.");
                }

                var createdBuilding = await _buildingService.CreateBuildingAsync(createBuildingDto);
                return CreatedAtAction(nameof(GetBuilding), new { id = createdBuilding.Id }, createdBuilding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing building.
        /// </summary>
        /// <param name="id">The ID of the building to update.</param>
        /// <param name="updateBuildingDto">The data for updating the building.</param>
        /// <returns>The updated building.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<BuildingDto>> UpdateBuilding(int id, UpdateBuildingDto updateBuildingDto)
        {
            try
            {
                // Check if building name already exists (excluding current building)
                if (await _buildingService.BuildingExistsAsync(updateBuildingDto.Name, id))
                {
                    return BadRequest($"A building with the name '{updateBuildingDto.Name}' already exists.");
                }

                var updatedBuilding = await _buildingService.UpdateBuildingAsync(id, updateBuildingDto);
                if (updatedBuilding == null)
                {
                    return NotFound($"Building with ID {id} not found.");
                }

                return Ok(updatedBuilding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a building.
        /// </summary>
        /// <param name="id">The ID of the building to delete.</param>
        /// <returns>A confirmation of deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBuilding(int id)
        {
            try
            {
                var result = await _buildingService.DeleteBuildingAsync(id);
                if (!result)
                {
                    return NotFound($"Building with ID {id} not found.");
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
