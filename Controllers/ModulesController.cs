using Microsoft.AspNetCore.Mvc;
using YopoBackend.DTOs;
using YopoBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using YopoBackend.Data;
using YopoBackend.Constants;

namespace YopoBackend.Controllers
{
    /// <summary>
    /// Controller for managing modules.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("01-Modules")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ModulesController class.
        /// </summary>
        /// <param name="moduleService">The module service.</param>
        /// <param name="context">The database context.</param>
        public ModulesController(IModuleService moduleService, ApplicationDbContext context)
        {
            _moduleService = moduleService;
            _context = context;
        }

        /// <summary>
        /// Gets modules in the system with pagination and filtering.
        /// Property Managers will not see core admin modules (1, 2, 3).
        /// </summary>
        /// <param name="page">Page number (starting from 1). Default: 1</param>
        /// <param name="pageSize">Number of items per page. Default: 10</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <returns>Paginated list of modules with their IDs, names, and metadata.</returns>
        /// <response code="200">Returns the paginated list of modules</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ModuleListDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModuleListDto>> GetModules(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;

                var modules = await _moduleService.GetModulesAsync(page, pageSize, isActive);

                // Filter out restricted modules for Property Managers
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                {
                    modules.Modules = modules.Modules
                        .Where(m => m.Id != ModuleConstants.USER_TYPE_MODULE_ID
                                    && m.Id != ModuleConstants.INVITATION_MODULE_ID
                                    && m.Id != ModuleConstants.USER_MODULE_ID)
                        .ToList();
                    modules.TotalCount = modules.Modules.Count;
                    // Recalculate pagination info
                    modules.TotalPages = (int)Math.Ceiling((double)modules.TotalCount / pageSize);
                    modules.HasPreviousPage = page > 1;
                    modules.HasNextPage = page < modules.TotalPages;
                }

                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching modules.", details = ex.Message });
            }
        }




    }
}
