using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.UnitCRUD.DTOs;
using YopoBackend.DTOs;
using YopoBackend.Modules.UnitCRUD.Services;

namespace YopoBackend.Modules.UnitCRUD.Controllers
{
    /// <summary>
    /// Controller for managing units under floors and buildings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("06-Units")]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;
        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        /// <summary>
        /// Retrieve all units under a specific floor.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDTO<UnitResponseDTO>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResponseDTO<UnitResponseDTO>>> GetUnits([FromQuery] int? floorId, [FromQuery] int? buildingId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var (units, totalRecords) = await _unitService.GetUnitsAsync(floorId, buildingId, pageNumber, pageSize);

            var pagedResponse = new PagedResponseDTO<UnitResponseDTO>(units, pageNumber, pageSize, totalRecords);

            return Ok(pagedResponse);
        }

        /// <summary>
        /// Add a new unit.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

            var result = await _unitService.CreateUnitAsync(dto);
            if (!result.Success)
            {
                // 404 if reference missing; else 400 for business rule violations
                var isNotFound = result.Message.Contains("not found") || result.Message.Contains("does not belong");
                if (isNotFound)
                    return NotFound(new { success = false, message = result.Message, data = (object?)null });
                return BadRequest(new { success = false, message = result.Message, data = (object?)null });
            }

            return CreatedAtAction(nameof(GetUnits), new { buildingId = result.Data!.BuildingId, floorId = result.Data!.FloorId }, new { success = true, message = "Unit created successfully.", data = result.Data });
        }

        /// <summary>
        /// Edit an existing unit's details.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

            var result = await _unitService.UpdateUnitAsync(id, dto);
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message, data = (object?)null });

            return Ok(new { success = true, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Delete a unit record.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var result = await _unitService.DeleteUnitAsync(id);
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message, data = (object?)null });

            return Ok(new { success = true, message = result.Message, data = (object?)null });
        }
    }
}