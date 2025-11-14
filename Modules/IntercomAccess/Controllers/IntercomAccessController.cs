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
    [Tags("10-Intercom Access")] // next after 09-Intercoms
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

        [HttpPost("master-pin")] // SuperAdmin only
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> SetMasterPin(int intercomId, [FromBody] SetMasterPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.SetOrUpdateMasterPinAsync(intercomId, dto.Pin, GetCurrentUserId());
            var code = res.Success ? 200 : 400;
            return StatusCode(code, res);
        }


        [AllowAnonymous]
        [HttpPost("verify")] // endpoint a device can call to verify a pin
        public async Task<IActionResult> VerifyPin(int intercomId, [FromBody] VerifyPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();
            var res = await _service.VerifyPinAsync(intercomId, dto.Pin, ip, device);
            return Ok(res);
        }

    }
}
