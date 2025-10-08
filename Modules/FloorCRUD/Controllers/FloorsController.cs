using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.FloorCRUD.DTOs;
using YopoBackend.Modules.FloorCRUD.Services;

namespace YopoBackend.Modules.FloorCRUD.Controllers
{
    /// <summary>
    /// Controller for managing floors under buildings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("05-Floors")]
    public class FloorsController : ControllerBase
    {
        private readonly IFloorService _floorService;

        public FloorsController(IFloorService floorService)
        {
            _floorService = floorService;
        }

        /// <summary>
        /// Retrieves all floors under a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<FloorResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<FloorResponseDTO>>> GetFloors([FromQuery] int? buildingId)
        {
            if (!buildingId.HasValue)
            {
                return BadRequest(new { message = "Query parameter 'buildingId' is required." });
            }

            var floors = await _floorService.GetFloorsByBuildingAsync(buildingId.Value);
            if (floors.Count == 0)
            {
                // If building doesn't exist or no floors, we return 404 for non-existent building
                // Here we can't differentiate easily without extra call; keep behavior simple
                return Ok(floors);
            }

            return Ok(floors);
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

            var updated = await _floorService.UpdateFloorAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Floor with ID {id} not found." });

            return Ok(updated);
        }

        /// <summary>
        /// Deletes a specific floor record.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var deleted = await _floorService.DeleteFloorAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Floor with ID {id} not found." });

            return NoContent();
        }
    }
}