using YopoBackend.Modules.UserTypeCRUD.DTOs;
using YopoBackend.Modules.UserTypeCRUD.Models;

namespace YopoBackend.Modules.UserTypeCRUD.Services
{
    /// <summary>
    /// Interface for UserType service operations.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    public interface IUserTypeService
    {
        /// <summary>
        /// Gets all user types with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user type DTOs.</returns>
        Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync(int currentUserId);

        /// <summary>
        /// Gets all active user types with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active user type DTOs.</returns>
        Task<IEnumerable<UserTypeDto>> GetActiveUserTypesAsync(int currentUserId);

        /// <summary>
        /// Gets a user type by its ID with access control.
        /// </summary>
        /// <param name="id">The ID of the user type to retrieve.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user type DTO if found, otherwise null.</returns>
        Task<UserTypeDto?> GetUserTypeByIdAsync(int id, int currentUserId);

        /// <summary>
        /// Creates a new user type.
        /// </summary>
        /// <param name="createUserTypeDto">The data for creating the user type.</param>
        /// <param name="createdByUserId">The ID of the user creating this user type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created user type DTO.</returns>
        Task<UserTypeDto> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto, int createdByUserId);

        /// <summary>
        /// Updates an existing user type with access control.
        /// </summary>
        /// <param name="id">The ID of the user type to update.</param>
        /// <param name="updateUserTypeDto">The data for updating the user type.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated user type DTO if successful, otherwise null.</returns>
        Task<UserTypeDto?> UpdateUserTypeAsync(int id, UpdateUserTypeDto updateUserTypeDto, int currentUserId);

        /// <summary>
        /// Deletes a user type with access control.
        /// </summary>
        /// <param name="id">The ID of the user type to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, otherwise false.</returns>
        Task<bool> DeleteUserTypeAsync(int id, int currentUserId);

        /// <summary>
        /// Gets a list of existing user type names for dropdown/autocomplete functionality.
        /// </summary>
        /// <param name="activeOnly">Optional parameter to return only active user types (default: true).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user type names.</returns>
        Task<IEnumerable<string>> GetUserTypeNamesAsync(bool activeOnly = true);

        /// <summary>
        /// Checks if a user type with the specified name exists.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the name exists, otherwise false.</returns>
        Task<bool> UserTypeExistsAsync(string name, int? excludeId = null);

        /// <summary>
        /// Gets the module permissions for a specific user type.
        /// </summary>
        /// <param name="userTypeId">The ID of the user type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of module permission DTOs.</returns>
        Task<IEnumerable<UserTypeModulePermissionDto>> GetUserTypeModulePermissionsAsync(int userTypeId);

        /// <summary>
        /// Updates the module permissions for a specific user type.
        /// </summary>
        /// <param name="userTypeId">The ID of the user type.</param>
        /// <param name="moduleIds">The list of module IDs to assign to the user type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, otherwise false.</returns>
        Task<bool> UpdateUserTypeModulePermissionsAsync(int userTypeId, List<int> moduleIds);

        /// <summary>
        /// Validates if all provided module IDs exist in the system.
        /// </summary>
        /// <param name="moduleIds">The list of module IDs to validate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if all module IDs are valid, otherwise false.</returns>
        Task<bool> ValidateModuleIdsAsync(List<int> moduleIds);

        /// <summary>
        /// Initializes default user types (like Super Admin) in the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InitializeDefaultUserTypesAsync();
    }
}
