using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Constants;
using YopoBackend.Modules.CCTVCRUD.DTOs;
using YopoBackend.Modules.CCTVCRUD.Services;

namespace YopoBackend.Modules.CCTVCRUD.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("10-CCTV")]
    public class CCTVController : ControllerBase
    {
        private readonly ICCTVService _cctvService;

        public CCTVController(ICCTVService cctvService)
        {
            _cctvService = cctvService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CCTVResponseDto>>> GetAll()
        {
            if (!IsAuthorized()) return Forbid();
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cctvs = await _cctvService.GetAllCCTVsAsync(userId);
            return Ok(cctvs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CCTVResponseDto>> GetById(int id)
        {
            if (!IsAuthorized()) return Forbid();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cctv = await _cctvService.GetCCTVByIdAsync(id, userId);
            
            if (cctv == null) return NotFound();
            
            return Ok(cctv);
        }

        [HttpPost]
        public async Task<ActionResult<CCTVResponseDto>> Create(CreateCCTVDto createDto)
        {
            if (!IsAuthorized()) return Forbid();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            try
            {
                var cctv = await _cctvService.CreateCCTVAsync(createDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = cctv.CCTVId }, cctv);
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
