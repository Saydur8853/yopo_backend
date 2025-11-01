using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.DTOs;
using YopoBackend.Modules.IntercomCRUD.DTOs;
using YopoBackend.Modules.IntercomCRUD.Services;

namespace YopoBackend.Modules.IntercomCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [Tags("09-Intercoms")]
    public class IntercomsController : ControllerBase
    {
        private readonly IIntercomService _service;
        public IntercomsController(IIntercomService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<IntercomResponseDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PaginatedResponse<IntercomResponseDTO>>> GetIntercoms(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? unitId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isInstalled = null,
            [FromQuery] string? intercomType = null,
            [FromQuery] string? operatingSystem = null,
            [FromQuery] bool? hasCCTV = null,
            [FromQuery] bool? hasPinPad = null,
            [FromQuery] DateTime? installedFrom = null,
            [FromQuery] DateTime? installedTo = null,
            [FromQuery] DateTime? serviceFrom = null,
            [FromQuery] DateTime? serviceTo = null,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null,
            [FromQuery] string? color = null,
            [FromQuery] string? model = null
        )
        {
            try
            {
                var (items, total) = await _service.GetIntercomsAsync(page, pageSize, searchTerm, customerId, buildingId, unitId, isActive, isInstalled, intercomType, operatingSystem, hasCCTV, hasPinPad, installedFrom, installedTo, serviceFrom, serviceTo, priceMin, priceMax, color, model);
                var response = new PaginatedResponse<IntercomResponseDTO>(items, total, page, pageSize);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateIntercomDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

                var result = await _service.CreateIntercomAsync(dto);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message, data = (object?)null });

                return CreatedAtAction(nameof(GetIntercoms), new { intercomId = result.Data!.IntercomId }, new { success = true, message = result.Message, data = result.Data });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIntercomDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

                var result = await _service.UpdateIntercomAsync(id, dto);
                if (!result.Success)
                    return NotFound(new { success = false, message = result.Message, data = (object?)null });

                return Ok(new { success = true, message = result.Message, data = result.Data });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteIntercomAsync(id);
                if (!result.Success)
                    return NotFound(new { success = false, message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpPut("{id}/assign")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignToUnit(int id, [FromBody] AssignIntercomDTO dto)
        {
            try
            {
                var result = await _service.AssignToUnitAsync(id, dto.UnitId);
                if (!result.Success)
                    return NotFound(new { success = false, message = result.Message, data = (object?)null });

                return Ok(new { success = true, message = result.Message, data = result.Data });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }
    }
}