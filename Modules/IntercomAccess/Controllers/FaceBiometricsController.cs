using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Services;

namespace YopoBackend.Modules.IntercomAccess.Controllers
{
    [ApiController]
    [Route("api/face-biometric")]
    [Produces("application/json")]
    [Authorize]
    [Tags("09-Intercom Access")]
    public class FaceBiometricsController : ControllerBase
    {
        private readonly IFaceBiometricService _service;

        public FaceBiometricsController(IFaceBiometricService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId)) throw new UnauthorizedAccessException("Invalid token.");
            return currentUserId;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var record = await _service.GetAsync(GetCurrentUserId());
            if (record == null)
            {
                return NotFound(new { message = "Face data not found." });
            }

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FaceBiometricUploadDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.UpsertAsync(GetCurrentUserId(), dto);
                if (!result.Success || result.Record == null)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message, files = result.Record.Files });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Storage or internal error: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FaceBiometricUploadDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.UpsertAsync(GetCurrentUserId(), dto);
                if (!result.Success || result.Record == null)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message, files = result.Record.Files });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Storage or internal error: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var result = await _service.DeleteAsync(GetCurrentUserId());
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}
