using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomCRUD.DTOs;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.IntercomCRUD.Services
{
    /// <summary>
    /// Service implementation for Intercom operations.
    /// Module ID: 9 (IntercomCRUD)
    /// </summary>
    public class IntercomService : BaseAccessControlService, IIntercomService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public IntercomService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetAllIntercomsAsync(int userId)
        {
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: true);
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetActiveIntercomsAsync(int userId)
        {
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: false, onlyActive: true);
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetInstalledIntercomsAsync(int userId)
        {
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: true, onlyInstalled: true);
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IntercomDto?> GetIntercomByIdAsync(int id, int userId)
        {
            var intercom = await _context.Intercoms
                .Include(i => i.Building)
                .FirstOrDefaultAsync(i => i.IntercomId == id);

            if (intercom == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(intercom, userId))
            {
                return null; // User doesn't have access to this intercom
            }

            return MapToDto(intercom);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetIntercomsByBuildingIdAsync(int buildingId, int userId)
        {
            // First verify user has access to this building
            if (!await ValidateBuildingAccessAsync(buildingId, userId))
            {
                return Enumerable.Empty<IntercomListDto>();
            }

            var query = _context.Intercoms
                .Include(i => i.Building)
                .Where(i => i.BuildingId == buildingId);

            // Apply access control
            query = await ApplyAccessControlAsync(query, userId);

            var intercoms = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetIntercomsWithCCTVAsync(int userId)
        {
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: true, hasCCTV: true);
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetIntercomsWithPinPadAsync(int userId)
        {
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: true, hasPinPad: true);
            return intercoms.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IntercomDto> CreateIntercomAsync(CreateIntercomDto createIntercomDto, int createdByUserId)
        {
            var intercom = new Intercom
            {
                Name = createIntercomDto.Name,
                Model = createIntercomDto.Model,
                Type = createIntercomDto.Type,
                Price = createIntercomDto.Price,
                IsInstalled = createIntercomDto.IsInstalled,
                Size = createIntercomDto.Size,
                Color = createIntercomDto.Color,
                IsActive = createIntercomDto.IsActive,
                DateInstalled = createIntercomDto.DateInstalled,
                ServiceDate = createIntercomDto.ServiceDate,
                OperatingSystem = createIntercomDto.OperatingSystem,
                BuildingId = createIntercomDto.BuildingId,
                Location = createIntercomDto.Location,
                HasCCTV = createIntercomDto.HasCCTV,
                IsPinPad = createIntercomDto.IsPinPad,
                HasTouchScreen = createIntercomDto.HasTouchScreen,
                HasRemoteAccess = createIntercomDto.HasRemoteAccess,
                IpAddress = createIntercomDto.IpAddress,
                MacAddress = createIntercomDto.MacAddress,
                FirmwareVersion = createIntercomDto.FirmwareVersion,
                WarrantyExpiryDate = createIntercomDto.WarrantyExpiryDate,
                Description = createIntercomDto.Description,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Intercoms.Add(intercom);
            await _context.SaveChangesAsync();

            // Load the building information for the response
            await _context.Entry(intercom)
                .Reference(i => i.Building)
                .LoadAsync();

            return MapToDto(intercom);
        }

        /// <inheritdoc/>
        public async Task<IntercomDto?> UpdateIntercomAsync(int id, UpdateIntercomDto updateIntercomDto, int userId)
        {
            var intercom = await _context.Intercoms
                .Include(i => i.Building)
                .FirstOrDefaultAsync(i => i.IntercomId == id);

            if (intercom == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(intercom, userId))
            {
                return null; // User doesn't have access to update this intercom
            }

            intercom.Name = updateIntercomDto.Name;
            intercom.Model = updateIntercomDto.Model;
            intercom.Type = updateIntercomDto.Type;
            intercom.Price = updateIntercomDto.Price;
            intercom.IsInstalled = updateIntercomDto.IsInstalled;
            intercom.Size = updateIntercomDto.Size;
            intercom.Color = updateIntercomDto.Color;
            intercom.IsActive = updateIntercomDto.IsActive;
            intercom.DateInstalled = updateIntercomDto.DateInstalled;
            intercom.ServiceDate = updateIntercomDto.ServiceDate;
            intercom.OperatingSystem = updateIntercomDto.OperatingSystem;
            intercom.BuildingId = updateIntercomDto.BuildingId;
            intercom.Location = updateIntercomDto.Location;
            intercom.HasCCTV = updateIntercomDto.HasCCTV;
            intercom.IsPinPad = updateIntercomDto.IsPinPad;
            intercom.HasTouchScreen = updateIntercomDto.HasTouchScreen;
            intercom.HasRemoteAccess = updateIntercomDto.HasRemoteAccess;
            intercom.IpAddress = updateIntercomDto.IpAddress;
            intercom.MacAddress = updateIntercomDto.MacAddress;
            intercom.FirmwareVersion = updateIntercomDto.FirmwareVersion;
            intercom.WarrantyExpiryDate = updateIntercomDto.WarrantyExpiryDate;
            intercom.Description = updateIntercomDto.Description;
            intercom.UpdatedAt = DateTime.UtcNow;

            _context.Intercoms.Update(intercom);
            await _context.SaveChangesAsync();

            return MapToDto(intercom);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteIntercomAsync(int id, int userId)
        {
            var intercom = await _context.Intercoms.FirstOrDefaultAsync(i => i.IntercomId == id);
            if (intercom == null)
            {
                return false;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(intercom, userId))
            {
                return false; // User doesn't have access to delete this intercom
            }

            _context.Intercoms.Remove(intercom);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IntercomExistsInBuildingAsync(string name, int buildingId, int? excludeId = null)
        {
            var query = _context.Intercoms.Where(i => i.Name.ToLower() == name.ToLower() && i.BuildingId == buildingId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.IntercomId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateBuildingAccessAsync(int buildingId, int userId)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId);
            if (building == null)
            {
                return false;
            }

            return await HasAccessToEntityAsync(building, userId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetIntercomsRequiringMaintenanceAsync(int userId, int monthsThreshold = 12)
        {
            var thresholdDate = DateTime.UtcNow.AddMonths(-monthsThreshold);
            
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: false);
            
            // Filter for intercoms that either never had maintenance or need maintenance
            var maintenanceRequired = intercoms.Where(i => 
                !i.ServiceDate.HasValue || i.ServiceDate.Value <= thresholdDate);
            
            return maintenanceRequired.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IntercomListDto>> GetIntercomsWithExpiringWarrantyAsync(int userId, int daysThreshold = 90)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            
            var intercoms = await GetIntercomsBasedOnUserAccess(userId, includeInactive: false);
            
            // Filter for intercoms with warranty expiring within the threshold
            var expiringWarranty = intercoms.Where(i => 
                i.WarrantyExpiryDate.HasValue && 
                i.WarrantyExpiryDate.Value <= thresholdDate &&
                i.WarrantyExpiryDate.Value >= DateTime.UtcNow);
            
            return expiringWarranty.Select(MapToListDto);
        }

        /// <inheritdoc/>
        public async Task<IntercomDto?> ToggleIntercomStatusAsync(int id, int userId)
        {
            var intercom = await _context.Intercoms
                .Include(i => i.Building)
                .FirstOrDefaultAsync(i => i.IntercomId == id);

            if (intercom == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(intercom, userId))
            {
                return null; // User doesn't have access to modify this intercom
            }

            intercom.IsActive = !intercom.IsActive;
            intercom.UpdatedAt = DateTime.UtcNow;

            _context.Intercoms.Update(intercom);
            await _context.SaveChangesAsync();

            return MapToDto(intercom);
        }

        /// <inheritdoc/>
        public async Task<IntercomDto?> UpdateServiceDateAsync(int id, DateTime serviceDate, int userId)
        {
            var intercom = await _context.Intercoms
                .Include(i => i.Building)
                .FirstOrDefaultAsync(i => i.IntercomId == id);

            if (intercom == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(intercom, userId))
            {
                return null; // User doesn't have access to modify this intercom
            }

            intercom.ServiceDate = serviceDate;
            intercom.UpdatedAt = DateTime.UtcNow;

            _context.Intercoms.Update(intercom);
            await _context.SaveChangesAsync();

            return MapToDto(intercom);
        }

        /// <summary>
        /// Gets intercoms based on user's access control settings with various filters.
        /// </summary>
        /// <param name="userId">The ID of the user requesting access.</param>
        /// <param name="includeInactive">Whether to include inactive intercoms.</param>
        /// <param name="onlyActive">Whether to only include active intercoms.</param>
        /// <param name="onlyInstalled">Whether to only include installed intercoms.</param>
        /// <param name="hasCCTV">Whether to only include intercoms with CCTV.</param>
        /// <param name="hasPinPad">Whether to only include intercoms with PIN pad.</param>
        /// <returns>List of intercoms the user has access to.</returns>
        private async Task<List<Intercom>> GetIntercomsBasedOnUserAccess(
            int userId, 
            bool includeInactive, 
            bool? onlyActive = null, 
            bool? onlyInstalled = null, 
            bool? hasCCTV = null, 
            bool? hasPinPad = null)
        {
            var query = _context.Intercoms
                .Include(i => i.Building)
                .AsQueryable();

            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);

            // Apply active/inactive filter
            if (onlyActive == true || (!includeInactive && onlyActive != false))
            {
                query = query.Where(i => i.IsActive);
            }

            // Apply installed filter
            if (onlyInstalled == true)
            {
                query = query.Where(i => i.IsInstalled);
            }

            // Apply CCTV filter
            if (hasCCTV == true)
            {
                query = query.Where(i => i.HasCCTV);
            }

            // Apply PIN pad filter
            if (hasPinPad == true)
            {
                query = query.Where(i => i.IsPinPad);
            }

            return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Maps an Intercom entity to an IntercomDto.
        /// </summary>
        /// <param name="intercom">The intercom entity to map.</param>
        /// <returns>The mapped intercom DTO.</returns>
        private static IntercomDto MapToDto(Intercom intercom)
        {
            return new IntercomDto
            {
                IntercomId = intercom.IntercomId,
                Name = intercom.Name,
                Model = intercom.Model,
                Type = intercom.Type,
                Price = intercom.Price,
                IsInstalled = intercom.IsInstalled,
                Size = intercom.Size,
                Color = intercom.Color,
                IsActive = intercom.IsActive,
                DateInstalled = intercom.DateInstalled,
                ServiceDate = intercom.ServiceDate,
                OperatingSystem = intercom.OperatingSystem,
                BuildingId = intercom.BuildingId,
                BuildingName = intercom.Building?.Name,
                Location = intercom.Location,
                HasCCTV = intercom.HasCCTV,
                IsPinPad = intercom.IsPinPad,
                HasTouchScreen = intercom.HasTouchScreen,
                HasRemoteAccess = intercom.HasRemoteAccess,
                IpAddress = intercom.IpAddress,
                MacAddress = intercom.MacAddress,
                FirmwareVersion = intercom.FirmwareVersion,
                WarrantyExpiryDate = intercom.WarrantyExpiryDate,
                Description = intercom.Description,
                CreatedBy = intercom.CreatedBy,
                CreatedAt = intercom.CreatedAt,
                UpdatedAt = intercom.UpdatedAt
            };
        }

        /// <summary>
        /// Maps an Intercom entity to an IntercomListDto.
        /// </summary>
        /// <param name="intercom">The intercom entity to map.</param>
        /// <returns>The mapped intercom list DTO.</returns>
        private static IntercomListDto MapToListDto(Intercom intercom)
        {
            return new IntercomListDto
            {
                IntercomId = intercom.IntercomId,
                Name = intercom.Name,
                Model = intercom.Model,
                Type = intercom.Type,
                IsInstalled = intercom.IsInstalled,
                IsActive = intercom.IsActive,
                BuildingId = intercom.BuildingId,
                BuildingName = intercom.Building?.Name,
                Location = intercom.Location,
                HasCCTV = intercom.HasCCTV,
                IsPinPad = intercom.IsPinPad,
                DateInstalled = intercom.DateInstalled,
                CreatedAt = intercom.CreatedAt
            };
        }
    }
}
