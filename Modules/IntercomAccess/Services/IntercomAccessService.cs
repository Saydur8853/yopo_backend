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

        private async Task<bool> HasBuildingAccessAsync(int buildingId)
        {
            if (IsSuperAdmin()) return true;
            var userId = GetUserId();
            return await _context.UserBuildingPermissions.AsNoTracking().AnyAsync(p => p.UserId == userId && p.BuildingId == buildingId);
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

            // Get intercom to determine building for building-wide codes
            var intercom = await _context.Intercoms.AsNoTracking().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (intercom == null)
            {
                return new VerifyPinResponseDTO { Granted = false, Reason = "Intercom not found", CredentialType = "None", CredentialRefId = null, Timestamp = now };
            }

            // Try AccessCodes first (intercom-specific or building-wide), active and not expired
            var codeCandidates = await _context.Set<IntercomAccessCode>()
                .Where(c => c.IsActive && (c.ExpiresAt == null || c.ExpiresAt > now)
                            && (c.IntercomId == intercomId || (c.IntercomId == null && c.BuildingId == intercom.BuildingId)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            foreach (var c in codeCandidates)
            {
                if (BCrypt.Net.BCrypt.Verify(pin, c.CodeHash))
                {
                    _context.Add(new IntercomAccessLog {
                        IntercomId = intercomId,
                        UserId = c.CreatedBy, // attribute to creator for auditing, if desired
                        CredentialType = "AccessCode",
                        CredentialRefId = c.Id,
                        IsSuccess = true,
                        OccurredAt = now,
                        IpAddress = ip,
                        DeviceInfo = deviceInfo
                    });
                    await _context.SaveChangesAsync();
                    return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "AccessCode", CredentialRefId = c.Id, Timestamp = now };
                }
            }

            // Try Temporary PINs (active, not expired, not max uses reached)
            var tempCandidates = await _context.Set<IntercomTemporaryPin>()
                .Where(t => t.IntercomId == intercomId && t.IsActive && t.ExpiresAt > now && t.UsesCount < t.MaxUses)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            foreach (var t in tempCandidates)
            {
                if (BCrypt.Net.BCrypt.Verify(pin, t.PinHash))
                {
                    // Increment usage
                    t.UsesCount++;
                    if (t.FirstUsedAt == null) t.FirstUsedAt = now;
                    t.LastUsedAt = now;
                    if (t.UsesCount >= t.MaxUses) t.IsActive = false;
                    await _context.SaveChangesAsync();

                    _context.Add(new IntercomAccessLog {
                        IntercomId = intercomId,
                        UserId = t.CreatedByUserId,
                        CredentialType = "Temporary",
                        CredentialRefId = t.Id,
                        IsSuccess = true,
                        OccurredAt = now,
                        IpAddress = ip,
                        DeviceInfo = deviceInfo
                    });
                    await _context.SaveChangesAsync();
                    return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Temporary", CredentialRefId = t.Id, Timestamp = now };
                }
            }

            // Optional: Master pin fallback (if you want to keep admin override)
            var master = await _context.Set<IntercomMasterPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.IsActive);
            if (master != null && BCrypt.Net.BCrypt.Verify(pin, master.PinHash))
            {
                _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "Master", CredentialRefId = master.Id, IsSuccess = true, OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
                await _context.SaveChangesAsync();
                return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Master", CredentialRefId = master.Id, Timestamp = now };
            }

            // Log failed attempt
            _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "None", CredentialRefId = null, IsSuccess = false, Reason = "Invalid or expired", OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
            await _context.SaveChangesAsync();
            return new VerifyPinResponseDTO { Granted = false, Reason = "Invalid or expired", CredentialType = "None", CredentialRefId = null, Timestamp = now };
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

        // Access codes (QR or PIN)
        public async Task<List<YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO>> GetAccessCodesAsync(int? buildingId, int? intercomId)
        {
            if (!IsSuperAdmin())
            {
                // Limit to buildings current user has access to
                var userId = GetUserId();
                var allowedBuildingIds = await _context.UserBuildingPermissions
                    .AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Select(p => p.BuildingId)
                    .ToListAsync();

                var qLimited = _context.Set<IntercomAccessCode>()
                    .AsNoTracking()
                    .Where(c => allowedBuildingIds.Contains(c.BuildingId));

                if (buildingId.HasValue) qLimited = qLimited.Where(c => c.BuildingId == buildingId.Value);
                if (intercomId.HasValue) qLimited = qLimited.Where(c => c.IntercomId == intercomId.Value);

                return await qLimited.OrderByDescending(c => c.CreatedAt)
                    .Select(c => new AccessCodeDTO
                    {
                        Id = c.Id,
                        BuildingId = c.BuildingId,
                        IntercomId = c.IntercomId,
                        Type = c.CodeType,
                        ExpiresAt = c.ExpiresAt,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
            else
            {
                var q = _context.Set<IntercomAccessCode>().AsNoTracking().AsQueryable();
                if (buildingId.HasValue) q = q.Where(c => c.BuildingId == buildingId.Value);
                if (intercomId.HasValue) q = q.Where(c => c.IntercomId == intercomId.Value);

                return await q.OrderByDescending(c => c.CreatedAt)
                    .Select(c => new AccessCodeDTO
                    {
                        Id = c.Id,
                        BuildingId = c.BuildingId,
                        IntercomId = c.IntercomId,
                        Type = c.CodeType,
                        ExpiresAt = c.ExpiresAt,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
        }

        public async Task<(bool Success, string Message, YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO? Code)> CreateAccessCodeAsync(YopoBackend.Modules.IntercomAccess.DTOs.CreateAccessCodeDTO dto, int currentUserId)
        {
            var type = (dto.Type ?? string.Empty).Trim().ToUpperInvariant();
            if (type != "QR" && type != "PIN")
                return (false, "Type must be 'QR' or 'PIN'.", null);

            // Validate building exists
            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId);
            if (building == null) return (false, $"Building {dto.BuildingId} not found.", null);

            // Authz
            if (!await HasBuildingAccessAsync(dto.BuildingId))
                return (false, "Not allowed for this building.", null);

            int? intercomIdToUse = dto.IntercomId;
            if (dto.IntercomId.HasValue)
            {
                var intercom = await _context.Intercoms.AsNoTracking().FirstOrDefaultAsync(i => i.IntercomId == dto.IntercomId.Value);
                if (intercom == null) return (false, $"Intercom {dto.IntercomId.Value} not found.", null);
                if (intercom.BuildingId != dto.BuildingId) return (false, "Intercom does not belong to the specified building.", null);
            }

            // ExpiresAt can be null for infinity; if provided, must be in the future
            if (dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= DateTime.UtcNow)
                return (false, "Expiry must be in the future.", null);

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Code);
            var entity = new IntercomAccessCode
            {
                BuildingId = dto.BuildingId,
                IntercomId = intercomIdToUse,
                CodeType = type,
                CodeHash = hash,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();

            var result = new AccessCodeDTO
            {
                Id = entity.Id,
                BuildingId = entity.BuildingId,
                IntercomId = entity.IntercomId,
                Type = entity.CodeType,
                ExpiresAt = entity.ExpiresAt,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
            return (true, "Access code created.", result);
        }

        public async Task<(bool Success, string Message)> DeactivateAccessCodeAsync(int id, int currentUserId)
        {
            var entity = await _context.Set<IntercomAccessCode>().FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return (false, "Not found.");
            if (!await HasBuildingAccessAsync(entity.BuildingId)) return (false, "Not allowed.");
            if (!entity.IsActive) return (true, "Already inactive.");
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            return (true, "Deactivated.");
        }

        public async Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO> items, int total)> GetAccessLogsGlobalAsync(
            int? buildingId, int? intercomId, int? codeId, int page, int pageSize, DateTime? from, DateTime? to, bool? success, string? credentialType, int? userId)
        {
            if (!IsSuperAdmin()) return (new List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO>(), 0);

            var q = _context.IntercomAccessLogs.AsNoTracking().AsQueryable();

            if (buildingId.HasValue)
            {
                var intercomIds = await _context.Intercoms.AsNoTracking()
                    .Where(i => i.BuildingId == buildingId.Value)
                    .Select(i => i.IntercomId)
                    .ToListAsync();
                q = q.Where(l => intercomIds.Contains(l.IntercomId));
            }
            if (intercomId.HasValue) q = q.Where(l => l.IntercomId == intercomId.Value);
            if (codeId.HasValue) q = q.Where(l => l.CredentialType == "AccessCode" && l.CredentialRefId == codeId.Value);
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
    }
}
