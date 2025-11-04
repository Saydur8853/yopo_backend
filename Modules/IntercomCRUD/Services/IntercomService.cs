using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Auth;
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

        private string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }

        private bool IsSuperAdmin() => string.Equals(GetUserRole(), Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

        private async Task<bool> CanManageBuildingAsync(int buildingId)
        {
            if (IsSuperAdmin()) return true;
            var userId = GetUserId();
            return await _context.UserBuildingPermissions.AnyAsync(p => p.UserId == userId && p.BuildingId == buildingId);
        }

        public async Task<(List<IntercomResponseDTO> intercoms, int totalRecords)> GetIntercomsAsync(
            int page, int pageSize, string? searchTerm, int? customerId, int? buildingId,
            bool? isActive, bool? isInstalled, string? intercomType, string? operatingSystem, bool? hasCCTV, bool? hasPinPad,
            DateTime? installedFrom, DateTime? installedTo, DateTime? serviceFrom, DateTime? serviceTo, decimal? priceMin, decimal? priceMax,
            string? color, string? model)
        {
            var query = _context.Set<Intercom>()
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Building)
                .Include(i => i.Amenity)
                .AsQueryable();

            // Access control by building - PM and FD only see their assigned buildings
            var userRole = GetUserRole();
            if (userRole != "SuperAdmin")
            {
                var userId = GetUserId();
                var userBuildings = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == userId)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                
                if (userBuildings.Any())
                {
                    query = query.Where(i => userBuildings.Contains(i.BuildingId));
                }
                else
                {
                    // User has no building permissions
                    return (new List<IntercomResponseDTO>(), 0);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(i => i.IntercomName.ToLower().Contains(term)
                    || (i.IntercomModel != null && i.IntercomModel.ToLower().Contains(term))
                    || (i.IntercomType != null && i.IntercomType.ToLower().Contains(term))
                    || (i.OperatingSystem != null && i.OperatingSystem.ToLower().Contains(term))
                    || (i.InstalledLocation != null && i.InstalledLocation.ToLower().Contains(term))
                );
            }

            if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId);
            if (buildingId.HasValue) query = query.Where(i => i.BuildingId == buildingId.Value);
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
            // Only SuperAdmin can create intercoms
            // Authorization: SuperAdmin only
            if (!IsSuperAdmin())
                return (false, "Only Super Admins can create intercoms.", null);

            // Validate references
            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId);
            if (building == null)
                return (false, $"Building with ID {dto.BuildingId} not found.", null);

            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
            if (!customerExists)
                return (false, $"Customer with ID {dto.CustomerId} not found.", null);

            // Cross-entity consistency: Building.CustomerId must match Intercom.CustomerId
            if (building.CustomerId != dto.CustomerId)
                return (false, "CustomerId does not match the Building's CustomerId.", null);

            if (dto.AmenityId.HasValue)
            {
                var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == dto.AmenityId.Value);
                if (!amenityExists) return (false, $"Amenity with ID {dto.AmenityId.Value} not found.", null);
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
                BuildingId = dto.BuildingId,
                AmenityId = dto.AmenityId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetUserId()
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Customer).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Building).LoadAsync();
            if (entity.AmenityId.HasValue)
            {
                await _context.Entry(entity).Reference(e => e.Amenity).LoadAsync();
            }

            return (true, "Intercom created successfully.", MapToResponse(entity));
        }

        public async Task<(bool Success, string Message, IntercomResponseDTO? Data)> UpdateIntercomAsync(int intercomId, UpdateIntercomDTO dto)
        {
            var entity = await _context.Set<Intercom>().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (entity == null)
                return (false, $"Intercom with ID {intercomId} not found.", null);

            // Authorization: SuperAdmin only
            if (!IsSuperAdmin())
                return (false, "Only Super Admins can update intercoms.", null);

            if (dto.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId.Value);
                if (!customerExists) return (false, $"Customer with ID {dto.CustomerId.Value} not found.", null);
                entity.CustomerId = dto.CustomerId.Value;
            }

            if (dto.BuildingId.HasValue)
            {
                var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId.Value);
                if (building == null) return (false, $"Building with ID {dto.BuildingId.Value} not found.", null);
                // Cross-entity consistency: if CustomerId is set or existing, it must match building's customer
                var effectiveCustomerId = dto.CustomerId ?? entity.CustomerId;
                if (building.CustomerId != effectiveCustomerId)
                    return (false, "CustomerId does not match the Building's CustomerId.", null);
                entity.BuildingId = dto.BuildingId.Value;
            }

            if (dto.AmenityId.HasValue)
            {
                var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == dto.AmenityId.Value);
                if (!amenityExists) return (false, $"Amenity with ID {dto.AmenityId.Value} not found.", null);
                entity.AmenityId = dto.AmenityId.Value;
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
            entity.UpdatedBy = GetUserId();
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Customer).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Building).LoadAsync();
            if (entity.AmenityId.HasValue)
            {
                await _context.Entry(entity).Reference(e => e.Amenity).LoadAsync();
            }

            return (true, "Intercom updated successfully.", MapToResponse(entity));
        }

        public async Task<(bool Success, string Message)> DeleteIntercomAsync(int intercomId)
        {
            var entity = await _context.Set<Intercom>().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (entity == null)
                return (false, $"Intercom with ID {intercomId} not found.");

            // Authorization: SuperAdmin only
            if (!IsSuperAdmin())
                return (false, "Only Super Admins can delete intercoms.");

            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return (true, "Intercom deleted successfully.");
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
                BuildingId = e.BuildingId,
                AmenityId = e.AmenityId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Customer = e.Customer != null ? new CustomerInfoDTO
                {
                    CustomerId = e.Customer.CustomerId,
                    CustomerName = e.Customer.CustomerName,
                    CompanyName = e.Customer.CompanyName,
                    CompanyAddress = e.Customer.CompanyAddress
                } : null,
                Building = e.Building != null ? new BuildingInfoDTO
                {
                    BuildingId = e.Building.BuildingId,
                    BuildingName = e.Building.Name,
                    BuildingAddress = e.Building.Address
                } : null,
                Amenity = e.Amenity != null ? new AmenityInfoDTO
                {
                    AmenityId = e.Amenity.AmenityId,
                    AmenityName = e.Amenity.Name,
                    AmenityType = e.Amenity.Type
                } : null
            };

            return resp;
        }
    }
}