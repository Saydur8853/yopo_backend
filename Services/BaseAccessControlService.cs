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
        
        // Cache for user access control data to prevent N+1 queries
        private readonly Dictionary<int, (User User, string? DataAccessControl)> _userCache = new();
        private readonly Dictionary<int, List<int>> _pmEcosystemCache = new();

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
            var (_, dataAccessControl) = await GetUserCacheDataAsync(userId);
            return dataAccessControl;
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
            var (user, _) = await GetUserCacheDataAsync(userId);
            return user;
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
            // Check cache first
            if (_pmEcosystemCache.TryGetValue(userId, out var cachedEcosystem))
            {
                return cachedEcosystem;
            }

            var (user, _) = await GetUserCacheDataAsync(userId);
            if (user == null)
            {
                var emptyResult = new List<int>();
                _pmEcosystemCache[userId] = emptyResult;
                return emptyResult;
            }

            // Find the Property Manager for this user's ecosystem
            var propertyManagerId = await FindPropertyManagerForUserAsync(userId);
            
            if (propertyManagerId == null)
            {
                var singleUserResult = new List<int> { userId }; // If no PM found, user can only see their own data
                _pmEcosystemCache[userId] = singleUserResult;
                return singleUserResult;
            }

            // Get all users in this PM's ecosystem:
            // - the PM themselves
            // - any users invited by the PM (InviteById)
            // - any users directly created by the PM (legacy records using CreatedBy)
            var ecosystemUserIds = await _context.Users
                .Where(u => u.Id == propertyManagerId || u.InviteById == propertyManagerId || u.CreatedBy == propertyManagerId)
                .Select(u => u.Id)
                .ToListAsync();

            // Cache the result
            _pmEcosystemCache[userId] = ecosystemUserIds;
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
            return await FindPropertyManagerForUserAsync(userId, new HashSet<int>());
        }

        /// <summary>
        /// Internal method to find the Property Manager ID with visited tracking to prevent infinite recursion.
        /// </summary>
        /// <param name="userId">The user ID to find the PM for.</param>
        /// <param name="visited">Set of already visited user IDs to prevent infinite loops.</param>
        /// <returns>The Property Manager ID, or null if not found.</returns>
        private async Task<int?> FindPropertyManagerForUserAsync(int userId, HashSet<int> visited)
        {
            // Prevent infinite loops by checking if we've already visited this user
            if (visited.Contains(userId))
                return null;
            
            visited.Add(userId);
            
            var (user, _) = await GetUserCacheDataAsync(userId);
            if (user == null)
                return null;

            // If user is a Property Manager themselves
            if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                return user.Id;
            }

            // Prefer invitation chain: if user was invited by someone, follow inviter to find the PM
            if (user.InviteById.HasValue)
            {
                var (inviter, _) = await GetUserCacheDataAsync(user.InviteById.Value);
                if (inviter != null)
                {
                    if (inviter.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
                    {
                        return inviter.Id;
                    }
                    if (!visited.Contains(inviter.Id))
                    {
                        var pmIdViaInviter = await FindPropertyManagerForUserAsync(inviter.Id, visited);
                        if (pmIdViaInviter != null)
                            return pmIdViaInviter;
                    }
                }
            }

            // Fallback to creation chain (legacy)
            var (creator, _) = await GetUserCacheDataAsync(user.CreatedBy);
            if (creator?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                return creator.Id;
            }

            // Recursively check up the creation chain to find the PM
            if (creator != null && !visited.Contains(creator.Id))
            {
                return await FindPropertyManagerForUserAsync(creator.Id, visited);
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

        /// <summary>
        /// Internal method to get user data from cache or database.
        /// This prevents N+1 queries by caching user data for the duration of the request.
        /// </summary>
        /// <param name="userId">The user ID to fetch.</param>
        /// <returns>Tuple of User and DataAccessControl, or (null, null) if not found.</returns>
        private async Task<(User?, string?)> GetUserCacheDataAsync(int userId)
        {
            // Check cache first
            if (_userCache.TryGetValue(userId, out var cachedData))
            {
                return (cachedData.User, cachedData.DataAccessControl);
            }

            // Fetch from database if not in cache
            var user = await _context.Users
                .AsNoTracking() // Use AsNoTracking for better performance since we're only reading
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var dataAccessControl = user?.UserType?.DataAccessControl;

            // Cache the result
            _userCache[userId] = (user, dataAccessControl);

            return (user, dataAccessControl);
        }

        /// <summary>
        /// Clears the internal cache. Call this method when user data might have changed
        /// or at the end of a request to free memory.
        /// </summary>
        protected void ClearCache()
        {
            _userCache.Clear();
            _pmEcosystemCache.Clear();
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
