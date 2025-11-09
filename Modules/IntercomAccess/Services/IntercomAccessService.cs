using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Auth;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Models;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public class IntercomAccessService : IIntercomAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IntercomAccessService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(userId);
        }

        private string GetUserRole() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        private bool IsSuperAdmin() => string.Equals(GetUserRole(), Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

        private async Task<bool> CanViewIntercomAsync(int intercomId)
        {
            if (IsSuperAdmin()) return true;
            var userId = GetUserId();
            var intercom = await _context.Intercoms.AsNoTracking().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (intercom == null) return false;
            return await _context.UserBuildingPermissions.AnyAsync(p => p.UserId == userId && p.BuildingId == intercom.BuildingId);
        }

        public async Task<PinOperationResponseDTO> SetOrUpdateMasterPinAsync(int intercomId, string pin, int currentUserId)
        {
            if (!IsSuperAdmin()) return new PinOperationResponseDTO { Success = false, Message = "Only Super Admin can manage master pin." };

            var intercomExists = await _context.Intercoms.AnyAsync(i => i.IntercomId == intercomId);
            if (!intercomExists) return new PinOperationResponseDTO { Success = false, Message = $"Intercom {intercomId} not found." };

            var existing = await _context.Set<IntercomMasterPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.IsActive);
            var hash = BCrypt.Net.BCrypt.HashPassword(pin);
            if (existing == null)
            {
                var entity = new IntercomMasterPin { IntercomId = intercomId, PinHash = hash, IsActive = true, CreatedBy = currentUserId, CreatedAt = DateTime.UtcNow };
                _context.Add(entity);
            }
            else
            {
                existing.PinHash = hash;
                existing.UpdatedBy = currentUserId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            // Audit: record edit event
            _context.IntercomAccessLogs.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = currentUserId, CredentialType = "Master", CredentialRefId = existing?.Id, IsSuccess = true, Reason = "Master pin set/updated", OccurredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return new PinOperationResponseDTO { Success = true, Message = "Master pin set/updated." };
        }

        public async Task<PinOperationResponseDTO> SetOrUpdateUserPinAsync(int intercomId, int userId, string pin, int currentUserId, string? masterPin = null)
        {
            var isSuper = IsSuperAdmin();
            if (!isSuper && currentUserId != userId)
                return new PinOperationResponseDTO { Success = false, Message = "Not allowed." };

            // If acting on another user's pin, require master pin authentication
            if (isSuper && currentUserId != userId)
            {
                if (string.IsNullOrWhiteSpace(masterPin))
                    return new PinOperationResponseDTO { Success = false, Message = "Master pin required to reset another user's pin." };
                var master = await _context.Set<IntercomMasterPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.IsActive);
                if (master == null || !BCrypt.Net.BCrypt.Verify(masterPin, master.PinHash))
                    return new PinOperationResponseDTO { Success = false, Message = "Invalid master pin." };
            }

            var intercomExists = await _context.Intercoms.AnyAsync(i => i.IntercomId == intercomId);
            if (!intercomExists) return new PinOperationResponseDTO { Success = false, Message = $"Intercom {intercomId} not found." };
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive);
            if (!userExists) return new PinOperationResponseDTO { Success = false, Message = $"User {userId} not found or inactive." };

            var existing = await _context.Set<IntercomUserPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.UserId == userId && x.IsActive);
            var hash = BCrypt.Net.BCrypt.HashPassword(pin);
            if (existing == null)
            {
                _context.Add(new IntercomUserPin { IntercomId = intercomId, UserId = userId, PinHash = hash, IsActive = true, CreatedBy = currentUserId, CreatedAt = DateTime.UtcNow });
            }
            else
            {
                existing.PinHash = hash;
                existing.UpdatedBy = currentUserId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            _context.IntercomAccessLogs.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = currentUserId, CredentialType = "User", CredentialRefId = existing?.Id, IsSuccess = true, Reason = currentUserId == userId ? "Own pin set/updated" : "User pin set/updated by admin", OccurredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return new PinOperationResponseDTO { Success = true, Message = "User pin set/updated." };
        }

        public async Task<PinOperationResponseDTO> UpdateOwnUserPinAsync(int intercomId, int currentUserId, string newPin, string? oldPin)
        {
            var existing = await _context.Set<IntercomUserPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.UserId == currentUserId && x.IsActive);
            if (existing == null)
            {
                // create if none exists
                var hashNew = BCrypt.Net.BCrypt.HashPassword(newPin);
                _context.Add(new IntercomUserPin { IntercomId = intercomId, UserId = currentUserId, PinHash = hashNew, IsActive = true, CreatedBy = currentUserId, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            _context.IntercomAccessLogs.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = currentUserId, CredentialType = "User", CredentialRefId = null, IsSuccess = true, Reason = "Own pin created", OccurredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return new PinOperationResponseDTO { Success = true, Message = "Pin created." };
            }
            // if exists and oldPin provided, verify
            if (!string.IsNullOrEmpty(oldPin) && !BCrypt.Net.BCrypt.Verify(oldPin, existing.PinHash))
            {
                return new PinOperationResponseDTO { Success = false, Message = "Old pin does not match." };
            }
            existing.PinHash = BCrypt.Net.BCrypt.HashPassword(newPin);
            existing.UpdatedBy = currentUserId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _context.IntercomAccessLogs.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = currentUserId, CredentialType = "User", CredentialRefId = existing.Id, IsSuccess = true, Reason = "Own pin updated", OccurredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return new PinOperationResponseDTO { Success = true, Message = "Pin updated." };
        }

        public async Task<PinOperationResponseDTO> CreateTemporaryPinAsync(int intercomId, int currentUserId, string pin, DateTime expiresAt, int maxUses)
        {
            // Tenant-only can create temp pins for now
            var role = GetUserRole();
            if (!string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
                return new PinOperationResponseDTO { Success = false, Message = "Only Tenants can create temporary pins." };

            if (expiresAt <= DateTime.UtcNow)
                return new PinOperationResponseDTO { Success = false, Message = "Expiry must be in the future." };
            if (maxUses < 1) maxUses = 1;

            var intercomExists = await _context.Intercoms.AnyAsync(i => i.IntercomId == intercomId);
            if (!intercomExists) return new PinOperationResponseDTO { Success = false, Message = $"Intercom {intercomId} not found." };

            var entity = new IntercomTemporaryPin
            {
                IntercomId = intercomId,
                CreatedByUserId = currentUserId,
                PinHash = BCrypt.Net.BCrypt.HashPassword(pin),
                ExpiresAt = expiresAt,
                MaxUses = maxUses,
                UsesCount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            _context.IntercomAccessLogs.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = currentUserId, CredentialType = "Temporary", CredentialRefId = entity.Id, IsSuccess = true, Reason = "Temporary pin created", OccurredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return new PinOperationResponseDTO { Success = true, Message = "Temporary pin created." };
        }

        public async Task<VerifyPinResponseDTO> VerifyPinAsync(int intercomId, string pin, string? ip, string? deviceInfo)
        {
            var now = DateTime.UtcNow;
            // Try Temporary pins valid and active with time and uses left
            var tempCandidates = await _context.Set<IntercomTemporaryPin>()
                .Where(t => t.IntercomId == intercomId && t.IsActive && t.ExpiresAt > now && t.UsesCount < t.MaxUses)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
            foreach (var t in tempCandidates)
            {
                if (BCrypt.Net.BCrypt.Verify(pin, t.PinHash))
                {
                    t.UsesCount += 1;
                    t.LastUsedAt = now;
                    if (!t.FirstUsedAt.HasValue) t.FirstUsedAt = now;
                    if (t.UsesCount >= t.MaxUses) t.IsActive = false;
                    _context.Add(new IntercomTemporaryPinUsage { TemporaryPinId = t.Id, UsedAt = now, UsedFromIp = ip, DeviceInfo = deviceInfo });
                    _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = t.CreatedByUserId, CredentialType = "Temporary", CredentialRefId = t.Id, IsSuccess = true, OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
                    await _context.SaveChangesAsync();
                    return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Temporary", CredentialRefId = t.Id, Timestamp = now };
                }
            }

            // Try User pins
            var userPins = await _context.Set<IntercomUserPin>().Where(x => x.IntercomId == intercomId && x.IsActive).ToListAsync();
            foreach (var up in userPins)
            {
                if (BCrypt.Net.BCrypt.Verify(pin, up.PinHash))
                {
                    _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = up.UserId, CredentialType = "User", CredentialRefId = up.Id, IsSuccess = true, OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
                    await _context.SaveChangesAsync();
                    return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "User", CredentialRefId = up.Id, Timestamp = now };
                }
            }

            // Try Master pin
            var master = await _context.Set<IntercomMasterPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.IsActive);
            if (master != null && BCrypt.Net.BCrypt.Verify(pin, master.PinHash))
            {
                _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "Master", CredentialRefId = master.Id, IsSuccess = true, OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
                await _context.SaveChangesAsync();
                return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Master", CredentialRefId = master.Id, Timestamp = now };
            }

            // Log failed attempt
            _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "None", CredentialRefId = null, IsSuccess = false, Reason = "Invalid PIN or expired", OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
            await _context.SaveChangesAsync();
            return new VerifyPinResponseDTO { Granted = false, Reason = "Invalid PIN or expired", CredentialType = "None", CredentialRefId = null, Timestamp = now };
        }

        public async Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO> items, int total)> GetAccessLogsAsync(
            int intercomId, int page, int pageSize, DateTime? from, DateTime? to, bool? success, string? credentialType, int? userId)
        {
            // Super Admin only
            if (!IsSuperAdmin()) return (new List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO>(), 0);

            var q = _context.IntercomAccessLogs.AsNoTracking().Where(l => l.IntercomId == intercomId);
            if (from.HasValue) q = q.Where(l => l.OccurredAt >= from.Value);
            if (to.HasValue) q = q.Where(l => l.OccurredAt <= to.Value);
            if (success.HasValue) q = q.Where(l => l.IsSuccess == success.Value);
            if (!string.IsNullOrWhiteSpace(credentialType)) q = q.Where(l => l.CredentialType == credentialType);
            if (userId.HasValue) q = q.Where(l => l.UserId == userId.Value);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(l => l.OccurredAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .Select(l => new YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO
                               {
                                   Id = l.Id,
                                   IntercomId = l.IntercomId,
                                   UserId = l.UserId,
                                   CredentialType = l.CredentialType,
                                   CredentialRefId = l.CredentialRefId,
                                   IsSuccess = l.IsSuccess,
                                   Reason = l.Reason,
                                   OccurredAt = l.OccurredAt,
                                   IpAddress = l.IpAddress,
                                   DeviceInfo = l.DeviceInfo
                               })
                               .ToListAsync();
            return (items, total);
        }

        public async Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.TemporaryPinUsageDTO> items, int total)> GetTemporaryUsagesAsync(
            int intercomId, int page, int pageSize, DateTime? from, DateTime? to, int? temporaryPinId)
        {
            // Super Admin only
            if (!IsSuperAdmin()) return (new List<YopoBackend.Modules.IntercomAccess.DTOs.TemporaryPinUsageDTO>(), 0);

            // Filter usages by intercom via join on TemporaryPins
            var q = _context.IntercomTemporaryPinUsages.AsNoTracking()
                .Join(_context.IntercomTemporaryPins, u => u.TemporaryPinId, t => t.Id, (u, t) => new { u, t })
                .Where(x => x.t.IntercomId == intercomId)
                .Select(x => x.u);

            if (from.HasValue) q = q.Where(u => u.UsedAt >= from.Value);
            if (to.HasValue) q = q.Where(u => u.UsedAt <= to.Value);
            if (temporaryPinId.HasValue) q = q.Where(u => u.TemporaryPinId == temporaryPinId.Value);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(u => u.UsedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .Select(u => new YopoBackend.Modules.IntercomAccess.DTOs.TemporaryPinUsageDTO
                               {
                                   Id = u.Id,
                                   TemporaryPinId = u.TemporaryPinId,
                                   UsedAt = u.UsedAt,
                                   UsedFromIp = u.UsedFromIp,
                                   DeviceInfo = u.DeviceInfo
                               })
                               .ToListAsync();
            return (items, total);
        }
    }
}
