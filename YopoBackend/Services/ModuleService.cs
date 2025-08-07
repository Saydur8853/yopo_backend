using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.DTOs;
using YopoBackend.Models;

namespace YopoBackend.Services
{
    /// <summary>
    /// Service for managing module operations.
    /// </summary>
    public class ModuleService : IModuleService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ModuleService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ModuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ModuleListDto> GetAllModulesAsync()
        {
            var modules = await _context.Modules
                .Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    Version = m.Version,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return new ModuleListDto
            {
                Modules = modules,
                TotalCount = modules.Count
            };
        }

        /// <inheritdoc />
        public async Task<ModuleDto?> GetModuleByIdAsync(int id)
        {
            var module = await _context.Modules
                .Where(m => m.Id == id)
                .Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    Version = m.Version,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return module;
        }

        /// <inheritdoc />
        public async Task<ModuleListDto> GetActiveModulesAsync()
        {
            var modules = await _context.Modules
                .Where(m => m.IsActive)
                .Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    Version = m.Version,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return new ModuleListDto
            {
                Modules = modules,
                TotalCount = modules.Count
            };
        }

        /// <inheritdoc />
        public async Task InitializeModulesAsync()
        {
            foreach (var moduleInfo in ModuleConstants.Modules)
            {
                var existingModule = await _context.Modules
                    .FirstOrDefaultAsync(m => m.Id == moduleInfo.Key);

                if (existingModule == null)
                {
                    // Create new module
                    var newModule = new Module
                    {
                        Id = moduleInfo.Value.Id,
                        Name = moduleInfo.Value.Name,
                        Description = moduleInfo.Value.Description,
                        Version = moduleInfo.Value.Version,
                        IsActive = moduleInfo.Value.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Modules.Add(newModule);
                }
                else
                {
                    // Update existing module if needed
                    var hasChanges = false;

                    if (existingModule.Name != moduleInfo.Value.Name)
                    {
                        existingModule.Name = moduleInfo.Value.Name;
                        hasChanges = true;
                    }

                    if (existingModule.Description != moduleInfo.Value.Description)
                    {
                        existingModule.Description = moduleInfo.Value.Description;
                        hasChanges = true;
                    }

                    if (existingModule.Version != moduleInfo.Value.Version)
                    {
                        existingModule.Version = moduleInfo.Value.Version;
                        hasChanges = true;
                    }

                    if (existingModule.IsActive != moduleInfo.Value.IsActive)
                    {
                        existingModule.IsActive = moduleInfo.Value.IsActive;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingModule.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
