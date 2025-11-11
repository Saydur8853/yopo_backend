using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Services;

namespace YopoBackend.Modules.IntercomAccess.Controllers
{
    [ApiController]
    [Route("api/access-codes")]
    [Produces("application/json")]
    [Authorize]
    [Tags("10-Intercom Access")]
    public class AccessCodesController : ControllerBase
    {
        private readonly IIntercomAccessService _service;
        public AccessCodesController(IIntercomAccessService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId)) throw new UnauthorizedAccessException("Invalid token.");
            return currentUserId;
        }

        // GET: list access codes, optionally filtered by building and/or intercom
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? buildingId = null, [FromQuery] int? intercomId = null)
        {
            var list = await _service.GetAccessCodesAsync(buildingId, intercomId);
            return Ok(list);
        }

        // POST: create an access code (QR or PIN). If IntercomId is omitted, the code applies to all intercoms in the building
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccessCodeDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.CreateAccessCodeAsync(dto, GetCurrentUserId());
            if (!res.Success) return BadRequest(new { message = res.Message });
            return StatusCode(201, res.Code);
        }

        // PATCH: deactivate an access code (soft delete)
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var res = await _service.DeactivateAccessCodeAsync(id, GetCurrentUserId());
            if (!res.Success) return BadRequest(new { message = res.Message });
            return Ok(new { message = res.Message });
        }
    }
}
