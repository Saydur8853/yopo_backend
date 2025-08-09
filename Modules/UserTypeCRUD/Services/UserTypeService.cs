using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Models;

namespace YopoBackend.Modules.UserTypeCRUD.Services
{
    /// <summary>
    /// Service implementation for UserType operations.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    public class UserTypeService : IUserTypeService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public UserTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync()
        {
            var userTypes = await _context.UserTypes
                .Include(ut => ut.ModulePermissions)
                .ThenInclude(mp => mp.Module)
                .ToListAsync();

            return userTypes.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserTypeDto>> GetActiveUserTypesAsync()
        {
            var userTypes = await _context.UserTypes
                .Include(ut => ut.ModulePermissions.Where(mp => mp.IsActive))
                .ThenInclude(mp => mp.Module)
                .Where(ut => ut.IsActive)
                .ToListAsync();

            return userTypes.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> GetUserTypeByIdAsync(int id)
        {
            var userType = await _context.UserTypes
                .Include(ut => ut.ModulePermissions.Where(mp => mp.IsActive))
                .ThenInclude(mp => mp.Module)
                .FirstOrDefaultAsync(ut => ut.Id == id);

            return userType != null ? MapToDto(userType) : null;
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto)
        {
            var userType = new UserType
            {
                Name = createUserTypeDto.Name,
                Description = createUserTypeDto.Description,
                DataAccessControl = createUserTypeDto.DataAccessControl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTypes.Add(userType);
            await _context.SaveChangesAsync();

            // Add module permissions if provided
            if (createUserTypeDto.ModuleIds.Any())
            {
                await UpdateUserTypeModulePermissionsAsync(userType.Id, createUserTypeDto.ModuleIds);
            }

            return await GetUserTypeByIdAsync(userType.Id) ?? throw new InvalidOperationException("Failed to retrieve created user type");
        }

        /// <inheritdoc/>
        public async Task<UserTypeDto?> UpdateUserTypeAsync(int id, UpdateUserTypeDto updateUserTypeDto)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
            {
                return null;
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

            return await GetUserTypeByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserTypeAsync(int id)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
            {
                return false;
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
            foreach (var userTypeInfo in UserTypeConstants.DefaultUserTypes)
            {
                var existingUserType = await _context.UserTypes
                    .FirstOrDefaultAsync(ut => ut.Id == userTypeInfo.Key);

                if (existingUserType == null)
                {
                    // Create new default user type
                    var newUserType = new UserType
                    {
                        Id = userTypeInfo.Value.Id,
                        Name = userTypeInfo.Value.Name,
                        Description = userTypeInfo.Value.Description,
                        IsActive = userTypeInfo.Value.IsActive,
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
                IsActive = userType.IsActive,
                CreatedAt = userType.CreatedAt,
                UpdatedAt = userType.UpdatedAt,
                ModuleIds = activePermissions.Select(mp => mp.ModuleId).ToList(),
                ModuleNames = activePermissions.Select(mp => mp.Module.Name).ToList()
            };
        }
    }
}
