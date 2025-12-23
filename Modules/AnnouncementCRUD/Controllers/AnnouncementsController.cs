using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.AnnouncementCRUD.DTOs;
using YopoBackend.Modules.AnnouncementCRUD.Services;

namespace YopoBackend.Modules.AnnouncementCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("13-Announcements")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AnnouncementListResponseDTO), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AnnouncementListResponseDTO>> GetAnnouncements(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? buildingId = null)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            var result = await _announcementService.GetAnnouncementsAsync(currentUserId.Value, page, pageSize, buildingId);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AnnouncementResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AnnouncementResponseDTO>> CreateAnnouncement([FromBody] CreateAnnouncementDTO dto)
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
                var created = await _announcementService.CreateAnnouncementAsync(dto, currentUserId.Value);
                return CreatedAtAction(nameof(GetAnnouncements), new { buildingId = created.BuildingId }, created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AnnouncementResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AnnouncementResponseDTO>> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementDTO dto)
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
                var updated = await _announcementService.UpdateAnnouncementAsync(id, dto, currentUserId.Value);
                if (updated == null)
                {
                    return NotFound(new { message = $"Announcement with ID {id} not found." });
                }

                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            try
            {
                var deleted = await _announcementService.DeleteAnnouncementAsync(id, currentUserId.Value);
                if (!deleted)
                {
                    return NotFound(new { message = $"Announcement with ID {id} not found." });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}
