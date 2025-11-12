using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.Auth;
using YopoBackend.Modules.IntercomAccess.Services;

namespace YopoBackend.Modules.IntercomAccess.Controllers
{
    [ApiController]
    [Route("api/access-logs")]
    [Produces("application/json")]
    [Authorize]
    [Tags("Audit/Access Logs")]
    public class AccessLogsController : ControllerBase
    {
        private readonly IIntercomAccessService _service;
        public AccessLogsController(IIntercomAccessService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int? buildingId = null,
            [FromQuery] int? intercomId = null,
            [FromQuery] int? codeId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] bool? success = null,
            [FromQuery] string? credentialType = null,
            [FromQuery] int? userId = null)
        {
            var (items, total) = await _service.GetAccessLogsGlobalAsync(buildingId, intercomId, codeId, page, pageSize, from, to, success, credentialType, userId);
            var response = new YopoBackend.DTOs.PaginatedResponse<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO>(items, total, page, pageSize);
            return Ok(response);
        }
    }
}
