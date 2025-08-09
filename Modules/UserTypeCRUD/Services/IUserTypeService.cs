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
        /// Gets all user types.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user type DTOs.</returns>
        Task<IEnumerable<UserTypeDto>> GetAllUserTypesAsync();

        /// <summary>
        /// Gets all active user types.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active user type DTOs.</returns>
        Task<IEnumerable<UserTypeDto>> GetActiveUserTypesAsync();

        /// <summary>
        /// Gets a user type by its ID.
        /// </summary>
        /// <param name="id">The ID of the user type to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user type DTO if found, otherwise null.</returns>
        Task<UserTypeDto?> GetUserTypeByIdAsync(int id);

        /// <summary>
        /// Creates a new user type.
        /// </summary>
        /// <param name="createUserTypeDto">The data for creating the user type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created user type DTO.</returns>
        Task<UserTypeDto> CreateUserTypeAsync(CreateUserTypeDto createUserTypeDto);

        /// <summary>
        /// Updates an existing user type.
        /// </summary>
        /// <param name="id">The ID of the user type to update.</param>
        /// <param name="updateUserTypeDto">The data for updating the user type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated user type DTO if successful, otherwise null.</returns>
        Task<UserTypeDto?> UpdateUserTypeAsync(int id, UpdateUserTypeDto updateUserTypeDto);

        /// <summary>
        /// Deletes a user type.
        /// </summary>
        /// <param name="id">The ID of the user type to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, otherwise false.</returns>
        Task<bool> DeleteUserTypeAsync(int id);

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
    }
}
