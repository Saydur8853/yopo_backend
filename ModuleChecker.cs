using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Constants;

namespace YopoBackend
{
    public class ModuleChecker
    {
        private readonly ApplicationDbContext _context;

        public ModuleChecker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CheckModulesAsync()
        {
            Console.WriteLine("=== CHECKING MODULES IN DATABASE ===");
            
            // Get all modules from database
            var dbModules = await _context.Modules.ToListAsync();
            
            Console.WriteLine($"Found {dbModules.Count} modules in database:");
            foreach (var module in dbModules)
            {
                var status = module.IsActive ? "✅ Active" : "❌ Inactive";
                Console.WriteLine($"   • ID: {module.Id}, Name: {module.Name}, Status: {status}");
            }

            Console.WriteLine("\n=== CHECKING MODULES IN CONSTANTS ===");
            Console.WriteLine($"Found {ModuleConstants.Modules.Count} modules in constants:");
            foreach (var module in ModuleConstants.Modules)
            {
                var status = module.Value.IsActive ? "✅ Active" : "❌ Inactive";
                Console.WriteLine($"   • ID: {module.Key}, Name: {module.Value.Name}, Status: {status}");
                
                // Check if this module exists in database
                var dbModule = dbModules.FirstOrDefault(m => m.Id == module.Key);
                if (dbModule == null)
                {
                    Console.WriteLine($"     ⚠️  WARNING: Module {module.Value.Name} (ID: {module.Key}) is missing from database!");
                }
            }

            Console.WriteLine("\n=== MODULE CHECK COMPLETE ===");
        }
    }
}
