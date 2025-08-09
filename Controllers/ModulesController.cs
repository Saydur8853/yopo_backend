using Microsoft.AspNetCore.Mvc;
using YopoBackend.DTOs;
using YopoBackend.Services;

namespace YopoBackend.Controllers
{
    /// <summary>
    /// Controller for managing modules.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("01-Modules")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        /// <summary>
        /// Initializes a new instance of the ModulesController class.
        /// </summary>
        /// <param name="moduleService">The module service.</param>
        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Gets all modules in the system.
        /// </summary>
        /// <returns>List of all modules with their IDs, names, and metadata.</returns>
        /// <response code="200">Returns the list of modules</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ModuleListDto), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModuleListDto>> GetAllModules()
        {
            try
            {
                var modules = await _moduleService.GetAllModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching modules.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific module by its ID.
        /// </summary>
        /// <param name="id">The module ID to retrieve.</param>
        /// <returns>The module information if found.</returns>
        /// <response code="200">Returns the module information</response>
        /// <response code="404">If the module is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ModuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModuleDto>> GetModuleById(int id)
        {
            try
            {
                var module = await _moduleService.GetModuleByIdAsync(id);
                
                if (module == null)
                {
                    return NotFound(new { message = $"Module with ID {id} not found." });
                }

                return Ok(module);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the module.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all active modules in the system.
        /// </summary>
        /// <returns>List of active modules.</returns>
        /// <response code="200">Returns the list of active modules</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ModuleListDto), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModuleListDto>> GetActiveModules()
        {
            try
            {
                var modules = await _moduleService.GetActiveModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching active modules.", details = ex.Message });
            }
        }

        /// <summary>
        /// Initializes or updates modules in the database based on code constants.
        /// This is typically called during application startup.
        /// </summary>
        /// <returns>Success message.</returns>
        /// <response code="200">If modules were successfully initialized</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("initialize")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> InitializeModules()
        {
            try
            {
                await _moduleService.InitializeModulesAsync();
                return Ok(new { message = "Modules initialized successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while initializing modules.", details = ex.Message });
            }
        }
    }
}
