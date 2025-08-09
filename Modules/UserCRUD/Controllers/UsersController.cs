using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Services;
using YopoBackend.Services;

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

        /// <summary>
        /// Initializes a new instance of the UsersController class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="jwtService">The JWT service for token management.</param>
        public UsersController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
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
        public async Task<IActionResult> Register([FromBody] RegisterUserRequestDTO registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAsync(registerRequest);

            if (result == null)
            {
                return BadRequest(new { message = "Registration failed. Email may already be registered." });
            }

            return Ok(result);
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
        [ProducesResponseType(typeof(UserListResponseDTO), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? userTypeId = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _userService.GetAllUsersAsync(page, pageSize, searchTerm, userTypeId, isActive);
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
            var result = await _userService.GetUserByIdAsync(id);

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

            var result = await _userService.GetUserByIdAsync(userId);

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
        [ProducesResponseType(typeof(UserResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDTO createRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.CreateUserAsync(createRequest);

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

            var result = await _userService.UpdateUserAsync(id, updateRequest);

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
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);

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
    }
}
