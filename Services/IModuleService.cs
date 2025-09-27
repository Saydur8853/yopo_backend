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
        /// Gets modules with pagination and filtering.
        /// </summary>
        /// <param name="page">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <returns>Paginated list of modules.</returns>
        Task<ModuleListDto> GetModulesAsync(int page = 1, int pageSize = 10, bool? isActive = null);

        /// <summary>
        /// Initializes modules in the database based on constants.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        Task InitializeModulesAsync();
        Task<ModuleListDto> GetAllModulesAsync();
    }
}
