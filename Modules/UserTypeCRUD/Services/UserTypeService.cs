using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.UserTypeCRUD.Services
{
    /// <summary>
    /// Service implementation for UserType operations with Data Access Control.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    public class UserTypeService : BaseAccessControlService, IUserTypeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public UserTypeService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync(int currentUserId)
        {
            var query = _context.UserTypes
                .Include(ut => ut.ModulePermissions)
                .ThenInclude(mp => mp.Module)
                .AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var userTypes = await query.ToListAsync();
            return userTypes.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDto>> GetActiveUserTypesAsync(int currentUserId)
        {
            var query = _context.UserTypes
                .Include(ut => ut.ModulePermissions.Where(mp => mp.IsActive))
                .ThenInclude(mp => mp.Module)
                .Where(ut => ut.IsActive)
                .AsQueryable();
            
            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);
            
            var userTypes = await query.ToListAsync();
            return userTypes.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> GetUserTypeByIdAsync(int id, int currentUserId)
        {
            var userType = await _context.UserTypes
                .Include(ut => ut.ModulePermissions.Where(mp => mp.IsActive))
                .ThenInclude(mp => mp.Module)
                .FirstOrDefaultAsync(ut => ut.Id == id);

            if (userType == null)
            {
                return null;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(userType, currentUserId))
            {
                return null; // User doesn't have access to this user type
            }
            
            return MapToDto(userType);
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto, int createdByUserId)
        {
            var userType = new UserType
            {
                Name = createUserTypeDto.Name,
                Description = createUserTypeDto.Description,
                DataAccessControl = createUserTypeDto.DataAccessControl,
                IsActive = true,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTypes.Add(userType);
            await _context.SaveChangesAsync();

            // Add module permissions if provided
            if (createUserTypeDto.ModuleIds.Any())
            {
                await UpdateUserTypeModulePermissionsAsync(userType.Id, createUserTypeDto.ModuleIds);
            }

            return await GetUserTypeByIdAsync(userType.Id, createdByUserId) ?? throw new InvalidOperationException("Failed to retrieve created user type");
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> UpdateUserTypeAsync(int id, UpdateUserTypeDto updateUserTypeDto, int currentUserId)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
            {
                return null;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(userType, currentUserId))
            {
                return null; // User doesn't have access to update this user type
            }

            userType.Name = updateUserTypeDto.Name;
            userType.Description = updateUserTypeDto.Description;
            userType.DataAccessControl = updateUserTypeDto.DataAccessControl;
            userType.IsActive = updateUserTypeDto.IsActive;
            userType.UpdatedAt = DateTime.UtcNow;

            _context.UserTypes.Update(userType);
            await _context.SaveChangesAsync();

            // Update module permissions
            await UpdateUserTypeModulePermissionsAsync(id, updateUserTypeDto.ModuleIds);

            return await GetUserTypeByIdAsync(id, currentUserId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserTypeAsync(int id, int currentUserId)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
            {
                return false;
            }
            
            // Check access control
            if (!await HasAccessToEntityAsync(userType, currentUserId))
            {
                return false; // User doesn't have access to delete this user type
            }

            // First, remove all module permissions
            var permissions = await _context.UserTypeModulePermissions
                .Where(p => p.UserTypeId == id)
                .ToListAsync();

            _context.UserTypeModulePermissions.RemoveRange(permissions);

            // Then remove the user type
            _context.UserTypes.Remove(userType);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetUserTypeNamesAsync(bool activeOnly = true)
        {
            var query = _context.UserTypes.AsQueryable();
            
            if (activeOnly)
            {
                query = query.Where(ut => ut.IsActive);
            }

            var names = await query
                .Select(ut => ut.Name)
                .OrderBy(name => name)
                .ToListAsync();

            return names;
        }

        /// <inheritdoc/>
        public async Task<bool> UserTypeExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.UserTypes.Where(ut => ut.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(ut => ut.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeModulePermissionDto>> GetUserTypeModulePermissionsAsync(int userTypeId)
        {
            var permissions = await _context.UserTypeModulePermissions
                .Include(p => p.Module)
                .Include(p => p.UserType)
                .Where(p => p.UserTypeId == userTypeId)
                .ToListAsync();

            return permissions.Select(p => new UserTypeModulePermissionDto
            {
                UserTypeId = p.UserTypeId,
                UserTypeName = p.UserType.Name,
                ModuleId = p.ModuleId,
                ModuleName = p.Module.Name,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserTypeModulePermissionsAsync(int userTypeId, List<int> moduleIds)
        {
            // Validate that the user type exists
            if (!await _context.UserTypes.AnyAsync(ut => ut.Id == userTypeId))
            {
                return false;
            }

            // Validate that all module IDs are valid
            if (!await ValidateModuleIdsAsync(moduleIds))
            {
                return false;
            }

            // Remove existing permissions
            var existingPermissions = await _context.UserTypeModulePermissions
                .Where(p => p.UserTypeId == userTypeId)
                .ToListAsync();

            _context.UserTypeModulePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            var newPermissions = moduleIds.Select(moduleId => new UserTypeModulePermission
            {
                UserTypeId = userTypeId,
                ModuleId = moduleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            _context.UserTypeModulePermissions.AddRange(newPermissions);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateModuleIdsAsync(List<int> moduleIds)
        {
            if (!moduleIds.Any())
            {
                return true; // Empty list is valid
            }

            var existingModuleIds = await _context.Modules
                .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                .Select(m => m.Id)
                .ToListAsync();

            return existingModuleIds.Count == moduleIds.Count;
        }

        /// <inheritdoc/>
        public async Task InitializeDefaultUserTypesAsync()
        {
            // First, fix any existing user types that have CreatedBy = -1 or 0
            await FixExistingUserTypesCreatedByAsync();
            
            // Then proceed with normal initialization
            foreach (var userTypeInfo in UserTypeConstants.DefaultUserTypes)
            {
                var existingUserType = await _context.UserTypes
                    .FirstOrDefaultAsync(ut => ut.Id == userTypeInfo.Key);

                if (existingUserType == null)
                {
                    // For default user types during initialization, use the expected Super Admin user ID (1)
                    // This assumes the first Super Admin user will have ID 1, which is standard for identity columns
                    int createdByUserId = 1;
                    
                    Console.WriteLine($"   Creating user type '{userTypeInfo.Value.Name}' with CreatedBy={createdByUserId} (DefaultSuperAdmin)");
                    
                    // Create new default user type
                    var newUserType = new UserType
                    {
                        Id = userTypeInfo.Value.Id,
                        Name = userTypeInfo.Value.Name,
                        Description = userTypeInfo.Value.Description,
                        IsActive = userTypeInfo.Value.IsActive,
                        // Super Admin should always have ALL data access control, Property Manager should have OWN
                        DataAccessControl = userTypeInfo.Value.Id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID ? "ALL" : 
                                          userTypeInfo.Value.Id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID ? "OWN" : "ALL",
                        CreatedBy = createdByUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserTypes.Add(newUserType);
                    await _context.SaveChangesAsync();

                    // If this user type should have access to all modules, grant it
                    if (userTypeInfo.Value.HasAllModuleAccess)
                    {
                        await GrantAllModuleAccessAsync(userTypeInfo.Value.Id);
                    }
                }
                else
                {
                    // Update existing default user type if needed
                    var hasChanges = false;

                    if (existingUserType.Name != userTypeInfo.Value.Name)
                    {
                        existingUserType.Name = userTypeInfo.Value.Name;
                        hasChanges = true;
                    }

                    if (existingUserType.Description != userTypeInfo.Value.Description)
                    {
                        existingUserType.Description = userTypeInfo.Value.Description;
                        hasChanges = true;
                    }

                    if (existingUserType.IsActive != userTypeInfo.Value.IsActive)
                    {
                        existingUserType.IsActive = userTypeInfo.Value.IsActive;
                        hasChanges = true;
                    }

                    // Ensure Super Admin always has ALL data access control
                    if (userTypeInfo.Value.Id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID && existingUserType.DataAccessControl != "ALL")
                    {
                        existingUserType.DataAccessControl = "ALL";
                        hasChanges = true;
                    }
                    
                    // Ensure Property Manager always has OWN data access control
                    if (userTypeInfo.Value.Id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && existingUserType.DataAccessControl != "OWN")
                    {
                        existingUserType.DataAccessControl = "OWN";
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingUserType.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    // Ensure the user type has access to all modules if required
                    if (userTypeInfo.Value.HasAllModuleAccess)
                    {
                        await GrantAllModuleAccessAsync(userTypeInfo.Value.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Grants access to all active modules for a specific user type.
        /// </summary>
        /// <param name="userTypeId">The ID of the user type.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task GrantAllModuleAccessAsync(int userTypeId)
        {
            // Get all active module IDs
            var allActiveModuleIds = await _context.Modules
                .Where(m => m.IsActive)
                .Select(m => m.Id)
                .ToListAsync();

            if (allActiveModuleIds.Any())
            {
                // Get existing permissions for this user type
                var existingPermissions = await _context.UserTypeModulePermissions
                    .Where(p => p.UserTypeId == userTypeId)
                    .Select(p => p.ModuleId)
                    .ToListAsync();

                // Find missing permissions
                var missingModuleIds = allActiveModuleIds.Except(existingPermissions).ToList();

                if (missingModuleIds.Any())
                {
                    // Add missing permissions
                    var newPermissions = missingModuleIds.Select(moduleId => new UserTypeModulePermission
                    {
                        UserTypeId = userTypeId,
                        ModuleId = moduleId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });

                    _context.UserTypeModulePermissions.AddRange(newPermissions);
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Fixes existing user types that have incorrect CreatedBy values (0 or -1).
        /// This method should be called during initialization to correct legacy data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task FixExistingUserTypesCreatedByAsync()
        {
            try
            {
                // Find user types with CreatedBy = 0 or -1 (system/incorrect values)
                var userTypesToFix = await _context.UserTypes
                    .Where(ut => ut.CreatedBy <= 0)
                    .ToListAsync();

                if (!userTypesToFix.Any())
                {
                    Console.WriteLine("   No user types need CreatedBy fixes.");
                    return;
                }

                // Try to find the first Super Admin user
                var superAdminUser = await _context.Users
                    .Where(u => u.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID && u.IsActive)
                    .OrderBy(u => u.Id)  // Get the first one by ID
                    .FirstOrDefaultAsync();

                if (superAdminUser == null)
                {
                    Console.WriteLine($"   No Super Admin user found to fix {userTypesToFix.Count} user type(s).");
                    Console.WriteLine($"   Using default Super Admin user ID (1) for default user types.");
                    
                    // For default user types, use ID 1 (first Super Admin user ID)
                    foreach (var userType in userTypesToFix)
                    {
                        int createdById = userType.Id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID ? 1 : 1;  // Both use 1
                        Console.WriteLine($"     • Updating '{userType.Name}' CreatedBy from {userType.CreatedBy} to {createdById}");
                        userType.CreatedBy = createdById;
                        userType.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    Console.WriteLine($"   Fixing CreatedBy for {userTypesToFix.Count} user type(s) to Super Admin user ID {superAdminUser.Id}");
                    
                    foreach (var userType in userTypesToFix)
                    {
                        Console.WriteLine($"     • Updating '{userType.Name}' CreatedBy from {userType.CreatedBy} to {superAdminUser.Id}");
                        userType.CreatedBy = superAdminUser.Id;
                        userType.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Fixed CreatedBy for {userTypesToFix.Count} user type(s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ Error fixing user types CreatedBy: {ex.Message}");
                // Don't throw - this is a cleanup operation and shouldn't break initialization
            }
        }

        /// <summary>
        /// Maps a UserType entity to a UserTypeDto.
        /// </summary>
        /// <param name="userType">The user type entity to map.</param>
        /// <returns>The mapped user type DTO.</returns>
        private static UserTypeDto MapToDto(UserType userType)
        {
            var activePermissions = userType.ModulePermissions.Where(mp => mp.IsActive).ToList();

            return new UserTypeDto
            {
                Id = userType.Id,
                Name = userType.Name,
                Description = userType.Description,
                DataAccessControl = userType.DataAccessControl,
                IsActive = userType.IsActive,
                CreatedAt = userType.CreatedAt,
                UpdatedAt = userType.UpdatedAt,
                ModuleIds = activePermissions.Select(mp => mp.ModuleId).ToList(),
                ModuleNames = activePermissions.Select(mp => mp.Module.Name).ToList()
            };
        }
    }
}
