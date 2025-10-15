using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.AmenityCRUD.DTOs;
using YopoBackend.DTOs;
using YopoBackend.Modules.AmenityCRUD.Services;

namespace YopoBackend.Modules.AmenityCRUD.Controllers
{
    /// <summary>
    /// Controller for managing building amenities and facilities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("07-Amenities")]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        /// <summary>
        /// Initializes a new instance of the AmenitiesController class.
        /// </summary>
        /// <param name="amenityService">The amenity service.</param>
        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        /// <summary>
        /// Retrieves all amenities belonging to a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID to filter amenities by.</param>
        /// <returns>List of amenities for the specified building.</returns>
        /// <response code="200">Returns the list of amenities</response>
        /// <response code="400">If buildingId is not provided</response>
        /// <response code="404">If building is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<AmenityResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaginatedResponse<AmenityResponseDTO>>> GetAmenities([FromQuery] int? buildingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!buildingId.HasValue)
            {
                return BadRequest(new { message = "Query parameter 'buildingId' is required." });
            }

            var (amenities, totalRecords) = await _amenityService.GetAmenitiesByBuildingAsync(buildingId.Value, page, pageSize);

            var paginatedResponse = new PaginatedResponse<AmenityResponseDTO>(amenities, totalRecords, page, pageSize);

            return Ok(paginatedResponse);
        }

        /// <summary>
        /// Adds a new amenity under a building.
        /// </summary>
        /// <param name="dto">The amenity creation data.</param>
        /// <returns>The created amenity information.</returns>
        /// <response code="201">Returns the created amenity</response>
        /// <response code="400">If the amenity data is invalid</response>
        /// <response code="404">If building or floor is not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });
            }

            var result = await _amenityService.CreateAmenityAsync(dto);
            if (!result.Success)
            {
                var isNotFound = result.Message.Contains("not found") || result.Message.Contains("does not belong");
                if (isNotFound)
                {
                    return NotFound(new { success = false, message = result.Message, data = (object?)null });
                }
                return BadRequest(new { success = false, message = result.Message, data = (object?)null });
            }

            return CreatedAtAction(
                nameof(GetAmenities),
                new { buildingId = result.Data!.BuildingId },
                new { success = true, message = result.Message, data = result.Data }
            );
        }

        /// <summary>
        /// Updates an existing amenity by its ID.
        /// </summary>
        /// <param name="id">The amenity ID to update.</param>
        /// <param name="dto">The update data.</param>
        /// <returns>The updated amenity information.</returns>
        /// <response code="200">Returns the updated amenity</response>
        /// <response code="400">If the update data is invalid</response>
        /// <response code="404">If amenity or floor is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAmenity(int id, [FromBody] UpdateAmenityDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });
            }

            var result = await _amenityService.UpdateAmenityAsync(id, dto);
            if (!result.Success)
            {
                var isNotFound = result.Message.Contains("not found") || result.Message.Contains("does not belong");
                if (isNotFound)
                {
                    return NotFound(new { success = false, message = result.Message, data = (object?)null });
                }
                return BadRequest(new { success = false, message = result.Message, data = (object?)null });
            }

            return Ok(new { success = true, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Deletes an amenity by its ID.
        /// </summary>
        /// <param name="id">The amenity ID to delete.</param>
        /// <returns>Success confirmation if deleted.</returns>
        /// <response code="204">If the amenity was successfully deleted</response>
        /// <response code="404">If amenity is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var result = await _amenityService.DeleteAmenityAsync(id);
            if (!result.Success)
            {
                return NotFound(new { success = false, message = result.Message, data = (object?)null });
            }

            return NoContent();
        }
    }
}