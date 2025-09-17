using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Services;
using YopoBackend.Services;
using YopoBackend.Utils;

namespace YopoBackend.Modules.UserCRUD.Controllers
{
    /// <summary>
    /// Consolidated controller for user management with authentication and CRUD operations.
    /// Module: UserCRUD (Module ID: 3) - Streamlined API with merged endpoints.
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

        // ==== AUTHENTICATION ENDPOINTS ====

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginRequest">The login request containing email and password.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(loginRequest);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password, or account is inactive." });

            return Ok(result);
        }

        /// <summary>
        /// Registers a new user and returns a JWT token.
        /// </summary>
        /// <param name="registerRequest">The registration request containing user details.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequestDTO registerRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.RegisterAsync(registerRequest);
                if (result == null)
                    return BadRequest(new { message = "Registration failed. Email may already be registered." });

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
        /// Unified session management - logout, logout-all, or get active tokens.
        /// </summary>
        /// <param name="action">Action to perform: "logout", "logout-all", or "tokens"</param>
        /// <returns>Success status or token information.</returns>
        [HttpPost("session/{action}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ManageSession(string action)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token." });

            switch (action.ToLower())
            {
                case "logout":
                    var authHeader = Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer "))
                    {
                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        await _jwtService.RevokeTokenAsync(token);
                    }
                    return Ok(new { message = "Logout successful." });

                case "logout-all":
                    var revokedCount = await _jwtService.RevokeAllUserTokensAsync(userId);
                    return Ok(new { 
                        message = "Logout from all devices successful.", 
                        revokedTokens = revokedCount 
                    });

                case "tokens":
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
                        totalCount = activeTokens.Count() 
                    });

                default:
                    return BadRequest(new { message = "Invalid action. Use 'logout', 'logout-all', or 'tokens'." });
            }
        }

        // ==== USER CRUD ENDPOINTS ====

        /// <summary>
        /// Gets users with pagination and filtering. Use "me" as email to get current user profile.
        /// </summary>
        /// <param name="email">User email or "me" for current user</param>
        /// <param name="page">Page number for listing users (when email is not provided)</param>
        /// <param name="pageSize">Page size for listing users</param>
        /// <param name="searchTerm">Search term for filtering users</param>
        /// <param name="userTypeId">Filter by user type</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>User details or paginated list of users.</returns>
        [HttpGet("{email?}")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(typeof(UserListResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUsers(
            string? email = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? userTypeId = null,
            [FromQuery] bool? isActive = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            // Get current user profile
            if (email == "me")
            {
                var currentUser = await _userService.GetUserByIdAsync(currentUserId, currentUserId);
                if (currentUser == null)
                    return NotFound(new { message = "User not found." });
                return Ok(currentUser);
            }

            // Get specific user by email
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound(new { message = "User not found." });
                return Ok(user);
            }

            // Get all users with pagination (requires module access)
            if (!User.HasClaim("module", ModuleConstants.USER_MODULE_ID.ToString()))
                return Forbid("User module access required.");

            var result = await _userService.GetAllUsersAsync(currentUserId, page, pageSize, searchTerm, userTypeId, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new user or updates an existing user.
        /// </summary>
        /// <param name="email">User email for update, or omit for create</param>
        /// <param name="userRequest">User data</param>
        /// <returns>Created or updated user.</returns>
        [HttpPost("{email?}")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(typeof(UserResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateOrUpdateUser(string? email, [FromBody] object userRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            // Create new user
            if (string.IsNullOrEmpty(email))
            {
                var createRequest = System.Text.Json.JsonSerializer.Deserialize<CreateUserRequestDTO>(userRequest.ToString()!);
                if (createRequest == null)
                    return BadRequest(new { message = "Invalid user data." });

                var result = await _userService.CreateUserAsync(createRequest, currentUserId);
                if (result == null)
                    return BadRequest(new { message = "User creation failed. Email may already be registered or user type is invalid." });

                return CreatedAtAction(nameof(GetUsers), new { email = result.Email }, result);
            }

            // Update existing user
            var updateRequest = System.Text.Json.JsonSerializer.Deserialize<UpdateUserRequestDTO>(userRequest.ToString()!);
            if (updateRequest == null)
                return BadRequest(new { message = "Invalid user data." });

            var updateResult = await _userService.UpdateUserByEmailAsync(email, updateRequest, currentUserId);
            if (updateResult == null)
                return NotFound(new { message = "User not found or user type is invalid." });

            return Ok(updateResult);
        }

        /// <summary>
        /// Updates the current user's profile information.
        /// </summary>
        /// <param name="updateRequest">Profile update request</param>
        /// <returns>Updated user profile.</returns>
        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserRequestDTO updateRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            try
            {
                var result = await _userService.UpdateUserAsync(currentUserId, updateRequest, currentUserId);
                if (result == null)
                    return NotFound(new { message = "User not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update profile: " + ex.Message });
            }
        }




        /// <summary>
        /// Deletes a user by their email.
        /// </summary>
        /// <param name="email">The user email to delete.</param>
        /// <returns>Success status.</returns>
        [HttpDelete("{email}")]
        [Authorize]
        [RequireModule(ModuleConstants.USER_MODULE_ID)]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            var result = await _userService.DeleteUserByEmailAsync(email, currentUserId);
            if (!result)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User deleted successfully." });
        }

        // ==== UTILITY ENDPOINTS ====

        /// <summary>
        /// Unified validation endpoint - check email availability, registration eligibility, or get user modules.
        /// </summary>
        /// <param name="action">Action: "email-availability", "registration-eligibility", or "modules"</param>
        /// <param name="email">Email address for email-related checks</param>
        /// <returns>Validation results or user modules.</returns>
        [HttpGet("validate/{action}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateOrGetInfo(string action, [FromQuery] string? email = null)
        {
            switch (action.ToLower())
            {
                case "email-availability":
                    if (string.IsNullOrEmpty(email))
                        return BadRequest(new { message = "Email parameter is required." });

                    var isRegistered = await _userService.IsEmailRegisteredAsync(email);
                    return Ok(new { 
                        email = email, 
                        isAvailable = !isRegistered,
                        message = isRegistered ? "Email is already registered." : "Email is available."
                    });

                case "registration-eligibility":
                    if (string.IsNullOrEmpty(email))
                        return BadRequest(new { message = "Email parameter is required." });

                    var isFirstUser = !await _userService.IsEmailRegisteredAsync(email) && !await _context.Users.AnyAsync();
                    
                    if (isFirstUser)
                    {
                        return Ok(new {
                            canRegister = true,
                            isFirstUser = true,
                            message = "You will be registered as the Super Administrator."
                        });
                    }

                    if (await _userService.IsEmailRegisteredAsync(email))
                    {
                        return Ok(new {
                            canRegister = false,
                            isFirstUser = false,
                            message = "Email is already registered."
                        });
                    }

                    var hasInvitation = await _context.Invitations
                        .AnyAsync(i => i.EmailAddress.ToLower() == email.ToLower() && i.ExpiryTime > DateTime.UtcNow);

                    return Ok(new {
                        canRegister = hasInvitation,
                        isFirstUser = false,
                        message = hasInvitation ? "You have a valid invitation to register." : "You are not invited. Please contact with Authority."
                    });

                case "modules":
                    // Requires authentication
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!int.TryParse(userIdClaim, out int userId))
                        return Unauthorized(new { message = "Invalid token." });

                    var user = await _context.Users
                        .Include(u => u.UserType)
                            .ThenInclude(ut => ut!.ModulePermissions)
                                .ThenInclude(utmp => utmp.Module)
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user == null)
                        return NotFound(new { message = "User not found." });

                    object[] userModules = user.UserType?.ModulePermissions != null ?
                        user.UserType.ModulePermissions
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
                            .ToArray() : Array.Empty<object>();

                    return Ok(new
                    {
                        userId = user.Id,
                        userType = user.UserType?.Name,
                        modules = userModules,
                        totalModules = userModules.Length
                    });

                default:
                    return BadRequest(new { message = "Invalid action. Use 'email-availability', 'registration-eligibility', or 'modules'." });
            }
        }
    }
}