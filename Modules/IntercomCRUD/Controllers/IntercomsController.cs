using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using YopoBackend.Auth;
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
                var (items, total) = await _service.GetIntercomsAsync(page, pageSize, searchTerm, customerId, buildingId, isActive, isInstalled, intercomType, operatingSystem, hasCCTV, hasPinPad, installedFrom, installedTo, serviceFrom, serviceTo, priceMin, priceMax, color, model);
                var response = new PaginatedResponse<IntercomResponseDTO>(items, total, page, pageSize);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.SuperAdmin)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreateIntercomDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

                var result = await _service.CreateIntercomAsync(dto);
                if (!result.Success)
                {
                    if (result.Message.StartsWith("Only Super Admins", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.StartsWith("You don't have permission", StringComparison.OrdinalIgnoreCase))
                        return StatusCode(403, new { success = false, message = result.Message, data = (object?)null });

                    return BadRequest(new { success = false, message = result.Message, data = (object?)null });
                }

                return CreatedAtAction(nameof(GetIntercoms), new { intercomId = result.Data!.IntercomId }, new { success = true, message = result.Message, data = result.Data });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.SuperAdmin)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIntercomDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });

                var result = await _service.UpdateIntercomAsync(id, dto);
                if (!result.Success)
                {
                    if (result.Message.StartsWith("Only Super Admins", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.StartsWith("You don't have permission", StringComparison.OrdinalIgnoreCase))
                        return StatusCode(403, new { success = false, message = result.Message, data = (object?)null });

                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { success = false, message = result.Message, data = (object?)null });

                    return BadRequest(new { success = false, message = result.Message, data = (object?)null });
                }

                return Ok(new { success = true, message = result.Message, data = result.Data });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.SuperAdmin)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteIntercomAsync(id);
                if (!result.Success)
                {
                    if (result.Message.StartsWith("Only Super Admins", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.StartsWith("You don't have permission", StringComparison.OrdinalIgnoreCase))
                        return StatusCode(403, new { success = false, message = result.Message });

                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { success = false, message = result.Message });

                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required." });
            }
        }

    }
}