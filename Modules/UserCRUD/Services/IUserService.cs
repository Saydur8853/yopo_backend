using YopoBackend.Modules.UserCRUD.DTOs;

namespace YopoBackend.Modules.UserCRUD.Services
{
    /// <summary>
    /// Interface for user management services including authentication and CRUD operations.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    public interface IUserService
    {
        // Authentication methods

        /// <summary>
        /// Authenticates a user with email and password and returns a JWT token.
        /// </summary>
        /// <param name="loginRequest">The login request containing email and password.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        Task<AuthenticationResponseDTO?> LoginAsync(LoginRequestDTO loginRequest);

        /// <summary>
        /// Registers a new user and returns a JWT token.
        /// </summary>
        /// <param name="registerRequest">The registration request containing user details.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        Task<AuthenticationResponseDTO?> RegisterAsync(RegisterUserRequestDTO registerRequest);


        // CRUD operations

        /// <summary>
        /// Gets all users with pagination support and access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <param name="page">The page number (starting from 1).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="searchTerm">Optional search term to filter users by name or email.</param>
        /// <param name="userTypeId">Optional user type ID to filter users.</param>
        /// <param name="isActive">Optional active status to filter users.</param>
        /// <returns>A paginated list of users based on access control.</returns>
        Task<UserListResponseDTO> GetAllUsersAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, int? userTypeId = null, bool? isActive = null);

        /// <summary>
        /// Gets a user by their ID with access control.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>The user if found and accessible; otherwise, null.</returns>
        Task<UserResponseDTO?> GetUserByIdAsync(int id, int currentUserId);

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="emailAddress">The user's email address.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        Task<UserResponseDTO?> GetUserByEmailAsync(string emailAddress);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="createRequest">The create user request.</param>
        /// <param name="createdByUserId">The ID of the user creating this user.</param>
        /// <returns>The created user.</returns>
        Task<UserResponseDTO?> CreateUserAsync(CreateUserRequestDTO createRequest, int createdByUserId);

        /// <summary>
        /// Updates an existing user with access control.
        /// </summary>
        /// <param name="id">The user ID to update.</param>
        /// <param name="updateRequest">The update user request.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>The updated user if successful and accessible; otherwise, null.</returns>
        Task<UserResponseDTO?> UpdateUserAsync(int id, UpdateUserRequestDTO updateRequest, int currentUserId);

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns>True if the password was changed successfully; otherwise, false.</returns>
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDTO changePasswordRequest);

        /// <summary>
        /// Resets a user's password (Super Admin only - doesn't require current password).
        /// </summary>
        /// <param name="userId">The user ID whose password to reset.</param>
        /// <param name="resetPasswordRequest">The reset password request.</param>
        /// <param name="currentUserId">The ID of the user performing the reset (must be Super Admin).</param>
        /// <returns>True if the password was reset successfully; otherwise, false.</returns>
        Task<bool> ResetPasswordAsync(int userId, ResetPasswordRequestDTO resetPasswordRequest, int currentUserId);

        /// <summary>
        /// Deletes a user by their ID with access control.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>True if the user was deleted successfully and accessible; otherwise, false.</returns>
        Task<bool> DeleteUserAsync(int id, int currentUserId);

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="isActive">Whether to activate or deactivate the account.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        Task<bool> SetUserActiveStatusAsync(int id, bool isActive);

        /// <summary>
        /// Updates the user's last login time.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Checks if an email address is already registered.
        /// </summary>
        /// <param name="emailAddress">The email address to check.</param>
        /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates).</param>
        /// <returns>True if the email is already registered; otherwise, false.</returns>
        Task<bool> IsEmailRegisteredAsync(string emailAddress, int? excludeUserId = null);
    }
}
