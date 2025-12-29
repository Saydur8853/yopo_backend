using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Auth;
using YopoBackend.Constants;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Services;

namespace YopoBackend.Modules.UserTypeCRUD.Controllers
{
    /// <summary>
    /// Controller for managing user types and their module permissions.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("18-UserTypes")]
    [Authorize]
    [RequireModule(ModuleConstants.USER_TYPE_MODULE_ID)]
    public class UserTypesController : ControllerBase
    {
        private readonly IUserTypeService _userTypeService;

        /// <summary>
        /// Initializes a new instance of the UserTypesController class.
        /// </summary>
        /// <param name="userTypeService">The user type service.</param>
        public UserTypesController(IUserTypeService userTypeService)
        {
            _userTypeService = userTypeService;
        }

        /// <summary>
        /// Gets user types with pagination and optional filters.
        /// </summary>
        /// <param name="page">Page number (starting from 1)</param>
        /// <param name="pageSize">Items per page (max 50)</param>
        /// <param name="searchTerm">Search by name or description</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="id">Filter by specific user type ID</param>
        /// <param name="sortBy">Sort field: name, createdAt, isActive</param>
        /// <param name="isSortAscending">Sort order: true=ASC, false=DESC</param>
        /// <param name="includePermissions">Include module permission details</param>
        /// <param name="moduleId">Filter by module access</param>
        /// <param name="includeInactiveModules">Include inactive module permissions in filters</param>
        /// <param name="includeUserCounts">Include number of users per user type</param>
        /// <returns>Paginated list of user types.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserTypeListResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserTypeListResponseDTO>> GetUserTypes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? id = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isSortAscending = true,
            [FromQuery] bool includePermissions = true,
            [FromQuery] int? moduleId = null,
            [FromQuery] bool includeInactiveModules = false,
            [FromQuery] bool includeUserCounts = false)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var result = await _userTypeService.GetUserTypesWithFiltersAsync(
                currentUserId,
                page,
                pageSize,
                searchTerm,
                isActive,
                id,
                sortBy,
                isSortAscending,
                includePermissions,
                moduleId,
                includeInactiveModules,
                includeUserCounts);

            return Ok(result);
        }


        /// <summary>
        /// Creates a new user type.
        /// </summary>
        /// <param name="dto">User type data</param>
        /// <returns>Created user type.</returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(UserTypeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserTypeDto>> CreateUserType([FromBody] CreateUserTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", data = ModelState });
            }

            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                var created = await _userTypeService.CreateUserTypeAsync(dto, currentUserId);
                return StatusCode(StatusCodes.Status201Created, created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing user type.
        /// </summary>
        /// <param name="id">User type ID</param>
        /// <param name="dto">Updated user type data</param>
        /// <returns>Updated user type.</returns>
        [HttpPut("{id:int}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(UserTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserTypeDto>> UpdateUserType(int id, [FromBody] UpdateUserTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", data = ModelState });
            }

            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                var updated = await _userTypeService.UpdateUserTypeAsync(id, dto, currentUserId);
                if (updated == null)
                {
                    return NotFound(new { message = $"User type with ID {id} not found." });
                }

                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a user type.
        /// </summary>
        /// <param name="id">User type ID</param>
        /// <returns>Deletion confirmation.</returns>
        [HttpDelete("{id:int}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                var deleted = await _userTypeService.DeleteUserTypeAsync(id, currentUserId);
                if (deleted == null)
                {
                    return NotFound(new { message = $"User type with ID {id} not found." });
                }

                return Ok(new { message = "User type deleted successfully.", data = deleted });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Initializes default user types.
        /// </summary>
        /// <returns>Initialization status message.</returns>
        [HttpPost("initialize-defaults")]
        [Authorize(Roles = Roles.SuperAdmin)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InitializeDefaults()
        {
            await _userTypeService.InitializeDefaultUserTypesAsync();
            return Ok(new { message = "Default user types initialized." });
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out userId);
        }
    }
}
