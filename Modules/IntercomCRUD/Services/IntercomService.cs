using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomCRUD.DTOs;
using YopoBackend.Modules.IntercomCRUD.Models;

namespace YopoBackend.Modules.IntercomCRUD.Services
{
    public class IntercomService : IIntercomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IntercomService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(userId);
        }

        public async Task<(List<IntercomResponseDTO> intercoms, int totalRecords)> GetIntercomsAsync(
            int page, int pageSize, string? searchTerm, int? customerId, int? buildingId, int? unitId,
            bool? isActive, bool? isInstalled, string? intercomType, string? operatingSystem, bool? hasCCTV, bool? hasPinPad,
            DateTime? installedFrom, DateTime? installedTo, DateTime? serviceFrom, DateTime? serviceTo, decimal? priceMin, decimal? priceMax,
            string? color, string? model)
        {
            var query = _context.Set<Intercom>()
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Unit)!.ThenInclude(u => u!.Floor)
                .Include(i => i.Unit)!.ThenInclude(u => u!.Building)
                .AsQueryable();

            // Access control by building if none provided
            if (!buildingId.HasValue)
            {
                var userId = GetUserId();
                var userBuilding = await _context.UserBuildingPermissions.FirstOrDefaultAsync(p => p.UserId == userId);
                if (userBuilding != null)
                {
                    query = query.Where(i => i.Unit != null && i.Unit.BuildingId == userBuilding.BuildingId);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(i => i.IntercomName.ToLower().Contains(term)
                    || (i.IntercomModel != null && i.IntercomModel.ToLower().Contains(term))
                    || (i.IntercomType != null && i.IntercomType.ToLower().Contains(term))
                    || (i.OperatingSystem != null && i.OperatingSystem.ToLower().Contains(term))
                );
            }

            if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId);
            if (buildingId.HasValue) query = query.Where(i => i.Unit != null && i.Unit.BuildingId == buildingId.Value);
            if (unitId.HasValue) query = query.Where(i => i.UnitId == unitId.Value);
            if (isActive.HasValue) query = query.Where(i => i.IsActive == isActive.Value);
            if (isInstalled.HasValue) query = query.Where(i => i.IsInstalled == isInstalled.Value);
            if (!string.IsNullOrWhiteSpace(intercomType)) query = query.Where(i => i.IntercomType == intercomType);
            if (!string.IsNullOrWhiteSpace(operatingSystem)) query = query.Where(i => i.OperatingSystem == operatingSystem);
            if (hasCCTV.HasValue) query = query.Where(i => i.HasCCTV == hasCCTV.Value);
            if (hasPinPad.HasValue) query = query.Where(i => i.HasPinPad == hasPinPad.Value);
            if (installedFrom.HasValue) query = query.Where(i => i.DateInstalled >= installedFrom);
            if (installedTo.HasValue) query = query.Where(i => i.DateInstalled <= installedTo);
            if (serviceFrom.HasValue) query = query.Where(i => i.ServiceDate >= serviceFrom);
            if (serviceTo.HasValue) query = query.Where(i => i.ServiceDate <= serviceTo);
            if (priceMin.HasValue) query = query.Where(i => i.Price >= priceMin);
            if (priceMax.HasValue) query = query.Where(i => i.Price <= priceMax);
            if (!string.IsNullOrWhiteSpace(color)) query = query.Where(i => i.IntercomColor == color);
            if (!string.IsNullOrWhiteSpace(model)) query = query.Where(i => i.IntercomModel == model);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(i => i.IntercomName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = items.Select(MapToResponse).ToList();
            return (mapped, totalRecords);
        }

        public async Task<(bool Success, string Message, IntercomResponseDTO? Data)> CreateIntercomAsync(CreateIntercomDTO dto)
        {
            // Validate references
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
            if (!customerExists)
                return (false, $"Customer with ID {dto.CustomerId} not found.", null);

            if (dto.UnitId.HasValue)
            {
                var unitExists = await _context.Units.AnyAsync(u => u.UnitId == dto.UnitId.Value);
                if (!unitExists) return (false, $"Unit with ID {dto.UnitId.Value} not found.", null);
            }

            var entity = new Intercom
            {
                IntercomName = dto.IntercomName,
                IntercomModel = dto.IntercomModel,
                IntercomType = dto.IntercomType,
                Price = dto.Price,
                IsInstalled = dto.IsInstalled,
                IntercomSize = dto.IntercomSize,
                IntercomColor = dto.IntercomColor,
                IsActive = dto.IsActive,
                DateInstalled = dto.DateInstalled,
                ServiceDate = dto.ServiceDate,
                OperatingSystem = dto.OperatingSystem,
                InstalledLocation = dto.InstalledLocation,
                HasCCTV = dto.HasCCTV,
                HasPinPad = dto.HasPinPad,
                CustomerId = dto.CustomerId,
                UnitId = dto.UnitId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Customer).LoadAsync();
            if (entity.UnitId.HasValue)
            {
                await _context.Entry(entity).Reference(e => e.Unit).LoadAsync();
                if (entity.Unit != null)
                {
                    await _context.Entry(entity.Unit).Reference(u => u.Floor).LoadAsync();
                    await _context.Entry(entity.Unit).Reference(u => u.Building).LoadAsync();
                }
            }

            return (true, "Intercom created successfully.", MapToResponse(entity));
        }

        public async Task<(bool Success, string Message, IntercomResponseDTO? Data)> UpdateIntercomAsync(int intercomId, UpdateIntercomDTO dto)
        {
            var entity = await _context.Set<Intercom>().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (entity == null)
                return (false, $"Intercom with ID {intercomId} not found.", null);

            if (dto.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId.Value);
                if (!customerExists) return (false, $"Customer with ID {dto.CustomerId.Value} not found.", null);
                entity.CustomerId = dto.CustomerId.Value;
            }

            if (dto.UnitId.HasValue)
            {
                var unitExists = await _context.Units.AnyAsync(u => u.UnitId == dto.UnitId.Value);
                if (!unitExists) return (false, $"Unit with ID {dto.UnitId.Value} not found.", null);
                entity.UnitId = dto.UnitId.Value;
            }

            if (dto.IntercomName != null) entity.IntercomName = dto.IntercomName;
            if (dto.IntercomModel != null) entity.IntercomModel = dto.IntercomModel;
            if (dto.IntercomType != null) entity.IntercomType = dto.IntercomType;
            if (dto.Price.HasValue) entity.Price = dto.Price.Value;
            if (dto.IsInstalled.HasValue) entity.IsInstalled = dto.IsInstalled.Value;
            if (dto.IntercomSize != null) entity.IntercomSize = dto.IntercomSize;
            if (dto.IntercomColor != null) entity.IntercomColor = dto.IntercomColor;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
            if (dto.DateInstalled.HasValue) entity.DateInstalled = dto.DateInstalled.Value;
            if (dto.ServiceDate.HasValue) entity.ServiceDate = dto.ServiceDate.Value;
            if (dto.OperatingSystem != null) entity.OperatingSystem = dto.OperatingSystem;
            if (dto.InstalledLocation != null) entity.InstalledLocation = dto.InstalledLocation;
            if (dto.HasCCTV.HasValue) entity.HasCCTV = dto.HasCCTV.Value;
            if (dto.HasPinPad.HasValue) entity.HasPinPad = dto.HasPinPad.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Customer).LoadAsync();
            if (entity.UnitId.HasValue)
            {
                await _context.Entry(entity).Reference(e => e.Unit).LoadAsync();
                if (entity.Unit != null)
                {
                    await _context.Entry(entity.Unit).Reference(u => u.Floor).LoadAsync();
                    await _context.Entry(entity.Unit).Reference(u => u.Building).LoadAsync();
                }
            }

            return (true, "Intercom updated successfully.", MapToResponse(entity));
        }

        public async Task<(bool Success, string Message)> DeleteIntercomAsync(int intercomId)
        {
            var entity = await _context.Set<Intercom>().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (entity == null)
                return (false, $"Intercom with ID {intercomId} not found.");

            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return (true, "Intercom deleted successfully.");
        }

        public async Task<(bool Success, string Message, IntercomResponseDTO? Data)> AssignToUnitAsync(int intercomId, int unitId)
        {
            var entity = await _context.Set<Intercom>().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (entity == null)
                return (false, $"Intercom with ID {intercomId} not found.", null);

            var unit = await _context.Units.Include(u => u.Floor).Include(u => u.Building).FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit == null)
                return (false, $"Unit with ID {unitId} not found.", null);

            entity.UnitId = unitId;
            entity.IsInstalled = true;
            if (!entity.DateInstalled.HasValue) entity.DateInstalled = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Customer).LoadAsync();
            entity.Unit = unit;

            return (true, "Intercom assigned to unit successfully.", MapToResponse(entity));
        }

        private static IntercomResponseDTO MapToResponse(Intercom e)
        {
            var resp = new IntercomResponseDTO
            {
                IntercomId = e.IntercomId,
                IntercomName = e.IntercomName,
                IntercomModel = e.IntercomModel,
                IntercomType = e.IntercomType,
                Price = e.Price,
                IsInstalled = e.IsInstalled,
                IntercomSize = e.IntercomSize,
                IntercomColor = e.IntercomColor,
                IsActive = e.IsActive,
                DateInstalled = e.DateInstalled,
                ServiceDate = e.ServiceDate,
                OperatingSystem = e.OperatingSystem,
                InstalledLocation = e.InstalledLocation,
                HasCCTV = e.HasCCTV,
                HasPinPad = e.HasPinPad,
                CustomerId = e.CustomerId,
                UnitId = e.UnitId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Customer = e.Customer != null ? new CustomerInfoDTO
                {
                    CustomerId = e.Customer.CustomerId,
                    CustomerName = e.Customer.CustomerName,
                    CompanyName = e.Customer.CompanyName,
                    CompanyAddress = e.Customer.CompanyAddress
                } : null
            };

            if (e.Unit != null)
            {
                resp.Unit = new UnitInfoDTO { UnitId = e.Unit.UnitId, UnitNumber = e.Unit.UnitNumber };
                if (e.Unit.Building != null)
                {
                    resp.Building = new BuildingInfoDTO
                    {
                        BuildingId = e.Unit.Building.BuildingId,
                        BuildingName = e.Unit.Building.Name,
                        BuildingAddress = e.Unit.Building.Address
                    };
                }
                if (e.Unit.Floor != null)
                {
                    resp.Floor = new FloorInfoDTO
                    {
                        FloorId = e.Unit.Floor.FloorId,
                        Name = e.Unit.Floor.Name,
                        Number = e.Unit.Floor.Number
                    };
                }
            }

            return resp;
        }
    }
}