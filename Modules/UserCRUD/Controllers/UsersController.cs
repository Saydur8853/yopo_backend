using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Services;
using YopoBackend.Services;
using YopoBackend.Utils;

namespace YopoBackend.Modules.UserCRUD.Controllers
{
    /// <summary>
    /// Controller for managing users with authentication and CRUD operations.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UsersController class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="jwtService">The JWT service for token management.</param>
        /// <param name="context">The database context.</param>
        public UsersController(IUserService userService, IJwtService jwtService, ApplicationDbContext context)
        {
            _userService = userService;
            _jwtService = jwtService;
            _context = context;
        }

        // Authentication endpoints

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginRequest">The login request containing email and password.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid credentials or account inactive</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.LoginAsync(loginRequest);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password, or account is inactive." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Registers a new user and returns a JWT token.
        /// </summary>
        /// <param name="registerRequest">The registration request containing user details.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid request data or email already registered</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequestDTO registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userService.RegisterAsync(registerRequest);

                if (result == null)
                {
                    return BadRequest(new { message = "Registration failed. Email may already be registered." });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { 
                    message = ex.Message,
                    canRegister = false,
                    requiresInvitation = true
                });
            }
        }

        /// <summary>
        /// Logs out the current user by revoking their current token.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                await _jwtService.RevokeTokenAsync(token);
            }

            return Ok(new { message = "Logout successful." });
        }

        /// <summary>
        /// Logs out from all devices by revoking all user tokens.
        /// </summary>
        /// <returns>Success status with number of revoked tokens.</returns>
        /// <response code="200">Logout from all devices successful</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> LogoutAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var revokedCount = await _jwtService.RevokeAllUserTokensAsync(userId);

            return Ok(new { 
                message = "Logout from all devices successful.", 
                revokedTokens = revokedCount 
            });
        }

        /// <summary>
        /// Gets active tokens for the current user.
        /// </summary>
        /// <returns>List of active tokens.</returns>
        /// <response code="200">Active tokens retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("active-tokens")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetActiveTokens()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var activeTokens = await _jwtService.GetUserActiveTokensAsync(userId);

            var tokenSummary = activeTokens.Select(t => new
            {
                id = t.Id,
                tokenType = t.TokenType,
                createdAt = t.CreatedAt,
                expiresAt = t.ExpiresAt,
                lastUsedAt = t.LastUsedAt,
                deviceInfo = t.DeviceInfo,
                ipAddress = t.IpAddress,
                isExpired = t.IsExpired
            });

            return Ok(new { 
                activeTokens = tokenSummary,
                totalCount = activeTokens.Count 
            });
        }

        // CRUD endpoints

        /// <summary>
        /// Gets all users with pagination and filtering support.
        /// </summary>
        /// <param name="page">The page number (starting from 1). Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 10.</param>
        /// <param name="searchTerm">Optional search term to filter users by name or email.</param>
        /// <param name="userTypeId">Optional user type ID to filter users.</param>
        /// <param name="isActive">Optional active status to filter users.</param>
        /// <returns>A paginated list of users.</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(typeof(UserListResponseDTO), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? userTypeId = null,
            [FromQuery] bool? isActive = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.GetAllUsersAsync(currentUserId, page, pageSize, searchTerm, userTypeId, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The user if found.</returns>
        /// <response code="200">User found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.GetUserByIdAsync(id, currentUserId);

            if (result == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets the currently authenticated user's profile.
        /// </summary>
        /// <returns>The current user's profile.</returns>
        /// <response code="200">User profile retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.GetUserByIdAsync(userId, userId);

            if (result == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="createRequest">The create user request.</param>
        /// <returns>The created user.</returns>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Invalid request data or email already registered</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(typeof(UserResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDTO createRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.CreateUserAsync(createRequest, currentUserId);

            if (result == null)
            {
                return BadRequest(new { message = "User creation failed. Email may already be registered or user type is invalid." });
            }

            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The user ID to update.</param>
        /// <param name="updateRequest">The update user request.</param>
        /// <returns>The updated user.</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDTO updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.UpdateUserAsync(id, updateRequest, currentUserId);

            if (result == null)
            {
                return NotFound(new { message = "User not found or user type is invalid." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid request data or current password is incorrect</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.ChangePasswordAsync(userId, changePasswordRequest);

            if (!result)
            {
                return BadRequest(new { message = "Password change failed. Current password may be incorrect." });
            }

            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Resets the current user's own password (doesn't require current password).
        /// </summary>
        /// <param name="resetPasswordRequest">The reset password request.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("reset-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ResetOwnPassword([FromBody] ResetPasswordRequestDTO resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.ResetPasswordAsync(userId, resetPasswordRequest, userId);

            if (!result)
            {
                return BadRequest(new { message = "Password reset failed." });
            }

            return Ok(new { message = "Password reset successfully." });
        }

        /// <summary>
        /// Changes another user's password (admin function).
        /// </summary>
        /// <param name="id">The user ID whose password to change.</param>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid request data or current password is incorrect</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpPost("{id}/change-password")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ChangePasswordAsync(id, changePasswordRequest);

            if (!result)
            {
                return BadRequest(new { message = "Password change failed. User may not exist or current password may be incorrect." });
            }

            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Resets another user's password (Super Admin only - doesn't require current password).
        /// </summary>
        /// <param name="id">The user ID whose password to reset.</param>
        /// <param name="resetPasswordRequest">The reset password request.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Only Super Admin can reset passwords</response>
        /// <response code="404">User not found</response>
        [HttpPost("{id}/reset-password")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResetUserPassword(int id, [FromBody] ResetPasswordRequestDTO resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.ResetPasswordAsync(id, resetPasswordRequest, currentUserId);

            if (!result)
            {
                // Check if the current user is Super Admin to determine the error message
                var currentUser = await _context.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);
                
                if (currentUser == null || currentUser.UserTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
                {
                    return StatusCode(403, new { message = "Only Super Admin can reset user passwords." });
                }
                
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "Password reset successfully." });
        }

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="isActive">Whether to activate or deactivate the account.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">User status updated successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpPatch("{id}/status")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetUserActiveStatus(int id, [FromQuery] bool isActive)
        {
            var result = await _userService.SetUserActiveStatusAsync(id, isActive);

            if (!result)
            {
                return NotFound(new { message = "User not found." });
            }

            var status = isActive ? "activated" : "deactivated";
            return Ok(new { message = $"User account {status} successfully." });
        }

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var result = await _userService.DeleteUserAsync(id, currentUserId);

            if (!result)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "User deleted successfully." });
        }

        /// <summary>
        /// Checks if an email address is available for registration.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>Availability status.</returns>
        /// <response code="200">Email availability status</response>
        [HttpGet("check-email")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email parameter is required." });
            }

            var isRegistered = await _userService.IsEmailRegisteredAsync(email);

            return Ok(new { 
                email = email, 
                isAvailable = !isRegistered,
                message = isRegistered ? "Email is already registered." : "Email is available."
            });
        }

        /// <summary>
        /// Gets the current user's module permissions based on their user type.
        /// </summary>
        /// <returns>List of modules the current user has access to.</returns>
        /// <response code="200">Module permissions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("me/modules")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentUserModules()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            // Get user with user type and module permissions
            var user = await _context.Users
                .Include(u => u.UserType)
                    .ThenInclude(ut => ut!.ModulePermissions)
                        .ThenInclude(utmp => utmp.Module)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Get all modules the user has access to
            object[] userModules;
            if (user.UserType?.ModulePermissions != null)
            {
                userModules = user.UserType.ModulePermissions
                    .Where(utmp => utmp.IsActive && utmp.Module.IsActive)
                    .Select(utmp => new
                    {
                        id = utmp.Module.Id,
                        name = utmp.Module.Name,
                        description = utmp.Module.Description,
                        version = utmp.Module.Version,
                        isActive = utmp.Module.IsActive
                    })
                    .Cast<object>()
                    .ToArray();
            }
            else
            {
                userModules = Array.Empty<object>();
            }

            return Ok(new
            {
                userId = user.Id,
                userType = user.UserType?.Name,
                modules = userModules,
                totalModules = userModules.Length
            });
        }

        /// <summary>
        /// Checks if registration is allowed for a given email address.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>Registration eligibility status.</returns>
        /// <response code="200">Registration eligibility status</response>
        [HttpGet("check-registration-eligibility")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckRegistrationEligibility([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email parameter is required." });
            }

            // Check if this would be the first user
            var isFirstUser = !await _userService.IsEmailRegisteredAsync(email) && !await _context.Users.AnyAsync();
            
            if (isFirstUser)
            {
                return Ok(new {
                    canRegister = true,
                    isFirstUser = true,
                    message = "You will be registered as the Super Administrator."
                });
            }

            // Check if email is already registered
            if (await _userService.IsEmailRegisteredAsync(email))
            {
                return Ok(new {
                    canRegister = false,
                    isFirstUser = false,
                    message = "Email is already registered."
                });
            }

            // Check if email has a valid invitation
            var hasInvitation = await _context.Invitations
                .AnyAsync(i => i.EmailAddress.ToLower() == email.ToLower() && i.ExpiryTime > DateTime.UtcNow);

            if (hasInvitation)
            {
                return Ok(new {
                    canRegister = true,
                    isFirstUser = false,
                    message = "You have a valid invitation to register."
                });
            }

            return Ok(new {
                canRegister = false,
                isFirstUser = false,
                message = "You are not invited. Please contact with Authority."
            });
        }

        // Profile Photo Management Endpoints

        /// <summary>
        /// Updates the current user's profile photo.
        /// </summary>
        /// <param name="request">The profile photo update request containing base64 encoded image.</param>
        /// <returns>Success status with updated user information.</returns>
        /// <response code="200">Profile photo updated successfully</response>
        /// <response code="400">Invalid image format or size</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("me/profile-photo")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateCurrentUserProfilePhoto([FromBody] UpdateProfilePhotoRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                // Validate the image if provided
                if (!string.IsNullOrEmpty(request.ProfilePhotoBase64))
                {
                    var validationResult = ImageUtils.ValidateBase64Image(request.ProfilePhotoBase64);
                    if (!validationResult.IsValid)
                    {
                        return BadRequest(new { message = validationResult.ErrorMessage });
                    }
                }

                // Get current user
                var currentUser = await _context.Users.FindAsync(userId);
                if (currentUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Update profile photo
                if (!string.IsNullOrEmpty(request.ProfilePhotoBase64))
                {
                    var validationResult = ImageUtils.ValidateBase64Image(request.ProfilePhotoBase64);
                    currentUser.ProfilePhoto = validationResult.ImageBytes;
                    currentUser.ProfilePhotoMimeType = validationResult.MimeType;
                }
                else if (request.ProfilePhotoBase64 == string.Empty) // Explicitly removing photo
                {
                    currentUser.ProfilePhoto = null;
                    currentUser.ProfilePhotoMimeType = null;
                }

                currentUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Return updated user information
                var updatedUser = await _userService.GetUserByIdAsync(userId, userId);
                return Ok(new { 
                    message = "Profile photo updated successfully.", 
                    user = updatedUser 
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error updating profile photo: {ex.Message}" });
            }
        }

        /// <summary>
        /// Removes the current user's profile photo.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <response code="200">Profile photo removed successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpDelete("me/profile-photo")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveCurrentUserProfilePhoto()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                var currentUser = await _context.Users.FindAsync(userId);
                if (currentUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Remove profile photo
                currentUser.ProfilePhoto = null;
                currentUser.ProfilePhotoMimeType = null;
                currentUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile photo removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error removing profile photo: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets a user's profile photo by user ID (returns the raw image data).
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The profile photo as raw image data or 404 if not found.</returns>
        /// <response code="200">Profile photo retrieved successfully</response>
        /// <response code="404">User not found or no profile photo</response>
        [HttpGet("{id}/profile-photo")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserProfilePhoto(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new { u.ProfilePhoto, u.ProfilePhotoMimeType })
                    .FirstOrDefaultAsync();

                if (user == null || user.ProfilePhoto == null || user.ProfilePhoto.Length == 0)
                {
                    return NotFound(new { message = "Profile photo not found." });
                }

                // Return the raw image data with proper content type
                return File(user.ProfilePhoto, user.ProfilePhotoMimeType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving profile photo: {ex.Message}" });
            }
        }
    }
}
