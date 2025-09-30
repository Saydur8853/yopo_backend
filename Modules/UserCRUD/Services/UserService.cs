using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Utils;

namespace YopoBackend.Modules.UserCRUD.Services
{
    /// <summary>
    /// Service for managing users including authentication and CRUD operations.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    public class UserService : BaseAccessControlService, IUserService
    {
        private readonly IJwtService _jwtService;
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Initializes a new instance of the UserService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="jwtService">The JWT service for token generation.</param>
        /// <param name="customerService">The customer service for managing customer records.</param>
        public UserService(ApplicationDbContext context, IJwtService jwtService, ICustomerService customerService) : base(context)
        {
            _jwtService = jwtService;
            _customerService = customerService;
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
                if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    Console.WriteLine("Login attempt with empty email or password");
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.UserType)
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequest.Email.ToLower());

                if (user == null)
                {
                    Console.WriteLine($"Login attempt with non-existent email: {loginRequest.Email}");
                    return null; // User not found
                }

                if (!user.IsActive)
                {
                    Console.WriteLine($"Login attempt with inactive account: {loginRequest.Email}");
                    return null; // Account is deactivated
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    Console.WriteLine($"Login attempt with invalid password for: {loginRequest.Email}");
                    return null; // Invalid password
                }

                // Pre-load buildings for Property Managers so login response includes buildings
                List<UserBuildingDto>? userBuildings = null;
                if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                {
                    userBuildings = await _context.Buildings
                        .Include(b => b.Customer)
                        .Where(b => b.CustomerId == user.Id && b.IsActive)
                        .Select(b => new UserBuildingDto
                        {
                            BuildingId = b.BuildingId,
                            BuildingName = b.Name,
                            BuildingAddress = b.Address,
                            CustomerName = b.Customer.CustomerName,
                            CompanyName = b.Customer.CompanyName,
                            CompanyAddress = b.Customer.CompanyAddress,
                            UserId = b.CustomerId
                        })
                        .ToListAsync();
                }

                // Generate JWT token with IP and device info if available
                var (token, expiresAt) = await _jwtService.GenerateTokenAsync(user, null, null);

                Console.WriteLine($"Successful login for user: {user.Email}");
                return new AuthenticationResponseDTO
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = MapToUserResponse(user, userBuildings),
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error for {loginRequest.Email}: {ex.Message}");
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
                if (await IsEmailRegisteredAsync(registerRequest.Email))
                {
                    return null; // Email already registered
                }

                // Check if this is the first user in the system
                var isFirstUser = !await _context.Users.AnyAsync();
                
                int userTypeId;
                string? invitationCompanyName = null;
                int? inviterUserId = null;
                string? inviterUserName = null;
                
                if (isFirstUser)
                {
                    // First user becomes Super Admin
                    userTypeId = UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
                    Console.WriteLine($"First user registration - assigning Super Admin role to: {registerRequest.Email}");
                    // For self-registration (no invitation), mark inviter as self
                    inviterUserName = "self";
                }
                else
                {
                    // Check if email has a valid invitation
                    var invitation = await _context.Invitations
                        .Include(i => i.UserType)
                        .FirstOrDefaultAsync(i => i.EmailAddress.ToLower() == registerRequest.Email.ToLower() 
                                                 && i.ExpiryTime > DateTime.UtcNow);
                    
                    if (invitation == null)
                    {
                        // No valid invitation found
                        Console.WriteLine($"Registration denied for {registerRequest.Email} - no valid invitation");
                        throw new UnauthorizedAccessException("You are not invited. Please contact with Authority.");
                    }
                    
                    userTypeId = invitation.UserTypeId;
                    invitationCompanyName = invitation.CompanyName;

                    // Capture inviter details to store in the new user record
                    inviterUserId = invitation.CreatedBy;
                    var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Id == inviterUserId);
                    inviterUserName = inviter?.Name;

                    Console.WriteLine($"User registration with invitation - assigning user type {invitation.UserType?.Name} to: {registerRequest.Email}; invited by ID {inviterUserId} name '{inviterUserName}'");
                    
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

                // Process profile photo (optional)
                byte[]? profilePhotoBytes = null;
                string? profilePhotoMimeType = null;
                
                if (!string.IsNullOrEmpty(registerRequest.ProfilePhotoBase64))
                {
                    var validationResult = ImageUtils.ValidateBase64Image(registerRequest.ProfilePhotoBase64);
                    if (!validationResult.IsValid)
                    {
                        throw new ArgumentException($"Invalid profile photo: {validationResult.ErrorMessage}");
                    }
                    
                    profilePhotoBytes = validationResult.ImageBytes;
                    profilePhotoMimeType = validationResult.MimeType;
                }
                // Profile photo is optional, so we don't throw an error if it's not provided

                // Create new user
                var user = new User
                {
                    Email = registerRequest.Email.ToLower(),
                    PasswordHash = passwordHash,
                    Name = registerRequest.Name,
                    Address = registerRequest.Address,
                    PhoneNumber = registerRequest.PhoneNumber,
                    ProfilePhoto = profilePhotoBytes,
                    ProfilePhotoMimeType = profilePhotoMimeType,
                    UserTypeId = userTypeId,
                    // Set inviter info only if registered via an invitation
                    InviteById = inviterUserId,
                    InviteByName = inviterUserName,
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedBy = 0, // Will be set to the user's own ID after creation
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Update CreatedBy to the user's own ID for self-registration
                user.CreatedBy = user.Id;

                // If this was a self-registration (no invitation), set inviter to self
                if (isFirstUser)
                {
                    user.InviteById = user.Id;
                    user.InviteByName = "self";
                }

                await _context.SaveChangesAsync();

                // Create Customer record if user is a Property Manager
                if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                {
                    var customer = await _customerService.CreateCustomerAsync(user, invitationCompanyName);
                    if (customer != null)
                    {
                        Console.WriteLine($"Customer record created for Property Manager {user.Email} (Customer ID: {customer.CustomerId}) with company: {invitationCompanyName ?? "N/A"}");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Failed to create customer record for Property Manager {user.Email}");
                    }
                }

                // Reload user with full relationships for response
                var registeredUser = await _context.Users
                    .Include(u => u.UserType)
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (registeredUser == null) return null;

                // Generate JWT token
                var (token, expiresAt) = await _jwtService.GenerateTokenAsync(registeredUser);

                var message = isFirstUser ? "Registration successful - You are now the Super Administrator!" : "Registration successful";
                
                return new AuthenticationResponseDTO
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = MapToUserResponse(registeredUser),
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
                var query = _context.Users
                    .Include(u => u.UserType)
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .AsQueryable();

                // Apply access control
                query = await ApplyAccessControlAsync(query, currentUserId);

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(u => u.Email.ToLower().Contains(search) ||
                                           u.Name.ToLower().Contains(search));
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
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Pre-load buildings for Property Managers to avoid N+1 queries
                var buildingsLookup = new Dictionary<int, List<UserBuildingDto>>();
                var propertyManagerIds = users
                    .Where(u => u.UserType?.Name == "Property Manager")
                    .Select(u => u.Id)
                    .ToList();

                if (propertyManagerIds.Any())
                {
                    var allBuildings = await _context.Buildings
                        .Include(b => b.Customer)
                        .Where(b => propertyManagerIds.Contains(b.CustomerId) && b.IsActive)
                        .Select(b => new UserBuildingDto
                        {
                            BuildingId = b.BuildingId,
                            BuildingName = b.Name,
                            BuildingAddress = b.Address,
                            CustomerName = b.Customer.CustomerName,
                            CompanyName = b.Customer.CompanyName,
                            CompanyAddress = b.Customer.CompanyAddress,
                            UserId = b.CustomerId // Add UserId to group by
                        })
                        .ToListAsync();

                    buildingsLookup = allBuildings
                        .GroupBy(b => b.UserId)
                        .ToDictionary(g => g.Key, g => g.ToList());
                }

                return new UserListResponseDTO
                {
                    Users = users.Select(u => MapToUserResponse(u, buildingsLookup.GetValueOrDefault(u.Id))).ToList(),
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
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
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
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == emailAddress.ToLower());

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
                if (await IsEmailRegisteredAsync(createRequest.Email))
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

                // Process profile photo if provided
                byte[]? profilePhotoBytes = null;
                string? profilePhotoMimeType = null;
                
                if (!string.IsNullOrEmpty(createRequest.ProfilePhotoBase64))
                {
                    var validationResult = ImageUtils.ValidateBase64Image(createRequest.ProfilePhotoBase64);
                    if (!validationResult.IsValid)
                    {
                        throw new ArgumentException($"Invalid profile photo: {validationResult.ErrorMessage}");
                    }
                    
                    profilePhotoBytes = validationResult.ImageBytes;
                    profilePhotoMimeType = validationResult.MimeType;
                }

                // Create new user
                var user = new User
                {
                    Email = createRequest.Email.ToLower(),
                    PasswordHash = passwordHash,
                    Name = createRequest.Name,
                    Address = createRequest.Address,
                    PhoneNumber = createRequest.PhoneNumber,
                    ProfilePhoto = profilePhotoBytes,
                    ProfilePhotoMimeType = profilePhotoMimeType,
                    UserTypeId = createRequest.UserTypeId,
                    IsActive = createRequest.IsActive,
                    IsEmailVerified = createRequest.IsEmailVerified,
                    CreatedBy = createdByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Reload user with full relationships for response
                var createdUser = await _context.Users
                    .Include(u => u.UserType)
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                return createdUser != null ? MapToUserResponse(createdUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create user error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates an existing user's profile with access control.
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

                // Check if email is being changed and if it's already taken by another user
                if (!string.IsNullOrEmpty(updateRequest.Email) && updateRequest.Email.ToLower() != user.Email.ToLower())
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == updateRequest.Email.ToLower() && u.Id != id);
                    if (existingUser != null)
                    {
                        throw new ArgumentException("Email address is already registered by another user.");
                    }
                }

                // Process profile photo if provided
                if (updateRequest.ProfilePhotoBase64 != null)
                {
                    if (!string.IsNullOrEmpty(updateRequest.ProfilePhotoBase64))
                    {
                        var validationResult = ImageUtils.ValidateBase64Image(updateRequest.ProfilePhotoBase64);
                        if (!validationResult.IsValid)
                        {
                            throw new ArgumentException($"Invalid profile photo: {validationResult.ErrorMessage}");
                        }
                        
                        user.ProfilePhoto = validationResult.ImageBytes;
                        user.ProfilePhotoMimeType = validationResult.MimeType;
                    }
                    else if (updateRequest.ProfilePhotoBase64 == string.Empty) // Explicitly removing photo
                    {
                        user.ProfilePhoto = null;
                        user.ProfilePhotoMimeType = null;
                    }
                }
                // If ProfilePhotoBase64 is null, don't change the existing photo

                // Update user properties only if provided (partial update support)
                if (!string.IsNullOrEmpty(updateRequest.Email))
                {
                    user.Email = updateRequest.Email.ToLower();
                }
                if (!string.IsNullOrEmpty(updateRequest.Name))
                {
                    user.Name = updateRequest.Name;
                }
                if (updateRequest.Address != null)
                {
                    user.Address = updateRequest.Address;
                }
                if (updateRequest.PhoneNumber != null)
                {
                    user.PhoneNumber = updateRequest.PhoneNumber;
                }
                
                // Update password if provided
                if (!string.IsNullOrEmpty(updateRequest.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateRequest.Password);
                }
                
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload user with full relationships for response
                var updatedUser = await _context.Users
                    .Include(u => u.UserType)
                        .ThenInclude(ut => ut!.ModulePermissions)
                            .ThenInclude(mp => mp.Module)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                return updatedUser != null ? MapToUserResponse(updatedUser) : null;
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
                Console.WriteLine($"Password reset successful: {action} by {currentUser.Email} for user {targetUser.Email} (ID: {userId})");
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

        // Last login tracking removed since LastLoginAt property was removed from User model

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
                var query = _context.Users.Where(u => u.Email.ToLower() == emailAddress.ToLower());

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

        /// <summary>
        /// Updates an existing user by email address with access control.
        /// </summary>
        /// <param name="email">The user email to update.</param>
        /// <param name="updateRequest">The update user request.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>The updated user if successful and accessible; otherwise, null.</returns>
        public async Task<UserResponseDTO?> UpdateUserByEmailAsync(string email, UpdateUserRequestDTO updateRequest, int currentUserId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return null; // User not found
                }

                return await UpdateUserAsync(user.Id, updateRequest, currentUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update user by email error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a user by their email address with access control.
        /// </summary>
        /// <param name="email">The user email to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>True if the user was deleted successfully and accessible; otherwise, false.</returns>
        public async Task<bool> DeleteUserByEmailAsync(string email, int currentUserId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return false; // User not found
                }

                return await DeleteUserAsync(user.Id, currentUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete user by email error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Changes a user's password by email.
        /// </summary>
        /// <param name="email">The user email.</param>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns>True if the password was changed successfully; otherwise, false.</returns>
        public async Task<bool> ChangePasswordByEmailAsync(string email, ChangePasswordRequestDTO changePasswordRequest)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return false; // User not found
                }

                return await ChangePasswordAsync(user.Id, changePasswordRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password by email error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resets a user's password by email (Super Admin only - doesn't require current password).
        /// </summary>
        /// <param name="email">The user email whose password to reset.</param>
        /// <param name="resetPasswordRequest">The reset password request.</param>
        /// <param name="currentUserId">The ID of the user performing the reset (must be Super Admin).</param>
        /// <returns>True if the password was reset successfully; otherwise, false.</returns>
        public async Task<bool> ResetPasswordByEmailAsync(string email, ResetPasswordRequestDTO resetPasswordRequest, int currentUserId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return false; // User not found
                }

                return await ResetPasswordAsync(user.Id, resetPasswordRequest, currentUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset password by email error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Activates or deactivates a user account by email.
        /// </summary>
        /// <param name="email">The user email.</param>
        /// <param name="isActive">Whether to activate or deactivate the account.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public async Task<bool> SetUserActiveStatusByEmailAsync(string email, bool isActive)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return false; // User not found
                }

                return await SetUserActiveStatusAsync(user.Id, isActive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set user active status by email error: {ex.Message}");
                return false;
            }
        }

        // Helper methods

        /// <summary>
        /// Maps a User entity to a UserResponseDTO.
        /// </summary>
        /// <param name="user">The user entity to map.</param>
        /// <returns>The mapped UserResponseDTO.</returns>
        private UserResponseDTO MapToUserResponse(User user)
        {
            return MapToUserResponse(user, null);
        }

        /// <summary>
        /// Maps a User entity to a UserResponseDTO with optional pre-loaded buildings to avoid N+1 queries.
        /// </summary>
        /// <param name="user">The user entity to map.</param>
        /// <param name="userBuildings">Optional pre-loaded buildings for the user.</param>
        /// <returns>The mapped UserResponseDTO.</returns>
        private UserResponseDTO MapToUserResponse(User user, List<UserBuildingDto>? userBuildings)
        {
            // Get permitted modules from user type
            var permittedModules = new List<PermittedModuleDto>();
            if (user.UserType?.ModulePermissions != null)
            {
                permittedModules = user.UserType.ModulePermissions
                    .Where(utmp => utmp.IsActive && utmp.Module.IsActive)
                    .Select(utmp => new PermittedModuleDto
                    {
                        ModuleId = utmp.Module.Id.ToString(),
                        ModuleName = utmp.Module.Name
                    })
                    .OrderBy(pm => pm.ModuleId)
                    .ToList();
            }

            // Use pre-loaded buildings if provided, otherwise return empty list to avoid N+1 queries
            var buildings = userBuildings ?? new List<UserBuildingDto>();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                UserTypeId = user.UserTypeId,
                UserTypeName = user.UserType?.Name ?? string.Empty,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PermittedModules = permittedModules,
                Buildings = buildings,
                InviteById = user.InviteById,
                InviteByName = user.InviteByName,
                ProfilePhotoBase64 = ImageUtils.ConvertToBase64DataUrl(user.ProfilePhoto, user.ProfilePhotoMimeType)
            };
        }
    }
}
