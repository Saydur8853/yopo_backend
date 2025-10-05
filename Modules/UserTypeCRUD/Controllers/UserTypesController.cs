using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Services;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using System.Security.Claims;

namespace YopoBackend.Modules.UserTypeCRUD.Controllers
{
    /// <summary>
    /// Controller for UserType CRUD operations.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.USER_TYPE_MODULE_ID)]
    public class UserTypesController : ControllerBase
    {
        private readonly IUserTypeService _userTypeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypesController"/> class.
        /// </summary>
        /// <param name="userTypeService">The user type service.</param>
        public UserTypesController(IUserTypeService userTypeService)
        {
            _userTypeService = userTypeService;
        }

        /// <summary>
        /// Gets the current user ID from the JWT token claims.
        /// </summary>
        /// <returns>The current user's ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        /// <summary>
        /// Checks if the current user is a Property Manager.
        /// </summary>
        /// <returns>True if current user's UserTypeId equals PROPERTY_MANAGER_USER_TYPE_ID.</returns>
        private bool IsCurrentUserPropertyManager()
        {
            var userTypeIdClaim = User.FindFirst("UserTypeId")?.Value;
            return int.TryParse(userTypeIdClaim, out int userTypeId) && userTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID;
        }

        /// <summary>
        /// Gets paginated user types with optional filters.
        /// </summary>
        /// <param name="page">Page number (starting from 1)</param>
        /// <param name="pageSize">Items per page (default 10, max 50)</param>
        /// <param name="searchTerm">Optional search by name</param>
        /// <param name="isActive">Optional filter by active status</param>
        /// <returns>Paginated list of user types the current user has access to.</returns>
        [HttpGet]
        public async Task<ActionResult<UserTypeListResponseDTO>> GetAllUserTypes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
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

                var result = await _userTypeService.GetUserTypesAsync(userId, page, pageSize, searchTerm, isActive);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all user types (non-paginated - for backwards compatibility).
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<UserTypeDto>>> GetAllUserTypesList()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var userTypes = await _userTypeService.GetAllUserTypesAsync(userId);
                return Ok(userTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active user types based on current user's access control.
        /// </summary>
        /// <returns>A list of all active user types the current user has access to.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserTypeDto>>> GetActiveUserTypes()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var userTypes = await _userTypeService.GetActiveUserTypesAsync(userId);
                return Ok(userTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user type by ID based on current user's access control.
        /// </summary>
        /// <param name="id">The ID of the user type.</param>
        /// <returns>The user type with the specified ID if accessible.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTypeDto>> GetUserType(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var userType = await _userTypeService.GetUserTypeByIdAsync(id, userId);
                if (userType == null)
                {
                    return NotFound($"User type with ID {id} not found or you don't have access to it.");
                }

                return Ok(userType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new user type.
        /// </summary>
        /// <param name="createUserTypeDto">The data for creating the user type.</param>
        /// <returns>The created user type.</returns>
        [HttpPost]
        public async Task<ActionResult<UserTypeDto>> CreateUserType(CreateUserTypeDto createUserTypeDto)
        {
            try
            {
                if (IsCurrentUserPropertyManager())
                {
                    return StatusCode(403, new { message = "Property Managers are not allowed to create user types." });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Note: Multiple user types with the same name are allowed

                // Validate module IDs if provided
                if (createUserTypeDto.ModuleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(createUserTypeDto.ModuleIds))
                    {
                        return BadRequest(new { message = "One or more module IDs are invalid." });
                    }
                }

                var createdUserType = await _userTypeService.CreateUserTypeAsync(createUserTypeDto, userId);
                return CreatedAtAction(nameof(GetUserType), new { id = createdUserType.Id }, new
                {
                    message = $"User type '{createdUserType.Name}' with ID {createdUserType.Id} has been created successfully.",
                    userType = createdUserType
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing user type.
        /// </summary>
        /// <param name="id">The ID of the user type to update.</param>
        /// <param name="updateUserTypeDto">The data for updating the user type.</param>
        /// <returns>The updated user type.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserTypeDto>> UpdateUserType(int id, UpdateUserTypeDto updateUserTypeDto)
        {
            try
            {
                if (IsCurrentUserPropertyManager())
                {
                    return StatusCode(403, new { message = "Property Managers are not allowed to update user types." });
                }
                // Note: Multiple user types with the same name are allowed

                // Validate module IDs if provided
                if (updateUserTypeDto.ModuleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(updateUserTypeDto.ModuleIds))
                    {
                        return BadRequest(new { message = "One or more module IDs are invalid." });
                    }
                }

                var currentUserId = GetCurrentUserId();
                var updatedUserType = await _userTypeService.UpdateUserTypeAsync(id, updateUserTypeDto, currentUserId);
                if (updatedUserType == null)
                {
                    return NotFound(new { message = $"User type with ID {id} not found or you don't have access to update it." });
                }

                return Ok(new
                {
                    message = $"User type '{updatedUserType.Name}' with ID {updatedUserType.Id} has been updated successfully.",
                    userType = updatedUserType
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user type.
        /// </summary>
        /// <param name="id">The ID of the user type to delete.</param>
        /// <returns>A confirmation of deletion with details of the deleted user type.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserType(int id)
        {
            try
            {
                if (IsCurrentUserPropertyManager())
                {
                    return StatusCode(403, new { message = "Property Managers are not allowed to delete user types." });
                }
                var currentUserId = GetCurrentUserId();
                var deletedUserType = await _userTypeService.DeleteUserTypeAsync(id, currentUserId);
                if (deletedUserType == null)
                {
                    return NotFound(new { message = $"User type with ID {id} not found or you don't have access to delete it." });
                }

                return Ok(new 
                { 
                    message = $"User type '{deletedUserType.Name}' with ID {deletedUserType.Id} has been deleted successfully.",
                    deletedUserType = new
                    {
                        id = deletedUserType.Id,
                        name = deletedUserType.Name,
                        description = deletedUserType.Description,
                        dataAccessControl = deletedUserType.DataAccessControl,
                        moduleCount = deletedUserType.ModuleIds.Count
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets the module permissions for a specific user type.
        /// </summary>
        /// <param name="id">The ID of the user type.</param>
        /// <returns>The module permissions for the user type.</returns>
        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<UserTypeModulePermissionDto>>> GetUserTypePermissions(int id)
        {
            try
            {
                // Check if user type exists
                var currentUserId = GetCurrentUserId();
                var userType = await _userTypeService.GetUserTypeByIdAsync(id, currentUserId);
                if (userType == null)
                {
                    return NotFound($"User type with ID {id} not found.");
                }

                var permissions = await _userTypeService.GetUserTypeModulePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the module permissions for a specific user type.
        /// </summary>
        /// <param name="id">The ID of the user type.</param>
        /// <param name="moduleIds">The list of module IDs to assign to the user type.</param>
        /// <returns>A confirmation of the update.</returns>
        [HttpPut("{id}/permissions")]
        public async Task<ActionResult> UpdateUserTypePermissions(int id, [FromBody] List<int> moduleIds)
        {
            try
            {
                // Check if user type exists
                var currentUserId = GetCurrentUserId();
                var userType = await _userTypeService.GetUserTypeByIdAsync(id, currentUserId);
                if (userType == null)
                {
                    return NotFound($"User type with ID {id} not found.");
                }

                // Validate module IDs if provided
                if (moduleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(moduleIds))
                    {
                        return BadRequest(new { message = "One or more module IDs are invalid." });
                    }
                }

                var result = await _userTypeService.UpdateUserTypeModulePermissionsAsync(id, moduleIds, currentUserId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to update user type permissions." });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a list of existing user type names for dropdown/autocomplete functionality.
        /// </summary>
        /// <param name="activeOnly">Optional parameter to return only active user types (default: true).</param>
        /// <returns>A list of existing user type names.</returns>
        [HttpGet("names")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserTypeNames([FromQuery] bool activeOnly = true)
        {
            try
            {
                var names = await _userTypeService.GetUserTypeNamesAsync(activeOnly);
                return Ok(names);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a user type name is available.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name is available, false if it already exists.</returns>
        [HttpGet("check-name")]
        public async Task<ActionResult<bool>> CheckUserTypeName([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Name parameter is required." });
                }

                var exists = await _userTypeService.UserTypeExistsAsync(name, excludeId);
                return Ok(new { available = !exists, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes default user types (like Super Admin) in the database.
        /// This is typically called during application startup.
        /// </summary>
        /// <returns>A confirmation of initialization.</returns>
        [HttpPost("initialize-defaults")]
        public async Task<ActionResult> InitializeDefaultUserTypes()
        {
            try
            {
                await _userTypeService.InitializeDefaultUserTypesAsync();
                return Ok(new { message = "Default user types initialized successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
