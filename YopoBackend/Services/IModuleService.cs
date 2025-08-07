using YopoBackend.DTOs;
using YopoBackend.Models;

namespace YopoBackend.Services
{
    /// <summary>
    /// Interface for module service operations.
    /// </summary>
    public interface IModuleService
    {
        /// <summary>
        /// Gets all modules.
        /// </summary>
        /// <returns>List of all modules.</returns>
        Task<ModuleListDto> GetAllModulesAsync();

        /// <summary>
        /// Gets a module by its ID.
        /// </summary>
        /// <param name="id">The module ID.</param>
        /// <returns>The module if found, null otherwise.</returns>
        Task<ModuleDto?> GetModuleByIdAsync(int id);

        /// <summary>
        /// Gets all active modules.
        /// </summary>
        /// <returns>List of active modules.</returns>
        Task<ModuleListDto> GetActiveModulesAsync();

        /// <summary>
        /// Initializes modules in the database based on constants.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        Task InitializeModulesAsync();
    }
}
