using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Models;
using YopoBackend.Services;
using System.Text.RegularExpressions;

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
            
            // Apply custom access control for user types
            query = await ApplyUserTypeAccessControlAsync(query, currentUserId);

            var userTypes = await query.ToListAsync();
            return userTypes.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<UserTypeListResponseDTO> GetUserTypesAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null)
        {
            var query = _context.UserTypes
                .Include(ut => ut.ModulePermissions)
                    .ThenInclude(mp => mp.Module)
                .AsQueryable();

            // Apply custom access control for user types
            query = await ApplyUserTypeAccessControlAsync(query, currentUserId);

            // Filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(ut => ut.Name.ToLower().Contains(lower));
            }
            if (isActive.HasValue)
            {
                query = query.Where(ut => ut.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(ut => ut.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new UserTypeListResponseDTO
            {
                UserTypes = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDto>> GetActiveUserTypesAsync(int currentUserId)
        {
            var query = _context.UserTypes
                .Include(ut => ut.ModulePermissions.Where(mp => mp.IsActive))
                .ThenInclude(mp => mp.Module)
                .Where(ut => ut.IsActive)
                .AsQueryable();
            
            // Apply custom access control for user types
            query = await ApplyUserTypeAccessControlAsync(query, currentUserId);

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
            // Check Super Admin restrictions first
            if (!await IsSuperAdminOperationAllowedAsync(createdByUserId, "create"))
            {
                throw new UnauthorizedAccessException("Only Super Admin users are allowed to create user types.");
            }

            // Check if the creator is a Property Manager
            var creatorUser = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == createdByUserId);

            // Security rule: Property Managers cannot create user types related to "Property Manager"
            if (creatorUser?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                if (IsProhibitedPropertyManagerName(createUserTypeDto.Name))
                {
                    throw new UnauthorizedAccessException("You don't have permission to make this user type.");
                }
                // Additional security rule: PM cannot grant access to core admin modules
                if (createUserTypeDto.ModuleIds.Any(id => id == ModuleConstants.USER_TYPE_MODULE_ID || id == ModuleConstants.INVITATION_MODULE_ID || id == ModuleConstants.USER_MODULE_ID))
                {
                    throw new UnauthorizedAccessException("Property Managers cannot grant access to UserTypeCRUD, InvitationCRUD, or UserCRUD modules.");
                }
            }

            string dataAccessControl = createUserTypeDto.DataAccessControl;
            
            // If created by Property Manager, automatically set DataAccessControl to "PM"
            if (creatorUser?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                dataAccessControl = UserTypeConstants.DATA_ACCESS_PM;
                Console.WriteLine($"UserType '{createUserTypeDto.Name}' created by Property Manager - automatically setting DataAccessControl to 'PM'");
            }

            var userType = new UserType
            {
                Name = createUserTypeDto.Name,
                Description = createUserTypeDto.Description,
                DataAccessControl = dataAccessControl,
                IsActive = true,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTypes.Add(userType);
            await _context.SaveChangesAsync();

            // Add module permissions: explicit list or auto-defaults
            if (createUserTypeDto.ModuleIds.Any())
            {
                await UpdateUserTypeModulePermissionsAsync(userType.Id, createUserTypeDto.ModuleIds, createdByUserId);
            }
            else
            {
                // Auto-assign defaults based on DataAccessControl/name for non-SuperAdmin types
                var defaultIds = GetDefaultModuleIdsForUserType(userType);
                if (defaultIds.Any())
                {
                    await UpdateUserTypeModulePermissionsAsync(userType.Id, defaultIds, createdByUserId);
                }
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

            // Protect essential system user types from critical changes
            if (id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID || id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                // Prevent changing the name of essential system user types
                if (updateUserTypeDto.Name != userType.Name)
                {
                    throw new UnauthorizedAccessException($"The name of the '{userType.Name}' user type cannot be changed as it is essential to the system.");
                }
                
                // Prevent deactivating essential system user types
                if (!updateUserTypeDto.IsActive)
                {
                    throw new UnauthorizedAccessException($"The '{userType.Name}' user type cannot be deactivated as it is essential to the system.");
                }
                
                // Prevent changing DataAccessControl of Super Admin from "ALL"
                if (id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID && !string.IsNullOrEmpty(updateUserTypeDto.DataAccessControl) && updateUserTypeDto.DataAccessControl != "ALL")
                {
                    throw new UnauthorizedAccessException("The Super Admin user type must maintain 'ALL' data access control.");
                }
                
                // Prevent changing DataAccessControl of Property Manager from "OWN"
                if (id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && !string.IsNullOrEmpty(updateUserTypeDto.DataAccessControl) && updateUserTypeDto.DataAccessControl != "OWN")
                {
                    throw new UnauthorizedAccessException("The Property Manager user type must maintain 'OWN' data access control.");
                }
            }

            // Security rule on update as well: PM cannot rename to a prohibited name
            var updatingUser = await _context.Users.Include(u => u.UserType).FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (updatingUser?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                if (IsProhibitedPropertyManagerName(updateUserTypeDto.Name))
                {
                    throw new UnauthorizedAccessException("You don't have permission to make this user type.");
                }
                // Additional security rule: PM cannot grant access to core admin modules
                if (updateUserTypeDto.ModuleIds.Any(id => id == ModuleConstants.USER_TYPE_MODULE_ID || id == ModuleConstants.INVITATION_MODULE_ID || id == ModuleConstants.USER_MODULE_ID))
                {
                    throw new UnauthorizedAccessException("Property Managers cannot grant access to UserTypeCRUD, InvitationCRUD, or UserCRUD modules.");
                }
            }

            userType.Name = updateUserTypeDto.Name;
            userType.Description = updateUserTypeDto.Description;
            if (!string.IsNullOrEmpty(updateUserTypeDto.DataAccessControl))
            {
                userType.DataAccessControl = updateUserTypeDto.DataAccessControl;
            }
            userType.IsActive = updateUserTypeDto.IsActive;
            userType.UpdatedAt = DateTime.UtcNow;

            _context.UserTypes.Update(userType);
            await _context.SaveChangesAsync();

            // Update module permissions
            await UpdateUserTypeModulePermissionsAsync(id, updateUserTypeDto.ModuleIds, currentUserId);

            return await GetUserTypeByIdAsync(id, currentUserId);
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> DeleteUserTypeAsync(int id, int currentUserId)
        {
            // Prevent deletion of essential system user types
            if (id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID || id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                throw new UnauthorizedAccessException("The 'Super Admin' and 'Property Manager' user types cannot be deleted as they are essential to the system.");
            }

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
                return null; // User doesn't have access to delete this user type
            }

            // Check if there are users still using this user type
            var usersUsingThisType = await _context.Users
                .Where(u => u.UserTypeId == id)
                .Select(u => new { u.Id, u.Name, u.Email, u.IsActive })
                .ToListAsync();

            if (usersUsingThisType.Any())
            {
                var activeUsers = usersUsingThisType.Where(u => u.IsActive).ToList();
                var inactiveUsers = usersUsingThisType.Where(u => !u.IsActive).ToList();
                
                var errorMessage = $"Cannot delete user type '{userType.Name}' because {usersUsingThisType.Count} user(s) are still assigned to it.";
                
                if (activeUsers.Any())
                {
                    errorMessage += $" Active users ({activeUsers.Count}): {string.Join(", ", activeUsers.Take(3).Select(u => $"{u.Name} ({u.Email})"))}";
                    if (activeUsers.Count > 3)
                    {
                        errorMessage += $" and {activeUsers.Count - 3} more";
                    }
                    errorMessage += ".";
                }
                
                if (inactiveUsers.Any())
                {
                    errorMessage += $" Inactive users ({inactiveUsers.Count}): {string.Join(", ", inactiveUsers.Take(3).Select(u => $"{u.Name} ({u.Email})"))}";
                    if (inactiveUsers.Count > 3)
                    {
                        errorMessage += $" and {inactiveUsers.Count - 3} more";
                    }
                    errorMessage += ".";
                }
                
                errorMessage += " Please reassign these users to a different user type before deleting this one.";
                
                throw new InvalidOperationException(errorMessage);
            }

            // Create DTO before deletion
            var deletedUserTypeDto = MapToDto(userType);

            try
            {
                // First, remove all module permissions
                var permissions = await _context.UserTypeModulePermissions
                    .Where(p => p.UserTypeId == id)
                    .ToListAsync();

                _context.UserTypeModulePermissions.RemoveRange(permissions);

                // Then remove the user type
                _context.UserTypes.Remove(userType);
                await _context.SaveChangesAsync();

                return deletedUserTypeDto;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY constraint") == true ||
                                             ex.InnerException?.Message.Contains("foreign key constraint") == true ||
                                             ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                // Re-check for users in case they were created between our check and the delete attempt
                var newUsersCheck = await _context.Users
                    .Where(u => u.UserTypeId == id)
                    .Select(u => new { u.Name, u.Email })
                    .Take(5)
                    .ToListAsync();
                
                if (newUsersCheck.Any())
                {
                    throw new InvalidOperationException(
                        $"Cannot delete user type '{userType.Name}' because users are still assigned to it. " +
                        $"Users: {string.Join(", ", newUsersCheck.Select(u => $"{u.Name} ({u.Email})"))}. " +
                        "Please reassign these users to a different user type before deleting this one.");
                }
                
                // Generic foreign key constraint message if we can't identify the specific constraint
                throw new InvalidOperationException(
                    $"Cannot delete user type '{userType.Name}' because it is referenced by other records in the database. " +
                    "Please ensure all related records are removed or updated before deleting this user type.");
            }
            catch (Exception ex)
            {
                // Log the original exception for debugging purposes
                Console.WriteLine($"Error deleting user type {id}: {ex}");
                
                // Re-throw with more context
                throw new InvalidOperationException(
                    $"An error occurred while deleting user type '{userType.Name}'. " +
                    $"Error details: {ex.Message}", ex);
            }
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
        public async Task<bool> UpdateUserTypeModulePermissionsAsync(int userTypeId, List<int> moduleIds, int actingUserId)
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

            // Enforce restriction: Property Managers cannot grant access to UserTypeCRUD, InvitationCRUD, or UserCRUD
            var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actingUserId);
            if (actingUser?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                var prohibited = new HashSet<int>
                {
                    ModuleConstants.USER_TYPE_MODULE_ID,
                    ModuleConstants.INVITATION_MODULE_ID,
                    ModuleConstants.USER_MODULE_ID
                };
                if (moduleIds.Any(id => prohibited.Contains(id)))
                {
                    throw new UnauthorizedAccessException("Property Managers cannot grant access to UserTypeCRUD, InvitationCRUD, or UserCRUD modules.");
                }
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
                    .Include(ut => ut.ModulePermissions)
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
                                          userTypeInfo.Value.Id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID ? "OWN" : 
                                          userTypeInfo.Value.Id == UserTypeConstants.FRONT_DESK_OFFICER_USER_TYPE_ID ? "PM" : 
                                          userTypeInfo.Value.Id == UserTypeConstants.TENANT_USER_TYPE_ID ? "OWN" : "ALL",
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
                    else
                    {
                        // Assign opinionated defaults for non-all-access system types
                        var defaultIds = GetDefaultModuleIdsForUserType(newUserType);
                        if (defaultIds.Any())
                        {
                            await UpdateUserTypeModulePermissionsAsync(newUserType.Id, defaultIds, createdByUserId);
                        }
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

                    // Ensure Tenant always has OWN data access control
                    if (userTypeInfo.Value.Id == UserTypeConstants.TENANT_USER_TYPE_ID && existingUserType.DataAccessControl != "OWN")
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
                    else
                    {
                        // Ensure defaults exist for non-all-access system types
                        var defaultIds = GetDefaultModuleIdsForUserType(existingUserType);
                        if (defaultIds.Any())
                        {
                            await UpdateUserTypeModulePermissionsAsync(existingUserType.Id, defaultIds, existingUserType.CreatedBy);
                        }
                        else
                        {
                            await RemoveAllModuleAccessAsync(userTypeInfo.Value.Id);
                        }
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
        /// Removes all module access permissions for a specific user type.
        /// This is used during initialization to clean up user types that should not have all module access.
        /// </summary>
        /// <param name="userTypeId">The ID of the user type.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RemoveAllModuleAccessAsync(int userTypeId)
        {
            // Get existing permissions for this user type
            var existingPermissions = await _context.UserTypeModulePermissions
                .Where(p => p.UserTypeId == userTypeId)
                .ToListAsync();

            if (existingPermissions.Any())
            {
                Console.WriteLine($"   Removing {existingPermissions.Count} module permissions for user type ID {userTypeId} (Super Admin will manage access)");
                _context.UserTypeModulePermissions.RemoveRange(existingPermissions);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Returns default module IDs for a given user type, based on name/DataAccessControl.
        /// Super Admin: handled elsewhere (all modules).
        /// </summary>
        private static List<int> GetDefaultModuleIdsForUserType(UserType ut)
        {
            // Constants for readability
            var B = ModuleConstants.BUILDING_MODULE_ID;
            var F = ModuleConstants.FLOOR_MODULE_ID;
            var U = ModuleConstants.UNIT_MODULE_ID;
            var A = ModuleConstants.AMENITY_MODULE_ID;
            var T = ModuleConstants.TENANT_MODULE_ID;
            var I = ModuleConstants.INVITATION_MODULE_ID;
            var USER = ModuleConstants.USER_MODULE_ID;
            var UTYPE = ModuleConstants.USER_TYPE_MODULE_ID;

            var name = ut.Name.Trim();
            // Defaults per requested flow
            if (ut.Id == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID || name.Equals(UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                return new List<int> { UTYPE, B, F, U, A, T, I, USER }; // PM: includes UserType and UserCRUD module
            }
            if (ut.Id == UserTypeConstants.FRONT_DESK_OFFICER_USER_TYPE_ID || name.Equals(UserTypeConstants.FRONT_DESK_OFFICER_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                return new List<int> { B, F, U, T }; // FDO
            }
            if (ut.Id == UserTypeConstants.TENANT_USER_TYPE_ID || name.Equals(UserTypeConstants.TENANT_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                return new List<int> { B, F, U, A, T }; // Tenant: building/floor/unit/amenity/tenant modules
            }
            // PM-created types (DataAccess=PM): baseline
            if (ut.DataAccessControl == UserTypeConstants.DATA_ACCESS_PM)
            {
                return new List<int> { B, F, U, T };
            }
            // Default for other custom types: none
            return new List<int>();
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
        /// Custom access control logic for UserType entities.
        /// Property Managers can see:
        /// 1. User types they created (DataAccessControl = OWN)
        /// 2. Their own user type (Property Manager)
        /// 3. Subordinate user types (Front Desk Officer, Tenant)
        /// </summary>
        private async Task<IQueryable<UserType>> ApplyUserTypeAccessControlAsync(IQueryable<UserType> query, int currentUserId)
        {
            var userDataAccessControl = await GetUserDataAccessControlAsync(currentUserId);
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);

            if (currentUser == null)
            {
                return query.Where(ut => false); // No access if user not found
            }

            // Super Admin (ALL access) - can see everything
            if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_ALL)
            {
                return query;
            }

            // Property Manager (OWN access) - can see subordinate types and types they created
            if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                return query.Where(ut => 
                    ut.CreatedBy == currentUserId || // User types they created
                    ut.Id == UserTypeConstants.FRONT_DESK_OFFICER_USER_TYPE_ID || // Subordinate
                    ut.Id == UserTypeConstants.TENANT_USER_TYPE_ID); // Subordinate
            }

            // For PM ecosystem users (DATA_ACCESS_PM)
            if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_PM)
            {
                var pmEcosystemUserIds = await GetPMEcosystemUserIdsAsync(currentUserId);
                return query.Where(ut => pmEcosystemUserIds.Contains(ut.CreatedBy));
            }

            // Default OWN access - can only see user types they created
            if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_OWN)
            {
                return query.Where(ut => ut.CreatedBy == currentUserId);
            }

            // Fallback - no access
            return query.Where(ut => false);
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

        /// <summary>
        /// Maps a UserType entity to a simplified UserTypeDto for Property Managers.
        /// Excludes moduleIds, moduleNames, and dataAccessControl fields.
        /// </summary>
        /// <param name="userType">The user type entity to map.</param>
        /// <returns>The mapped simplified user type DTO.</returns>
        private static UserTypeDto MapToSimplifiedDto(UserType userType)
        {
            return new UserTypeDto
            {
                Id = userType.Id,
                Name = userType.Name,
                Description = userType.Description,
                IsActive = userType.IsActive,
                CreatedAt = userType.CreatedAt,
                UpdatedAt = userType.UpdatedAt,
                // Set these to null so JsonIgnore will omit them from the response
                ModuleIds = null,
                ModuleNames = null,
                DataAccessControl = null
            };
        }

        /// <summary>
        /// Determines if a provided user type name is prohibited for Property Managers.
        /// Matches variations like "Property Manager", "Prop Manager", "PM", etc.
        /// </summary>
        private static bool IsProhibitedPropertyManagerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            // Normalize: lowercase and remove non-alphanumeric characters
            var normalized = Regex.Replace(name, "[^a-zA-Z0-9]", "").ToLowerInvariant();

            // Direct matches and common variants
            if (normalized == "pm") return true;
            if (normalized.Contains("propertymanager")) return true;
            if (normalized.Contains("propmanager")) return true;
            if (normalized == "propertymgr" || normalized.Contains("propertymgr")) return true;

            return false;
        }

        /// <summary>
        /// Quick check to determine if the given user is a Super Admin.
        /// </summary>
        private async Task<bool> IsUserSuperAdminAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
        }

        /// <summary>
        /// Validates if a user is allowed to perform user type operations.
        /// Only Super Admin may create user types.
        /// </summary>
        /// <param name="userId">The ID of the user performing the operation</param>
        /// <param name="operation">The operation being performed ("create", "update", "delete")</param>
        /// <param name="targetUserTypeId">Optional: The ID of the user type being modified (for update/delete operations)</param>
        /// <returns>True if the operation is allowed, false otherwise</returns>
        private async Task<bool> IsSuperAdminOperationAllowedAsync(int userId, string operation, int? targetUserTypeId = null)
        {
            // Get the user making the request
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            bool isSuperAdmin = user?.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;

            switch (operation.ToLowerInvariant())
            {
                case "create":
                    // Only Super Admin can create user types
                    return isSuperAdmin;

                case "update":
                case "delete":
                    // All users are allowed to attempt update/delete (subject to other access checks)
                    return true;

                default:
                    return true;
            }
        }

        /// <inheritdoc/>
        public async Task<UserTypeListResponseDTO> GetUserTypesWithFiltersAsync(
            int currentUserId, 
            int page = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? isActive = null, 
            int? id = null,
            string? sortBy = null, 
            bool isSortAscending = true,
            bool includePermissions = false, 
            int? moduleId = null,
            bool includeInactiveModules = false,
            bool includeUserCounts = false)
        {
            // Check if current user is a Property Manager
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            bool isPropertyManager = currentUser?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID;

            var query = _context.UserTypes.AsQueryable();

            if (includePermissions)
            {
                query = query.Include(ut => ut.ModulePermissions)
                             .ThenInclude(mp => mp.Module);
            }

            // Apply custom access control for user types
            query = await ApplyUserTypeAccessControlAsync(query, currentUserId);

            // Filters
            if (id.HasValue)
            {
                query = query.Where(ut => ut.Id == id.Value);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(ut => ut.Name.ToLower().Contains(lower) || 
                                        (ut.Description != null && ut.Description.ToLower().Contains(lower)));
            }
            if (isActive.HasValue)
            {
                query = query.Where(ut => ut.IsActive == isActive.Value);
            }
            if (moduleId.HasValue)
            {
                query = query.Where(ut => ut.ModulePermissions.Any(mp => mp.ModuleId == moduleId.Value && (includeInactiveModules || mp.IsActive)));
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLowerInvariant() switch
                {
                    "name" => isSortAscending ? query.OrderBy(ut => ut.Name) : query.OrderByDescending(ut => ut.Name),
                    "createdat" => isSortAscending ? query.OrderBy(ut => ut.CreatedAt) : query.OrderByDescending(ut => ut.CreatedAt),
                    "isactive" => isSortAscending ? query.OrderBy(ut => ut.IsActive) : query.OrderByDescending(ut => ut.IsActive),
                    _ => isSortAscending ? query.OrderBy(ut => ut.Id) : query.OrderByDescending(ut => ut.Id) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(ut => ut.CreatedAt);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userTypeDtos = new List<UserTypeDto>();
            foreach (var userType in items)
            {
                var dto = isPropertyManager ? MapToSimplifiedDto(userType) : MapToDto(userType);
                if (includeUserCounts)
                {
                    dto.UserCount = await _context.Users.CountAsync(u => u.UserTypeId == userType.Id);
                }
                userTypeDtos.Add(dto);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new UserTypeListResponseDTO
            {
                UserTypes = userTypeDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> GetUserTypeByIdWithDetailsAsync(int id, int currentUserId, bool includeUserCount = false)
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
            
            var dto = MapToDto(userType);
            if (includeUserCount)
            {
                dto.UserCount = await _context.Users.CountAsync(u => u.UserTypeId == userType.Id);
            }
            return dto;
        }
    }
}
