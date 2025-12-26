using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.TermsConditionsCRUD.DTOs;
using YopoBackend.Modules.TermsConditionsCRUD.Models;

namespace YopoBackend.Modules.TermsConditionsCRUD.Services
{
    public class TermsAndConditionsService : ITermsAndConditionsService
    {
        private readonly ApplicationDbContext _context;

        public TermsAndConditionsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TermsAndConditionResponseDTO>> GetAllAsync()
        {
            var items = await _context.TermsAndConditions
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return items.Select(MapToResponse).ToList();
        }

        public async Task<TermsAndConditionResponseDTO?> GetByIdAsync(int id)
        {
            var item = await _context.TermsAndConditions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TermsAndConditionId == id);

            return item == null ? null : MapToResponse(item);
        }

        public async Task<TermsAndConditionResponseDTO> CreateAsync(CreateTermsAndConditionDTO dto)
        {
            var entity = new TermsAndCondition
            {
                Title = (dto.Title ?? string.Empty).Trim(),
                UsedPlace = dto.UsedPlace.Trim(),
                Description = (dto.Description ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.TermsAndConditions.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<TermsAndConditionResponseDTO?> UpdateAsync(int id, UpdateTermsAndConditionDTO dto)
        {
            var entity = await _context.TermsAndConditions
                .FirstOrDefaultAsync(t => t.TermsAndConditionId == id);

            if (entity == null)
            {
                return null;
            }

            if (dto.Title != null)
            {
                entity.Title = dto.Title.Trim();
            }

            if (dto.Description != null)
            {
                entity.Description = dto.Description.Trim();
            }

            if (dto.UsedPlace != null)
            {
                entity.UsedPlace = dto.UsedPlace.Trim();
            }

            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.TermsAndConditions
                .FirstOrDefaultAsync(t => t.TermsAndConditionId == id);

            if (entity == null)
            {
                return false;
            }

            _context.TermsAndConditions.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        private static TermsAndConditionResponseDTO MapToResponse(TermsAndCondition entity)
        {
            return new TermsAndConditionResponseDTO
            {
                TermsAndConditionId = entity.TermsAndConditionId,
                Title = entity.Title,
                UsedPlace = entity.UsedPlace,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
