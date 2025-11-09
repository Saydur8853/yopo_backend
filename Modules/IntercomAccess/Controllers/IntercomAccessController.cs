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

        [HttpPost("users/{userId:int}/pin")] // SuperAdmin sets or resets any user's pin (requires master pin if not self)
        public async Task<IActionResult> SetUserPin(int intercomId, int userId, [FromBody] SetUserPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.SetOrUpdateUserPinAsync(intercomId, userId, dto.Pin, GetCurrentUserId(), dto.MasterPin);
            if (!res.Success)
            {
                if (res.Message.Contains("Not allowed", StringComparison.OrdinalIgnoreCase)) return StatusCode(403, res);
                if (res.Message.Contains("Only Super Admin", StringComparison.OrdinalIgnoreCase)) return StatusCode(403, res);
                return BadRequest(res);
            }
            return Ok(res);
        }

        [HttpPut("me/pin")] // user updates own pin for this intercom
        public async Task<IActionResult> UpdateMyPin(int intercomId, [FromBody] UpdateOwnPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.UpdateOwnUserPinAsync(intercomId, GetCurrentUserId(), dto.NewPin, dto.OldPin);
            var code = res.Success ? 200 : 400;
            return StatusCode(code, res);
        }

        [HttpPost("temporary-pin")] // tenant creates temporary pin
        [Authorize(Roles = Roles.Tenant)]
        public async Task<IActionResult> CreateTemporaryPin(int intercomId, [FromBody] CreateTemporaryPinDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var expiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddMinutes(dto.ValidForMinutes ?? 30);
            var res = await _service.CreateTemporaryPinAsync(intercomId, GetCurrentUserId(), dto.Pin, expiresAt, dto.MaxUses);
            var code = res.Success ? 201 : 400;
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

        // History: Access logs for an intercom (paginated)
        [HttpGet("logs")]
        public async Task<IActionResult> GetAccessLogs(
            int intercomId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] bool? success = null,
            [FromQuery] string? credentialType = null,
            [FromQuery] int? userId = null)
        {
            if (!User.IsInRole(YopoBackend.Auth.Roles.SuperAdmin))
                return StatusCode(403, new { message = "Only Super Admin can view intercom access logs." });

            var (items, total) = await _service.GetAccessLogsAsync(intercomId, page, pageSize, from, to, success, credentialType, userId);
            var response = new YopoBackend.DTOs.PaginatedResponse<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO>(items, total, page, pageSize);
            return Ok(response);
        }

        // History: Temporary pin usages for an intercom (paginated)
        [HttpGet("temporary-usages")]
        public async Task<IActionResult> GetTemporaryUsages(
            int intercomId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int? temporaryPinId = null)
        {
            if (!User.IsInRole(YopoBackend.Auth.Roles.SuperAdmin))
                return StatusCode(403, new { message = "Only Super Admin can view intercom temporary PIN usages." });

            var (items, total) = await _service.GetTemporaryUsagesAsync(intercomId, page, pageSize, from, to, temporaryPinId);
            var response = new YopoBackend.DTOs.PaginatedResponse<YopoBackend.Modules.IntercomAccess.DTOs.TemporaryPinUsageDTO>(items, total, page, pageSize);
            return Ok(response);
        }
    }
}