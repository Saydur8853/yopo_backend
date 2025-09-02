using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.VirtualKeyCRUD.DTOs;
using YopoBackend.Modules.VirtualKeyCRUD.Models;
using YopoBackend.Services;
using System.Security.Cryptography;
using System.Text;

namespace YopoBackend.Modules.VirtualKeyCRUD.Services
{
    /// <summary>
    /// Service implementation for Virtual Key operations.
    /// Module ID: 10 (VirtualKeyCRUD)
    /// </summary>
    public class VirtualKeyService : BaseAccessControlService, IVirtualKeyService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualKeyService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public VirtualKeyService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetAllVirtualKeysAsync(int userId)
        {
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetActiveVirtualKeysAsync(int userId)
        {
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: false);
            return virtualKeys.Where(vk => vk.IsActive && (vk.DateExpired == null || vk.DateExpired > DateTime.UtcNow)).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetExpiredVirtualKeysAsync(int userId)
        {
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.DateExpired != null && vk.DateExpired <= DateTime.UtcNow).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<VirtualKeyDto?> GetVirtualKeyByIdAsync(int id, int userId)
        {
            var virtualKey = await _context.VirtualKeys
                .Include(vk => vk.Building)
                .Include(vk => vk.Tenant)
                .Include(vk => vk.Intercom)
                .FirstOrDefaultAsync(vk => vk.KeyId == id);

            if (virtualKey == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(virtualKey, userId))
            {
                return null; // User doesn't have access to this virtual key
            }

            return MapToDto(virtualKey);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByBuildingIdAsync(int buildingId, int userId)
        {
            // First check if user has access to this building
            if (!await ValidateBuildingAccessAsync(buildingId, userId))
            {
                return Enumerable.Empty<VirtualKeyDto>();
            }

            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.BuildingId == buildingId).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByTenantIdAsync(int tenantId, int userId)
        {
            // First check if user has access to this tenant
            if (!await ValidateTenantAccessAsync(tenantId, userId))
            {
                return Enumerable.Empty<VirtualKeyDto>();
            }

            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.TenantId == tenantId).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByIntercomIdAsync(int intercomId, int userId)
        {
            // First check if user has access to this intercom
            if (!await ValidateIntercomAccessAsync(intercomId, userId))
            {
                return Enumerable.Empty<VirtualKeyDto>();
            }

            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.IntercomId == intercomId).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByStatusAsync(string status, int userId)
        {
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByTypeAsync(string type, int userId)
        {
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: true);
            return virtualKeys.Where(vk => vk.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<VirtualKeyDto> CreateVirtualKeyAsync(CreateVirtualKeyDto createVirtualKeyDto, int createdByUserId)
        {
            // Validate building access
            if (!await ValidateBuildingAccessAsync(createVirtualKeyDto.BuildingId, createdByUserId))
            {
                throw new UnauthorizedAccessException("You don't have access to create virtual keys for this building.");
            }

            // Validate tenant access if specified
            if (createVirtualKeyDto.TenantId.HasValue && !await ValidateTenantAccessAsync(createVirtualKeyDto.TenantId.Value, createdByUserId))
            {
                throw new UnauthorizedAccessException("You don't have access to create virtual keys for this tenant.");
            }

            // Validate intercom access if specified
            if (createVirtualKeyDto.IntercomId.HasValue && !await ValidateIntercomAccessAsync(createVirtualKeyDto.IntercomId.Value, createdByUserId))
            {
                throw new UnauthorizedAccessException("You don't have access to create virtual keys for this intercom.");
            }

            // Generate PIN code if not provided
            string pinCode = string.IsNullOrEmpty(createVirtualKeyDto.PinCode) 
                ? await GenerateUniquePinCodeAsync() 
                : createVirtualKeyDto.PinCode;

            var virtualKey = new VirtualKey
            {
                Description = createVirtualKeyDto.Description,
                DateCreated = DateTime.UtcNow,
                Type = createVirtualKeyDto.Type,
                Status = createVirtualKeyDto.Status,
                DateExpired = createVirtualKeyDto.DateExpired,
                AccessLocation = createVirtualKeyDto.AccessLocation,
                BuildingId = createVirtualKeyDto.BuildingId,
                IntercomId = createVirtualKeyDto.IntercomId,
                TenantId = createVirtualKeyDto.TenantId,
                PinCode = pinCode,
                QrCode = createVirtualKeyDto.QrCode,
                IsActive = createVirtualKeyDto.IsActive,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.VirtualKeys.Add(virtualKey);
            await _context.SaveChangesAsync();

            // Generate QR code after creation if not provided
            if (string.IsNullOrEmpty(virtualKey.QrCode))
            {
                virtualKey.QrCode = await GenerateQrCodeAsync(virtualKey.KeyId);
                _context.VirtualKeys.Update(virtualKey);
                await _context.SaveChangesAsync();
            }

            // Load navigation properties for the response
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Building)
                .LoadAsync();
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Tenant)
                .LoadAsync();
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Intercom)
                .LoadAsync();

            return MapToDto(virtualKey);
        }

        /// <inheritdoc/>
        public async Task<VirtualKeyDto?> UpdateVirtualKeyAsync(int id, UpdateVirtualKeyDto updateVirtualKeyDto, int userId)
        {
            var virtualKey = await _context.VirtualKeys.FirstOrDefaultAsync(vk => vk.KeyId == id);
            if (virtualKey == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(virtualKey, userId))
            {
                return null; // User doesn't have access to update this virtual key
            }

            // Validate building access
            if (!await ValidateBuildingAccessAsync(updateVirtualKeyDto.BuildingId, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to assign virtual keys to this building.");
            }

            // Validate tenant access if specified
            if (updateVirtualKeyDto.TenantId.HasValue && !await ValidateTenantAccessAsync(updateVirtualKeyDto.TenantId.Value, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to assign virtual keys to this tenant.");
            }

            // Validate intercom access if specified
            if (updateVirtualKeyDto.IntercomId.HasValue && !await ValidateIntercomAccessAsync(updateVirtualKeyDto.IntercomId.Value, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to assign virtual keys to this intercom.");
            }

            virtualKey.Description = updateVirtualKeyDto.Description;
            virtualKey.Type = updateVirtualKeyDto.Type;
            virtualKey.Status = updateVirtualKeyDto.Status;
            virtualKey.DateExpired = updateVirtualKeyDto.DateExpired;
            virtualKey.AccessLocation = updateVirtualKeyDto.AccessLocation;
            virtualKey.BuildingId = updateVirtualKeyDto.BuildingId;
            virtualKey.IntercomId = updateVirtualKeyDto.IntercomId;
            virtualKey.TenantId = updateVirtualKeyDto.TenantId;
            virtualKey.PinCode = updateVirtualKeyDto.PinCode;
            virtualKey.QrCode = updateVirtualKeyDto.QrCode;
            virtualKey.IsActive = updateVirtualKeyDto.IsActive;
            virtualKey.UpdatedAt = DateTime.UtcNow;

            _context.VirtualKeys.Update(virtualKey);
            await _context.SaveChangesAsync();

            // Load navigation properties for the response
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Building)
                .LoadAsync();
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Tenant)
                .LoadAsync();
            await _context.Entry(virtualKey)
                .Reference(vk => vk.Intercom)
                .LoadAsync();

            return MapToDto(virtualKey);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteVirtualKeyAsync(int id, int userId)
        {
            var virtualKey = await _context.VirtualKeys.FirstOrDefaultAsync(vk => vk.KeyId == id);
            if (virtualKey == null)
            {
                return false;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(virtualKey, userId))
            {
                return false; // User doesn't have access to delete this virtual key
            }

            _context.VirtualKeys.Remove(virtualKey);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> RecordVirtualKeyUsageAsync(int keyId, string? usageLocation = null, string? usageDetails = null)
        {
            var virtualKey = await _context.VirtualKeys.FirstOrDefaultAsync(vk => vk.KeyId == keyId);
            if (virtualKey == null || !virtualKey.IsActive)
            {
                return false;
            }

            // Check if key has expired
            if (virtualKey.DateExpired.HasValue && virtualKey.DateExpired.Value <= DateTime.UtcNow)
            {
                return false;
            }

            // For now, we're just updating the virtual key's last used timestamp
            // In a more complete implementation, you might want to create a separate usage log table
            virtualKey.UpdatedAt = DateTime.UtcNow;
            _context.VirtualKeys.Update(virtualKey);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateUniquePinCodeAsync()
        {
            string pinCode;
            bool isUnique;

            do
            {
                // Generate a 6-digit PIN code
                using var rng = RandomNumberGenerator.Create();
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int number = Math.Abs(BitConverter.ToInt32(bytes, 0));
                pinCode = (number % 1000000).ToString("D6");

                // Check if this PIN code already exists
                isUnique = !await _context.VirtualKeys.AnyAsync(vk => vk.PinCode == pinCode);
            }
            while (!isUnique);

            return pinCode;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateQrCodeAsync(int keyId, string? additionalData = null)
        {
            // Simple QR code data generation
            // In a real implementation, you might want to use a QR code library to generate actual QR code images
            var qrData = new StringBuilder();
            qrData.Append($"VKEY:{keyId}");
            qrData.Append($":TS:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            
            if (!string.IsNullOrEmpty(additionalData))
            {
                qrData.Append($":DATA:{additionalData}");
            }

            // Add a simple hash for verification
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(qrData.ToString()));
            var hashString = Convert.ToHexString(hash)[..8]; // Take first 8 characters
            qrData.Append($":HASH:{hashString}");

            await Task.CompletedTask; // Async placeholder
            return qrData.ToString();
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
        public async Task<bool> ValidateTenantAccessAsync(int tenantId, int userId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return false;
            }

            return await HasAccessToEntityAsync(tenant, userId);
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateIntercomAccessAsync(int intercomId, int userId)
        {
            var intercom = await _context.Intercoms.FirstOrDefaultAsync(i => i.Id == intercomId);
            if (intercom == null)
            {
                return false;
            }

            return await HasAccessToEntityAsync(intercom, userId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsVirtualKeyExpiredAsync(int keyId)
        {
            var virtualKey = await _context.VirtualKeys.FirstOrDefaultAsync(vk => vk.KeyId == keyId);
            if (virtualKey == null)
            {
                return true;
            }

            return virtualKey.DateExpired.HasValue && virtualKey.DateExpired.Value <= DateTime.UtcNow;
        }

        /// <inheritdoc/>
        public async Task<int> UpdateExpiredVirtualKeysStatusAsync()
        {
            var expiredKeys = await _context.VirtualKeys
                .Where(vk => vk.DateExpired.HasValue && vk.DateExpired.Value <= DateTime.UtcNow && vk.Status != "Expired")
                .ToListAsync();

            foreach (var key in expiredKeys)
            {
                key.Status = "Expired";
                key.IsActive = false;
                key.UpdatedAt = DateTime.UtcNow;
            }

            if (expiredKeys.Any())
            {
                _context.VirtualKeys.UpdateRange(expiredKeys);
                await _context.SaveChangesAsync();
            }

            return expiredKeys.Count;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysExpiringInDaysAsync(int days, int userId)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);
            var virtualKeys = await GetVirtualKeysBasedOnUserAccess(userId, includeInactive: false);
            
            return virtualKeys.Where(vk => 
                vk.IsActive && 
                vk.DateExpired.HasValue && 
                vk.DateExpired.Value > DateTime.UtcNow && 
                vk.DateExpired.Value <= cutoffDate
            ).Select(MapToDto);
        }

        /// <summary>
        /// Gets virtual keys based on user's access control settings.
        /// </summary>
        /// <param name="userId">The ID of the user requesting access.</param>
        /// <param name="includeInactive">Whether to include inactive virtual keys.</param>
        /// <returns>List of virtual keys the user has access to.</returns>
        private async Task<List<VirtualKey>> GetVirtualKeysBasedOnUserAccess(int userId, bool includeInactive)
        {
            var query = _context.VirtualKeys
                .Include(vk => vk.Building)
                .Include(vk => vk.Tenant)
                .Include(vk => vk.Intercom)
                .AsQueryable();

            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);

            // Apply active/inactive filter
            if (!includeInactive)
            {
                query = query.Where(vk => vk.IsActive);
            }

            return await query.OrderByDescending(vk => vk.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Maps a VirtualKey entity to a VirtualKeyDto.
        /// </summary>
        /// <param name="virtualKey">The virtual key entity to map.</param>
        /// <returns>The mapped virtual key DTO.</returns>
        private static VirtualKeyDto MapToDto(VirtualKey virtualKey)
        {
            return new VirtualKeyDto
            {
                KeyId = virtualKey.KeyId,
                Description = virtualKey.Description,
                DateCreated = virtualKey.DateCreated,
                Type = virtualKey.Type,
                Status = virtualKey.Status,
                DateExpired = virtualKey.DateExpired,
                AccessLocation = virtualKey.AccessLocation,
                BuildingId = virtualKey.BuildingId,
                IntercomId = virtualKey.IntercomId,
                TenantId = virtualKey.TenantId,
                PinCode = virtualKey.PinCode,
                QrCode = virtualKey.QrCode,
                IsActive = virtualKey.IsActive,
                CreatedBy = virtualKey.CreatedBy,
                CreatedAt = virtualKey.CreatedAt,
                UpdatedAt = virtualKey.UpdatedAt,
                BuildingName = virtualKey.Building?.Name,
                IntercomName = virtualKey.Intercom?.Name,
                TenantName = virtualKey.Tenant?.FirstName + " " + virtualKey.Tenant?.LastName
            };
        }
    }
}
