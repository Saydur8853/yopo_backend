using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.DTOs;
using YopoBackend.Modules.VerifyIdentity.DTOs;
using YopoBackend.Modules.VerifyIdentity.Models;
using YopoBackend.Modules.VerifyIdentity.Services;

namespace YopoBackend.Modules.VerifyIdentity.Controllers
{
    [ApiController]
    [Route("api/verify_identity")]
    [Produces("application/json")]
    [Tags("20-VerifyIdentity")]
    [Authorize]
    public class VerifyIdentityController : ControllerBase
    {
        private readonly IVerifyIdentityService _verifyIdentityService;

        public VerifyIdentityController(IVerifyIdentityService verifyIdentityService)
        {
            _verifyIdentityService = verifyIdentityService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<IdentityVerificationResponseDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PaginatedResponse<IdentityVerificationResponseDTO>>> GetRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] string? status = null)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            try
            {
                var (requests, totalRecords) = await _verifyIdentityService.GetRequestsAsync(
                    currentUserId.Value,
                    GetUserRole(),
                    buildingId,
                    tenantId,
                    status,
                    page,
                    pageSize);

                var response = new PaginatedResponse<IdentityVerificationResponseDTO>(requests, totalRecords, page, pageSize);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("document-types")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public ActionResult<List<string>> GetDocumentTypes()
        {
            return Ok(IdentityVerificationDocumentTypes.Values);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IdentityVerificationResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IdentityVerificationResponseDTO>> CreateRequest([FromBody] CreateIdentityVerificationDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _verifyIdentityService.CreateRequestAsync(currentUserId.Value, GetUserRole(), dto);
                return CreatedAtAction(nameof(GetRequests), new { page = 1, pageSize = 10 }, created);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(IdentityVerificationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IdentityVerificationResponseDTO>> UpdateStatus(int id, [FromBody] UpdateIdentityVerificationStatusDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _verifyIdentityService.UpdateStatusAsync(id, currentUserId.Value, GetUserRole(), dto.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IdentityVerificationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IdentityVerificationResponseDTO>> UpdateDocuments(int id, [FromBody] UpdateIdentityVerificationDocumentsDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _verifyIdentityService.UpdateDocumentsAsync(id, currentUserId.Value, GetUserRole(), dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            try
            {
                await _verifyIdentityService.DeleteRequestAsync(id, currentUserId.Value, GetUserRole());
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        private string GetUserRole()
        {
            return (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
        }
    }
}
