using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.DTOs;
using YopoBackend.Modules.ThreadSocial.DTOs;
using YopoBackend.Modules.ThreadSocial.Services;

namespace YopoBackend.Modules.ThreadSocial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThreadSocialController : ControllerBase
    {
        private readonly IThreadSocialService _threadSocialService;

        public ThreadSocialController(IThreadSocialService threadSocialService)
        {
            _threadSocialService = threadSocialService;
        }

        [HttpPost("posts")]
        public async Task<ActionResult<ThreadPostResponseDTO>> CreatePost([FromBody] CreateThreadPostDTO dto)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                var post = await _threadSocialService.CreatePostAsync(userId, userRole, dto);
                return Ok(post);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("posts")]
        public async Task<ActionResult<PaginatedResponse<ThreadPostResponseDTO>>> GetPosts([FromQuery] int? buildingId = null, [FromQuery] int? authorId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                var (posts, totalRecords) = await _threadSocialService.GetPostsAsync(userId, userRole, buildingId, authorId, page, pageSize);
                var response = new PaginatedResponse<ThreadPostResponseDTO>(posts, totalRecords, page, pageSize);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("posts/{id}")]
        public async Task<ActionResult<ThreadPostResponseDTO>> UpdatePost(int id, [FromBody] UpdateThreadPostDTO dto)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                var post = await _threadSocialService.UpdatePostAsync(id, userId, userRole, dto);
                return Ok(post);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("posts/{id}/status")]
        public async Task<ActionResult<ThreadPostResponseDTO>> UpdatePostStatus(int id, [FromBody] UpdateThreadPostStatusDTO dto)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                var post = await _threadSocialService.UpdatePostStatusAsync(id, userId, userRole, dto.IsActive);
                return Ok(post);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("posts/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                await _threadSocialService.DeletePostAsync(id, userId, userRole);
                return Ok(new { message = "Post deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("comments")]
        public async Task<ActionResult<ThreadCommentResponseDTO>> CreateComment([FromBody] CreateThreadCommentDTO dto)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", data = ModelState });
            }

            try
            {
                var comment = await _threadSocialService.CreateCommentAsync(userId, userRole, dto);
                return Ok(comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("comments")]
        public async Task<ActionResult<PaginatedResponse<ThreadCommentResponseDTO>>> GetComments([FromQuery] int postId, [FromQuery] int? authorId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            if (postId <= 0)
            {
                return BadRequest(new { message = "postId is required." });
            }

            try
            {
                var (comments, totalRecords) = await _threadSocialService.GetCommentsAsync(userId, userRole, postId, authorId, page, pageSize);
                var response = new PaginatedResponse<ThreadCommentResponseDTO>(comments, totalRecords, page, pageSize);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("comments/{id}")]
        public async Task<ActionResult<ThreadCommentResponseDTO>> UpdateComment(int id, [FromBody] UpdateThreadCommentDTO dto)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", data = ModelState });
            }

            try
            {
                var comment = await _threadSocialService.UpdateCommentAsync(id, userId, userRole, dto);
                return Ok(comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            try
            {
                await _threadSocialService.DeleteCommentAsync(id, userId, userRole);
                return Ok(new { message = "Comment deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetUserRole()
        {
            return (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
        }
    }
}
