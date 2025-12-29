using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Constants;
using YopoBackend.DTOs;
using YopoBackend.Modules.CCTVCRUD.DTOs;
using YopoBackend.Modules.CCTVCRUD.Services;

namespace YopoBackend.Modules.CCTVCRUD.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("11-CCTV")]
    public class CCTVController : ControllerBase
    {
        private readonly ICCTVService _cctvService;

        public CCTVController(ICCTVService cctvService)
        {
            _cctvService = cctvService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CCTVResponseDto>>> GetAll(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? buildingId = null,
            [FromQuery] bool? isActive = null)
        {
            if (!IsAuthorized()) return Forbid();
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (items, totalCount) = await _cctvService.GetAllCCTVsAsync(userId, page, pageSize, searchTerm, buildingId, isActive);
            
            var response = new PaginatedResponse<CCTVResponseDto>(items.ToList(), totalCount, page, pageSize);
            return Ok(response);
        }



        [HttpPost]
        public async Task<ActionResult<CCTVResponseDto>> Create(CreateCCTVDto createDto)
        {
            if (!IsAuthorized()) return Forbid();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            try
            {
                var cctv = await _cctvService.CreateCCTVAsync(createDto, userId);
                return StatusCode(201, cctv);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CCTVResponseDto>> Update(int id, UpdateCCTVDto updateDto)
        {
            if (!IsAuthorized()) return Forbid();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cctv = await _cctvService.UpdateCCTVAsync(id, updateDto, userId);
            
            if (cctv == null) return NotFound();
            
            return Ok(cctv);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!IsAuthorized()) return Forbid();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _cctvService.DeleteCCTVAsync(id, userId);
            
            if (!result) return NotFound();
            
            return NoContent();
        }

        private bool IsAuthorized()
        {
            var userTypeIdStr = User.FindFirst("UserTypeId")?.Value;
            if (string.IsNullOrEmpty(userTypeIdStr)) return false;

            int userTypeId = int.Parse(userTypeIdStr);
            
            return userTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID ||
                   userTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID ||
                   userTypeId == UserTypeConstants.FRONT_DESK_OFFICER_USER_TYPE_ID;
        }
    }
}
