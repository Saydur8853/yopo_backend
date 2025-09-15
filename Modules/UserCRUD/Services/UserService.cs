using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Services;
using YopoBackend.Constants;

namespace YopoBackend.Modules.UserCRUD.Services
{
    /// <summary>
    /// Service for managing users including authentication and CRUD operations.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    public class UserService : BaseAccessControlService, IUserService
    {
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the UserService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="jwtService">The JWT service for token generation.</param>
        public UserService(ApplicationDbContext context, IJwtService jwtService) : base(context)
        {
            _jwtService = jwtService;
        }

        // Authentication methods

        /// <summary>
        /// Authenticates a user with email and password and returns a JWT token.
        /// </summary>
        /// <param name="loginRequest">The login request containing email and password.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        public async Task<AuthenticationResponseDTO?> LoginAsync(LoginRequestDTO loginRequest)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(loginRequest.EmailAddress) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    Console.WriteLine("Login attempt with empty email or password");
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == loginRequest.EmailAddress.ToLower());

                if (user == null)
                {
                    Console.WriteLine($"Login attempt with non-existent email: {loginRequest.EmailAddress}");
                    return null; // User not found
                }

                if (!user.IsActive)
                {
                    Console.WriteLine($"Login attempt with inactive account: {loginRequest.EmailAddress}");
                    return null; // Account is deactivated
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    Console.WriteLine($"Login attempt with invalid password for: {loginRequest.EmailAddress}");
                    return null; // Invalid password
                }

                // Update last login time
                await UpdateLastLoginAsync(user.Id);

                // Generate JWT token with IP and device info if available
                var (token, expiresAt) = await _jwtService.GenerateTokenAsync(user, null, null);

                Console.WriteLine($"Successful login for user: {user.EmailAddress}");
                return new AuthenticationResponseDTO
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = MapToUserResponse(user),
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error for {loginRequest.EmailAddress}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Registers a new user and returns a JWT token.
        /// </summary>
        /// <param name="registerRequest">The registration request containing user details.</param>
        /// <returns>An authentication response with JWT token if successful.</returns>
        public async Task<AuthenticationResponseDTO?> RegisterAsync(RegisterUserRequestDTO registerRequest)
        {
            try
            {
                // Check if email is already registered
                if (await IsEmailRegisteredAsync(registerRequest.EmailAddress))
                {
                    return null; // Email already registered
                }

                // Check if this is the first user in the system
                var isFirstUser = !await _context.Users.AnyAsync();
                
                int userTypeId;
                if (isFirstUser)
                {
                    // First user becomes Super Admin
                    userTypeId = UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
                    Console.WriteLine($"First user registration - assigning Super Admin role to: {registerRequest.EmailAddress}");
                }
                else
                {
                    // Check if email has a valid invitation
                    var invitation = await _context.Invitations
                        .Include(i => i.UserType)
                        .FirstOrDefaultAsync(i => i.EmailAddress.ToLower() == registerRequest.EmailAddress.ToLower() 
                                                 && i.ExpiryTime > DateTime.UtcNow);
                    
                    if (invitation == null)
                    {
                        // No valid invitation found
                        Console.WriteLine($"Registration denied for {registerRequest.EmailAddress} - no valid invitation");
                        throw new UnauthorizedAccessException("You are not invited. Please contact with Authority.");
                    }
                    
                    userTypeId = invitation.UserTypeId;
                    Console.WriteLine($"User registration with invitation - assigning user type {invitation.UserType?.Name} to: {registerRequest.EmailAddress}");
                    
                    // Remove the used invitation
                    _context.Invitations.Remove(invitation);
                }

                var userType = await _context.UserTypes.FindAsync(userTypeId);
                if (userType == null || !userType.IsActive)
                {
                    return null; // User type not found or inactive
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

                // Create new user
                var user = new User
                {
                    EmailAddress = registerRequest.EmailAddress.ToLower(),
                    PasswordHash = passwordHash,
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    PhoneNumber = registerRequest.PhoneNumber,
                    UserTypeId = userTypeId,
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedBy = 0, // Will be set to the user's own ID after creation
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Update CreatedBy to the user's own ID for self-registration
                user.CreatedBy = user.Id;
                await _context.SaveChangesAsync();

                // Load user type for response
                user.UserType = userType;

                // Generate JWT token
                var (token, expiresAt) = await _jwtService.GenerateTokenAsync(user);

                var message = isFirstUser ? "Registration successful - You are now the Super Administrator!" : "Registration successful";
                
                return new AuthenticationResponseDTO
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = MapToUserResponse(user),
                    Message = message
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw to preserve the specific error message
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return null;
            }
        }

        // CRUD operations with access control

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
        public async Task<UserListResponseDTO> GetAllUsersAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, int? userTypeId = null, bool? isActive = null)
        {
            try
            {
                var query = _context.Users.Include(u => u.UserType).AsQueryable();

                // Apply access control
                query = await ApplyAccessControlAsync(query, currentUserId);

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(u => u.EmailAddress.ToLower().Contains(search) ||
                                           u.FirstName.ToLower().Contains(search) ||
                                           u.LastName.ToLower().Contains(search));
                }

                if (userTypeId.HasValue)
                {
                    query = query.Where(u => u.UserTypeId == userTypeId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new UserListResponseDTO
                {
                    Users = users.Select(MapToUserResponse).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get users error: {ex.Message}");
                return new UserListResponseDTO();
            }
        }

        /// <summary>
        /// Gets a user by their ID with access control.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>The user if found and accessible; otherwise, null.</returns>
        public async Task<UserResponseDTO?> GetUserByIdAsync(int id, int currentUserId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return null;
                }

                // Check access control
                if (!await HasAccessToEntityAsync(user, currentUserId))
                {
                    return null; // User doesn't have access to this user record
                }

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user by ID error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="emailAddress">The user's email address.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<UserResponseDTO?> GetUserByEmailAsync(string emailAddress)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == emailAddress.ToLower());

                return user != null ? MapToUserResponse(user) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user by email error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="createRequest">The create user request.</param>
        /// <param name="createdByUserId">The ID of the user creating this user.</param>
        /// <returns>The created user.</returns>
        public async Task<UserResponseDTO?> CreateUserAsync(CreateUserRequestDTO createRequest, int createdByUserId)
        {
            try
            {
                // Check if email is already registered
                if (await IsEmailRegisteredAsync(createRequest.EmailAddress))
                {
                    return null; // Email already registered
                }

                // Check if user type exists
                var userType = await _context.UserTypes.FindAsync(createRequest.UserTypeId);
                if (userType == null)
                {
                    return null; // Invalid user type
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(createRequest.Password);

                // Create new user
                var user = new User
                {
                    EmailAddress = createRequest.EmailAddress.ToLower(),
                    PasswordHash = passwordHash,
                    FirstName = createRequest.FirstName,
                    LastName = createRequest.LastName,
                    PhoneNumber = createRequest.PhoneNumber,
                    ProfilePhoto = createRequest.ProfilePhoto,
                    UserTypeId = createRequest.UserTypeId,
                    IsActive = createRequest.IsActive,
                    IsEmailVerified = createRequest.IsEmailVerified,
                    CreatedBy = createdByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Load user type for response
                user.UserType = userType;

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create user error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates an existing user with access control.
        /// </summary>
        /// <param name="id">The user ID to update.</param>
        /// <param name="updateRequest">The update user request.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>The updated user if successful and accessible; otherwise, null.</returns>
        public async Task<UserResponseDTO?> UpdateUserAsync(int id, UpdateUserRequestDTO updateRequest, int currentUserId)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return null; // User not found
                }

                // Check access control
                if (!await HasAccessToEntityAsync(user, currentUserId))
                {
                    return null; // User doesn't have access to update this user record
                }

                // Check if user type exists
                var userType = await _context.UserTypes.FindAsync(updateRequest.UserTypeId);
                if (userType == null)
                {
                    return null; // Invalid user type
                }

                // Update user properties
                user.FirstName = updateRequest.FirstName;
                user.LastName = updateRequest.LastName;
                user.PhoneNumber = updateRequest.PhoneNumber;
                user.ProfilePhoto = updateRequest.ProfilePhoto;
                user.UserTypeId = updateRequest.UserTypeId;
                user.IsActive = updateRequest.IsActive;
                user.IsEmailVerified = updateRequest.IsEmailVerified;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load user type for response
                user.UserType = userType;

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update user error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns>True if the password was changed successfully; otherwise, false.</returns>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDTO changePasswordRequest)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false; // User not found
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.CurrentPassword, user.PasswordHash))
                {
                    return false; // Invalid current password
                }

                // Hash new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resets a user's password - Super Admin can reset any password, users can reset their own.
        /// </summary>
        /// <param name="userId">The user ID whose password to reset.</param>
        /// <param name="resetPasswordRequest">The reset password request.</param>
        /// <param name="currentUserId">The ID of the user performing the reset.</param>
        /// <returns>True if the password was reset successfully; otherwise, false.</returns>
        public async Task<bool> ResetPasswordAsync(int userId, ResetPasswordRequestDTO resetPasswordRequest, int currentUserId)
        {
            try
            {
                // Get current user information
                var currentUser = await _context.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);

                if (currentUser == null)
                {
                    Console.WriteLine($"Password reset denied: Current user {currentUserId} not found");
                    return false;
                }

                // Check permissions: Super Admin can reset any password, users can reset their own
                bool isSuperAdmin = currentUser.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
                bool isOwnPassword = currentUserId == userId;
                
                if (!isSuperAdmin && !isOwnPassword)
                {
                    Console.WriteLine($"Password reset denied: User {currentUserId} can only reset their own password");
                    return false;
                }

                // Find the target user
                var targetUser = await _context.Users.FindAsync(userId);
                if (targetUser == null)
                {
                    Console.WriteLine($"Password reset failed: User {userId} not found");
                    return false; // User not found
                }

                // Hash new password
                var oldPasswordHash = targetUser.PasswordHash;
                targetUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.NewPassword);
                targetUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                string action = isOwnPassword ? "self-reset" : "admin-reset";
                Console.WriteLine($"Password reset successful: {action} by {currentUser.EmailAddress} for user {targetUser.EmailAddress} (ID: {userId})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset password error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a user by their ID with access control.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>True if the user was deleted successfully and accessible; otherwise, false.</returns>
        public async Task<bool> DeleteUserAsync(int id, int currentUserId)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false; // User not found
                }

                // Check access control
                if (!await HasAccessToEntityAsync(user, currentUserId))
                {
                    return false; // User doesn't have access to delete this user record
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete user error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="isActive">Whether to activate or deactivate the account.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public async Task<bool> SetUserActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false; // User not found
                }

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set user active status error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the user's last login time.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false; // User not found
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update last login error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if an email address is already registered.
        /// </summary>
        /// <param name="emailAddress">The email address to check.</param>
        /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates).</param>
        /// <returns>True if the email is already registered; otherwise, false.</returns>
        public async Task<bool> IsEmailRegisteredAsync(string emailAddress, int? excludeUserId = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.EmailAddress.ToLower() == emailAddress.ToLower());

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUserId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Check email registration error: {ex.Message}");
                return false;
            }
        }

        // Helper methods

        /// <summary>
        /// Maps a User entity to a UserResponseDTO.
        /// </summary>
        /// <param name="user">The user entity to map.</param>
        /// <returns>The mapped UserResponseDTO.</returns>
        private static UserResponseDTO MapToUserResponse(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                EmailAddress = user.EmailAddress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ProfilePhoto = user.ProfilePhoto,
                UserTypeId = user.UserTypeId,
                UserTypeName = user.UserType?.Name ?? string.Empty,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
