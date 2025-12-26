using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Auth;
using YopoBackend.Modules.TermsConditionsCRUD.DTOs;
using YopoBackend.Modules.TermsConditionsCRUD.Services;

namespace YopoBackend.Modules.TermsConditionsCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("15-TermsAndConditions")]
    [Authorize]
    public class TermsAndConditionsController : ControllerBase
    {
        private readonly ITermsAndConditionsService _termsService;

        public TermsAndConditionsController(ITermsAndConditionsService termsService)
        {
            _termsService = termsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TermsAndConditionResponseDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<TermsAndConditionResponseDTO>>> GetAll()
        {
            var authResult = EnsureSuperAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            var items = await _termsService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TermsAndConditionResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TermsAndConditionResponseDTO>> GetById(int id)
        {
            var authResult = EnsureSuperAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            var item = await _termsService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Terms and Conditions with ID {id} not found." });
            }

            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TermsAndConditionResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TermsAndConditionResponseDTO>> Create([FromBody] CreateTermsAndConditionDTO dto)
        {
            var authResult = EnsureSuperAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _termsService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.TermsAndConditionId }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TermsAndConditionResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TermsAndConditionResponseDTO>> Update(int id, [FromBody] UpdateTermsAndConditionDTO dto)
        {
            var authResult = EnsureSuperAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _termsService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new { message = $"Terms and Conditions with ID {id} not found." });
            }

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = EnsureSuperAdmin();
            if (authResult != null)
            {
                return authResult;
            }

            var deleted = await _termsService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Terms and Conditions with ID {id} not found." });
            }

            return NoContent();
        }

        private ActionResult? EnsureSuperAdmin()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.Equals(role, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "Only Super Admin can manage Terms and Conditions." });
            }

            return null;
        }
    }
}
