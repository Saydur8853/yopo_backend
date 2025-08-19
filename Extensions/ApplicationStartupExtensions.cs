using Microsoft.EntityFrameworkCore;
using YopoBackend.Modules.UserTypeCRUD.Services;
using YopoBackend.Modules.BuildingCRUD.Services;
using YopoBackend.Services;

namespace YopoBackend.Extensions
{
    /// <summary>
    /// Extension methods for application startup initialization.
    /// </summary>
    public static class ApplicationStartupExtensions
    {
        /// <summary>
        /// Initializes default data (modules and user types) during application startup.
        /// This ensures Super Admin always has access to all modules automatically.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>The web application for method chaining.</returns>
        public static async Task<WebApplication> InitializeDefaultDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // Initialize modules first
                var moduleService = services.GetRequiredService<IModuleService>();
                await moduleService.InitializeModulesAsync();

                // Initialize default user types (Super Admin gets all module access automatically)
                var userTypeService = services.GetRequiredService<IUserTypeService>();
                await userTypeService.InitializeDefaultUserTypesAsync();

                // Initialize sample buildings for demonstration - DISABLED
                // var buildingService = services.GetRequiredService<IBuildingService>();
                // await buildingService.InitializeSampleBuildingsAsync();

                Console.WriteLine("✅ Default data initialization completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during default data initialization: {ex.Message}");
                throw; // Re-throw to prevent app from starting with incomplete data
            }

            return app;
        }

        /// <summary>
        /// Initializes default data during application startup, but continues even if it fails.
        /// Use this version if you want the app to start even if initialization fails.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>The web application for method chaining.</returns>
        public static async Task<WebApplication> TryInitializeDefaultDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // Initialize modules first
                var moduleService = services.GetRequiredService<IModuleService>();
                Console.WriteLine("🔧 Initializing default modules...");
                await moduleService.InitializeModulesAsync();

                // Show which modules were initialized
                var modules = await moduleService.GetAllModulesAsync();
                Console.WriteLine($"📦 Initialized {modules.TotalCount} modules:");
                foreach (var module in modules.Modules)
                {
                    var status = module.IsActive ? "✅ Active" : "❌ Inactive";
                    Console.WriteLine($"   • {module.Name} (ID: {module.Id}) - {module.Description} - v{module.Version} [{status}]");
                }

                // Initialize default user types (Super Admin gets all module access automatically)
                var userTypeService = services.GetRequiredService<IUserTypeService>();
                Console.WriteLine("\n👥 Initializing default user types...");
                await userTypeService.InitializeDefaultUserTypesAsync();

                // Show which user types were initialized
                var context = services.GetRequiredService<YopoBackend.Data.ApplicationDbContext>();
                var userTypes = await context.UserTypes
                    .Include(ut => ut.ModulePermissions)
                    .ToListAsync();
                Console.WriteLine($"🔐 Initialized {userTypes.Count} user types:");
                foreach (var userType in userTypes)
                {
                    var status = userType.IsActive ? "✅ Active" : "❌ Inactive";
                    var moduleCount = userType.ModulePermissions?.Count ?? 0;
                    Console.WriteLine($"   • {userType.Name} (ID: {userType.Id}) - {userType.Description} [{status}] - Access to {moduleCount} modules");
                }

                // Initialize sample buildings for demonstration - DISABLED
                // var buildingService = services.GetRequiredService<IBuildingService>();
                // await buildingService.InitializeSampleBuildingsAsync();

                Console.WriteLine("\n✅ Default data initialization completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Default data initialization failed: {ex.Message}");
                // Don't throw - let the app continue to start
            }

            return app;
        }
    }
}
