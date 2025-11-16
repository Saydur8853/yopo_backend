using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Services;
using YopoBackend.DTOs;

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
        public async Task<IActionResult> Get([FromQuery] int? buildingId = null, [FromQuery] int? intercomId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _service.GetAccessCodesAsync(buildingId, intercomId, page, pageSize);
            var response = new PaginatedResponse<AccessCodeDTO>(items, total, page, pageSize);
            return Ok(response);
        }

        // POST: create an access code (QR or PIN). If IntercomId is omitted, the code applies to all intercoms in the building
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccessCodeDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });
            var res = await _service.CreateAccessCodeAsync(dto, GetCurrentUserId());
            if (!res.Success) return BadRequest(new { success = false, message = res.Message, data = (object?)null });
            return StatusCode(201, new { success = true, message = res.Message, data = res.Code });
        }

        // PUT: update an existing access code (only mutable fields like code, label, expiry)
        /// <summary>
        /// Updates an existing access code's mutable fields (label, PIN value, expiry).
        /// </summary>
        /// <remarks>
        /// Role behavior:
        /// - <b>Tenant</b>: may update only codes they created.
        /// - <b>Property Manager / FrontDesk / other building users</b>: may update codes in buildings they have access to.
        /// - <b>SuperAdmin</b>: may update any code.
        /// 
        /// Notes:
        /// - If <c>code</c> is provided, the PIN value is re-hashed and replaced.
        /// - If <c>codeUser</c> is provided, the user-facing label is updated (can be set to null).
        /// - If <c>expiresAt</c> is provided, it must be in the future; null means no expiry.
        /// - <c>buildingId</c> and <c>intercomId</c> of the code cannot be changed via this endpoint.
        /// </remarks>
        /// <param name="id">Access code ID.</param>
        /// <param name="dto">Fields to update for the access code.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccessCodeDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Validation failed.", data = ModelState });
            var res = await _service.UpdateAccessCodeAsync(id, dto, GetCurrentUserId());
            if (!res.Success)
            {
                if (res.Message == "Not found.")
                    return NotFound(new { success = false, message = res.Message, data = (object?)null });

                return BadRequest(new { success = false, message = res.Message, data = (object?)null });
            }

            return Ok(new { success = true, message = res.Message, data = res.Code });
        }

        /// <summary>
        /// Deactivates an access code (soft delete).
        /// </summary>
        /// <remarks>
        /// Role behavior:
        /// - <b>Tenant</b>: may deactivate only codes they created.
        /// - <b>Property Manager / FrontDesk / other building users</b>: may deactivate codes in buildings they have access to.
        /// - <b>SuperAdmin</b>: may deactivate any code.
        /// </remarks>
        /// <param name="id">Access code ID.</param>
        /// <returns>Operation result (success flag and message).</returns>
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var res = await _service.DeactivateAccessCodeAsync(id, GetCurrentUserId());
            if (!res.Success) return BadRequest(new { success = false, message = res.Message });
            return Ok(new { success = true, message = res.Message });
        }

        /// <summary>
        /// Permanently deletes an access code.
        /// </summary>
        /// <remarks>
        /// Role behavior:
        /// - <b>Tenant</b>: may delete only codes they created.
        /// - <b>Property Manager / FrontDesk / other building users</b>: may delete codes in buildings they have access to.
        /// - <b>SuperAdmin</b>: may delete any code.
        /// </remarks>
        /// <param name="id">Access code ID.</param>
        /// <returns>Operation result (success flag and message).</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _service.DeleteAccessCodeAsync(id, GetCurrentUserId());
            if (!res.Success)
            {
                if (res.Message == "Not found.")
                    return NotFound(new { success = false, message = res.Message });

                return BadRequest(new { success = false, message = res.Message });
            }

            return Ok(new { success = true, message = res.Message });
        }
    }
}
