using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;

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
        /// <returns>The DataAccessControl setting ("OWN", "ALL", or null if user not found).</returns>
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
            if (userDataAccessControl == "OWN")
            {
                // User can only access data they created
                return query.Where(e => e.CreatedBy == userId);
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
            if (userDataAccessControl == "OWN")
            {
                // User can only access data they created
                return entity.CreatedBy == userId;
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
