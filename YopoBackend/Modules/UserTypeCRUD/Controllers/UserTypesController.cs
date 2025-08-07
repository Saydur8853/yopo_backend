using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Services;

namespace YopoBackend.Modules.UserTypeCRUD.Controllers
{
    /// <summary>
    /// Controller for UserType CRUD operations.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
        /// Gets all user types.
        /// </summary>
        /// <returns>A list of all user types.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTypeDto>>> GetAllUserTypes()
        {
            try
            {
                var userTypes = await _userTypeService.GetAllUserTypesAsync();
                return Ok(userTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active user types.
        /// </summary>
        /// <returns>A list of all active user types.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserTypeDto>>> GetActiveUserTypes()
        {
            try
            {
                var userTypes = await _userTypeService.GetActiveUserTypesAsync();
                return Ok(userTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user type by ID.
        /// </summary>
        /// <param name="id">The ID of the user type.</param>
        /// <returns>The user type with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTypeDto>> GetUserType(int id)
        {
            try
            {
                var userType = await _userTypeService.GetUserTypeByIdAsync(id);
                if (userType == null)
                {
                    return NotFound($"User type with ID {id} not found.");
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
                // Check if user type name already exists
                if (await _userTypeService.UserTypeExistsAsync(createUserTypeDto.Name))
                {
                    return BadRequest($"A user type with the name '{createUserTypeDto.Name}' already exists.");
                }

                // Validate module IDs if provided
                if (createUserTypeDto.ModuleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(createUserTypeDto.ModuleIds))
                    {
                        return BadRequest("One or more module IDs are invalid.");
                    }
                }

                var createdUserType = await _userTypeService.CreateUserTypeAsync(createUserTypeDto);
                return CreatedAtAction(nameof(GetUserType), new { id = createdUserType.Id }, createdUserType);
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
                // Check if user type name already exists (excluding current user type)
                if (await _userTypeService.UserTypeExistsAsync(updateUserTypeDto.Name, id))
                {
                    return BadRequest($"A user type with the name '{updateUserTypeDto.Name}' already exists.");
                }

                // Validate module IDs if provided
                if (updateUserTypeDto.ModuleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(updateUserTypeDto.ModuleIds))
                    {
                        return BadRequest("One or more module IDs are invalid.");
                    }
                }

                var updatedUserType = await _userTypeService.UpdateUserTypeAsync(id, updateUserTypeDto);
                if (updatedUserType == null)
                {
                    return NotFound($"User type with ID {id} not found.");
                }

                return Ok(updatedUserType);
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
        /// <returns>A confirmation of deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserType(int id)
        {
            try
            {
                var result = await _userTypeService.DeleteUserTypeAsync(id);
                if (!result)
                {
                    return NotFound($"User type with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                var userType = await _userTypeService.GetUserTypeByIdAsync(id);
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
                var userType = await _userTypeService.GetUserTypeByIdAsync(id);
                if (userType == null)
                {
                    return NotFound($"User type with ID {id} not found.");
                }

                // Validate module IDs if provided
                if (moduleIds.Any())
                {
                    if (!await _userTypeService.ValidateModuleIdsAsync(moduleIds))
                    {
                        return BadRequest("One or more module IDs are invalid.");
                    }
                }

                var result = await _userTypeService.UpdateUserTypeModulePermissionsAsync(id, moduleIds);
                if (!result)
                {
                    return BadRequest("Failed to update user type permissions.");
                }

                return NoContent();
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
                    return BadRequest("Name parameter is required.");
                }

                var exists = await _userTypeService.UserTypeExistsAsync(name, excludeId);
                return Ok(new { available = !exists, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
