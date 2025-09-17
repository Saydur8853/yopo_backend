using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Constants;

namespace YopoBackend.Services
{
    /// <summary>
    /// Base service class that provides common data access control functionality for all modules.
    /// This ensures consistent implementation of "OWN" vs "ALL" access control across the system.
    /// </summary>
    public abstract class BaseAccessControlService
    {
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BaseAccessControlService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        protected BaseAccessControlService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the user's data access control setting from their user type.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The DataAccessControl setting ("OWN", "ALL", "PM", or null if user not found).</returns>
        protected async Task<string?> GetUserDataAccessControlAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.UserType?.DataAccessControl;
        }

        /// <summary>
        /// Applies access control filtering to a queryable based on the user's access control settings.
        /// </summary>
        /// <typeparam name="T">The entity type that implements ICreatedByEntity.</typeparam>
        /// <param name="query">The base query to filter.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="userDataAccessControl">The user's data access control setting (optional - will be fetched if not provided).</param>
        /// <returns>The filtered query based on access control rules.</returns>
        protected async Task<IQueryable<T>> ApplyAccessControlAsync<T>(IQueryable<T> query, int userId, string? userDataAccessControl = null) 
            where T : ICreatedByEntity
        {
            // Get user's access control setting if not provided
            userDataAccessControl ??= await GetUserDataAccessControlAsync(userId);

            // Apply access control based on setting
            if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_OWN)
            {
                // User can only access data they created
                return query.Where(e => e.CreatedBy == userId);
            }
            else if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_PM)
            {
                // PM access control: User can access data within their PM ecosystem
                var pmEcosystemUserIds = await GetPMEcosystemUserIdsAsync(userId);
                return query.Where(e => pmEcosystemUserIds.Contains(e.CreatedBy));
            }
            
            // If DataAccessControl is "ALL" or null, return all data (no additional filtering)
            return query;
        }

        /// <summary>
        /// Checks if a user has access to a specific entity based on their access control settings.
        /// </summary>
        /// <typeparam name="T">The entity type that implements ICreatedByEntity.</typeparam>
        /// <param name="entity">The entity to check access for.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="userDataAccessControl">The user's data access control setting (optional - will be fetched if not provided).</param>
        /// <returns>True if the user has access to the entity, false otherwise.</returns>
        protected async Task<bool> HasAccessToEntityAsync<T>(T entity, int userId, string? userDataAccessControl = null) 
            where T : ICreatedByEntity
        {
            // Get user's access control setting if not provided
            userDataAccessControl ??= await GetUserDataAccessControlAsync(userId);

            // Check access based on setting
            if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_OWN)
            {
                // User can only access data they created
                return entity.CreatedBy == userId;
            }
            else if (userDataAccessControl == UserTypeConstants.DATA_ACCESS_PM)
            {
                // PM access control: User can access data within their PM ecosystem
                var pmEcosystemUserIds = await GetPMEcosystemUserIdsAsync(userId);
                return pmEcosystemUserIds.Contains(entity.CreatedBy);
            }
            
            // If DataAccessControl is "ALL" or null, user has access to all data
            return true;
        }

        /// <summary>
        /// Gets user information including their access control settings.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user with their user type information, or null if not found.</returns>
        protected async Task<User?> GetUserWithAccessControlAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Validates that a user exists and is active.
        /// </summary>
        /// <param name="userId">The ID of the user to validate.</param>
        /// <returns>True if the user exists and is active, false otherwise.</returns>
        protected async Task<bool> ValidateUserAsync(int userId)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == userId && u.IsActive);
        }

        /// <summary>
        /// Gets all user IDs that belong to the same PM ecosystem as the given user.
        /// This includes the PM user and all users created by that PM.
        /// </summary>
        /// <param name="userId">The ID of the user to find the PM ecosystem for.</param>
        /// <returns>List of user IDs in the same PM ecosystem.</returns>
        protected async Task<List<int>> GetPMEcosystemUserIdsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<int>();

            // Find the Property Manager for this user's ecosystem
            var propertyManagerId = await FindPropertyManagerForUserAsync(userId);
            
            if (propertyManagerId == null)
                return new List<int> { userId }; // If no PM found, user can only see their own data

            // Get all users in this PM's ecosystem (PM + all users created by the PM)
            var ecosystemUserIds = await _context.Users
                .Where(u => u.Id == propertyManagerId || u.CreatedBy == propertyManagerId)
                .Select(u => u.Id)
                .ToListAsync();

            return ecosystemUserIds;
        }

        /// <summary>
        /// Finds the Property Manager ID for a given user.
        /// If the user is a PM, returns their own ID. If user was created by a PM, returns the PM's ID.
        /// </summary>
        /// <param name="userId">The user ID to find the PM for.</param>
        /// <returns>The Property Manager ID, or null if not found.</returns>
        protected async Task<int?> FindPropertyManagerForUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            // If user is a Property Manager themselves
            if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                return user.Id;
            }

            // If user was created by a Property Manager, find that PM
            var creator = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == user.CreatedBy);

            if (creator?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                return creator.Id;
            }

            // Recursively check up the creation chain to find the PM
            if (creator != null)
            {
                return await FindPropertyManagerForUserAsync(creator.Id);
            }

            return null;
        }

        /// <summary>
        /// Checks if a user belongs to a specific Property Manager's ecosystem.
        /// </summary>
        /// <param name="userId">The user to check.</param>
        /// <param name="propertyManagerId">The Property Manager ID.</param>
        /// <returns>True if the user belongs to the PM's ecosystem, false otherwise.</returns>
        protected async Task<bool> IsUserInPMEcosystemAsync(int userId, int propertyManagerId)
        {
            var pmEcosystemIds = await GetPMEcosystemUserIdsAsync(propertyManagerId);
            return pmEcosystemIds.Contains(userId);
        }
    }

    /// <summary>
    /// Interface that entities must implement to support access control based on creator.
    /// </summary>
    public interface ICreatedByEntity
    {
        /// <summary>
        /// Gets the ID of the user who created this entity.
        /// </summary>
        int CreatedBy { get; }
    }
}
