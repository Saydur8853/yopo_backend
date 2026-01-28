using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Auth;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Services;

namespace YopoBackend.Modules.IntercomAccess.Controllers
{
    [ApiController]
    [Route("api/intercoms/{intercomId:int}/access")]
    [Produces("application/json")]
    [Authorize]
    [Tags("09-Intercom Access")] // next after 08-Intercoms
    public class IntercomAccessController : ControllerBase
    {
        private readonly IIntercomAccessService _service;
        public IntercomAccessController(IIntercomAccessService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId)) throw new UnauthorizedAccessException("Invalid token.");
            return currentUserId;
        }

        /// <summary>
        /// Set or rotate the master PIN for an intercom.
        /// </summary>
        /// <remarks>
        /// Use this to establish the master PIN that can open the intercom and authorize admin resets.
        /// </remarks>
        /// <param name="intercomId">Target intercom ID.</param>
        /// <param name="dto">Master PIN payload.</param>
        [HttpPost("master-pin")] // SuperAdmin only
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> SetMasterPin(int intercomId, [FromBody] SetMasterPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.SetOrUpdateMasterPinAsync(intercomId, dto.Pin, GetCurrentUserId());
            var code = res.Success ? 200 : 400;
            return StatusCode(code, res);
        }


        /// <summary>
        /// Verify a PIN or face payload for an intercom and return an allow or deny decision.
        /// </summary>
        /// <remarks>
        /// Use this endpoint from an intercom device to validate a user, master, access code PIN, or face payload.
        /// </remarks>
        /// <param name="intercomId">Target intercom ID.</param>
        /// <param name="dto">PIN payload to verify.</param>
        [AllowAnonymous]
        [HttpPost("verify")] // endpoint a device can call to verify a pin or face
        public async Task<IActionResult> VerifyPin(int intercomId, [FromBody] VerifyAccessDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();
            var res = await _service.VerifyPinAsync(intercomId, dto, ip, device);
            return Ok(res);
        }

        /// <summary>
        /// Update the current user's own PIN.
        /// </summary>
        /// <remarks>
        /// This endpoint lets any authenticated user (including Super Admin) update their own PIN.
        /// If a PIN already exists, the old PIN must be provided and will be validated before updating.
        /// If no PIN exists yet, a new PIN will be created without requiring an old PIN.
        /// </remarks>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("pin/self")]
        [Authorize]
        public async Task<IActionResult> UpdateOwnPin(int intercomId, [FromBody] UpdateOwnPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var res = await _service.UpdateOwnUserPinAsync(intercomId, currentUserId, dto.NewPin, dto.OldPin);

            return StatusCode(res.Success ? 200 : 400, res);
        }

        /// <summary>
        /// Set or update a specific user's PIN by Super Admin.
        /// </summary>
        /// <remarks>
        /// This endpoint is primarily used for resetting a user's PIN (e.g., when they forget their old PIN).
        /// Non-SuperAdmin users may only change their own PIN. SuperAdmin may reset another user's PIN, but must provide a valid master pin.
        /// </remarks>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("users/{userId:int}/pin")] // reset pin endpoint
        [Authorize]
        public async Task<IActionResult> SetOrUpdateUserPin(int intercomId, int userId, [FromBody] SetUserPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var res = await _service.SetOrUpdateUserPinAsync(intercomId, userId, dto.Pin, currentUserId, dto.MasterPin);

            return StatusCode(res.Success ? 200 : 400, res);
        }

    }
}
