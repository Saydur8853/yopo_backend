using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Services;
using System.Security.Claims;

namespace YopoBackend.Modules.BuildingCRUD.Controllers
{
    /// <summary>
    /// Controller for managing building operations.
    /// Module ID: 4 (BuildingCRUD)
    /// Data Access Control: PM (Super Admin sees all, Property Manager sees own customer buildings)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("04-Buildings")]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        /// <summary>
        /// Initializes a new instance of the BuildingsController class.
        /// </summary>
        /// <param name="buildingService">The building service.</param>
        public BuildingsController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        /// <summary>
        /// Gets all buildings with pagination and filtering based on user's access control.
        /// </summary>
        /// <param name="page">Page number (starting from 1). Default: 1</param>
        /// <param name="pageSize">Number of items per page. Default: 10</param>
        /// <param name="searchTerm">Optional search term for building name or address.</param>
        /// <param name="customerId">Optional filter by customer ID.</param>
        /// <param name="buildingId">Optional filter by specific building ID.</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <param name="hasGym">Optional filter by gym amenity.</param>
        /// <param name="hasSwimmingPool">Optional filter by swimming pool amenity.</param>
        /// <param name="hasSauna">Optional filter by sauna amenity.</param>
        /// <returns>Paginated list of buildings accessible to the current user.</returns>
        /// <response code="200">Returns the paginated list of buildings</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(BuildingListResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<BuildingListResponseDTO>> GetBuildings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? hasGym = null,
            [FromQuery] bool? hasSwimmingPool = null,
            [FromQuery] bool? hasSauna = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User authentication required." });
                }

                var result = await _buildingService.GetBuildingsAsync(
                    currentUserId.Value, page, pageSize, searchTerm, customerId, buildingId,
                    isActive, hasGym, hasSwimmingPool, hasSauna);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching buildings.", details = ex.Message });
            }
        }


        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="createBuildingDto">The building data to create. The CustomerId is automatically derived from the current user's token.</param>
        /// <returns>The created building information.</returns>
        /// <response code="201">Returns the created building</response>
        /// <response code="400">If the building data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to the specified customer</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(BuildingResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<BuildingResponseDTO>> CreateBuilding([FromBody] CreateBuildingDTO createBuildingDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User authentication required." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var building = await _buildingService.CreateBuildingAsync(createBuildingDto, currentUserId.Value);

                return CreatedAtAction(
                    nameof(GetBuildings),
                    new { buildingId = building.BuildingId },
                    building);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Show a friendly message specific to building creation privilege
                return StatusCode(403, new { message = "Sorry! Building create privilege is only for Property managers." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the building.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing building.
        /// </summary>
        /// <param name="id">The building ID to update.</param>
        /// <param name="updateBuildingDto">The updated building data.</param>
        /// <returns>The updated building information.</returns>
        /// <response code="200">Returns the updated building</response>
        /// <response code="400">If the building data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the building is not found or not accessible</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BuildingResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<BuildingResponseDTO>> UpdateBuilding(int id, [FromBody] UpdateBuildingDTO updateBuildingDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User authentication required." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var building = await _buildingService.UpdateBuildingAsync(id, updateBuildingDto, currentUserId.Value);

                if (building == null)
                {
                    return NotFound(new { message = $"Building with ID {id} not found or not accessible." });
                }

                return Ok(building);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the building.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a building.
        /// </summary>
        /// <param name="id">The building ID to delete.</param>
        /// <returns>Success confirmation if deleted.</returns>
        /// <response code="204">If the building was successfully deleted</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the building is not found or not accessible</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteBuilding(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new { message = "User authentication required." });
                }

                var success = await _buildingService.DeleteBuildingAsync(id, currentUserId.Value);

                if (!success)
                {
                    return NotFound(new { message = $"Building with ID {id} not found or not accessible." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the building.", details = ex.Message });
            }
        }




        /// <summary>
        /// Extracts the current user ID from the JWT token claims.
        /// </summary>
        /// <returns>The current user ID if authenticated, null otherwise.</returns>
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