using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using YopoBackend.Auth;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Models;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Utils;

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

        private async Task<int?> ResolveTenantBuildingIdAsync(int tenantUserId)
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantUserId);
            if (unit != null) return unit.BuildingId;

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantUserId);
            if (tenant != null) return tenant.BuildingId;

            return null;
        }

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

            // 1) Explicit per-user building permission
            var hasExplicit = await _context.UserBuildingPermissions
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.BuildingId == buildingId);
            if (hasExplicit) return true;

            // 2) Owner/PM of the building implicitly has access
            var building = await _context.Buildings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);
            if (building == null) return false;
            if (building.CustomerId == userId || building.CreatedBy == userId) return true;

            return false;
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

            // PIN exists: require oldPin and verify it
            if (string.IsNullOrEmpty(oldPin))
            {
                return new PinOperationResponseDTO { Success = false, Message = "Old pin is required." };
            }
            if (!BCrypt.Net.BCrypt.Verify(oldPin, existing.PinHash))
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

        public async Task<VerifyPinResponseDTO> VerifyPinAsync(int intercomId, VerifyAccessDTO dto, string? ip, string? deviceInfo)
        {
            var now = DateTime.UtcNow;

            // Get intercom to determine building for building-wide codes
            var intercom = await _context.Intercoms.AsNoTracking().FirstOrDefaultAsync(i => i.IntercomId == intercomId);
            if (intercom == null)
            {
                return new VerifyPinResponseDTO { Granted = false, Reason = "Intercom not found", CredentialType = "None", CredentialRefId = null, Timestamp = now };
            }

            var pinProvided = !string.IsNullOrWhiteSpace(dto.Pin);
            if (pinProvided)
            {
                // Try AccessCodes first (intercom-specific or building-wide), active and not expired
                var codeCandidates = await _context.Set<IntercomAccessCode>()
                    .Where(c => c.IsActive && (c.ExpiresAt == null || c.ExpiresAt > now)
                                && (c.ValidFrom == null || c.ValidFrom <= now)
                                && (c.IntercomId == intercomId || (c.IntercomId == null && c.BuildingId == intercom.BuildingId)))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                foreach (var c in codeCandidates)
                {
                    if (BCrypt.Net.BCrypt.Verify(dto.Pin!, c.CodeHash))
                    {
                        if (c.IsSingleUse)
                        {
                            c.IsActive = false;
                        }

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
            }

            if (pinProvided)
            {
                // Optional: Master pin fallback (if you want to keep admin override)
                var master = await _context.Set<IntercomMasterPin>().FirstOrDefaultAsync(x => x.IntercomId == intercomId && x.IsActive);
                if (master != null && BCrypt.Net.BCrypt.Verify(dto.Pin!, master.PinHash))
                {
                    _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "Master", CredentialRefId = master.Id, IsSuccess = true, OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
                    await _context.SaveChangesAsync();
                    return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Master", CredentialRefId = master.Id, Timestamp = now };
                }
            }

            if (dto.Face != null)
            {
                var faceResult = await TryVerifyFaceAsync(intercom, dto.Face, ip, deviceInfo);
                if (faceResult != null)
                {
                    return faceResult;
                }
            }

            // Log failed attempt
            _context.Add(new IntercomAccessLog { IntercomId = intercomId, UserId = null, CredentialType = "None", CredentialRefId = null, IsSuccess = false, Reason = "Invalid or expired", OccurredAt = now, IpAddress = ip, DeviceInfo = deviceInfo });
            await _context.SaveChangesAsync();
            return new VerifyPinResponseDTO { Granted = false, Reason = "Invalid or expired", CredentialType = "None", CredentialRefId = null, Timestamp = now };
        }

        private async Task<VerifyPinResponseDTO?> TryVerifyFaceAsync(Intercom intercom, FaceBiometricPayloadDTO face, string? ip, string? deviceInfo)
        {
            var now = DateTime.UtcNow;

            if (!TryValidateFacePayload(face, out var validationError, out var front, out var left, out var right))
            {
                return new VerifyPinResponseDTO { Granted = false, Reason = validationError ?? "Invalid face payload", CredentialType = "Face", CredentialRefId = null, Timestamp = now };
            }

            var match = await _context.Set<IntercomFaceBiometric>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.IsActive
                    && f.FrontImageHash == front.Hash
                    && f.LeftImageHash == left.Hash
                    && f.RightImageHash == right.Hash);

            if (match == null)
            {
                return new VerifyPinResponseDTO { Granted = false, Reason = "Face not recognized", CredentialType = "Face", CredentialRefId = null, Timestamp = now };
            }

            if (!await UserHasBuildingAccessAsync(match.UserId, intercom.BuildingId))
            {
                return new VerifyPinResponseDTO { Granted = false, Reason = "User has no access to this building", CredentialType = "Face", CredentialRefId = match.Id, Timestamp = now };
            }

            _context.Add(new IntercomAccessLog
            {
                IntercomId = intercom.IntercomId,
                UserId = match.UserId,
                CredentialType = "Face",
                CredentialRefId = match.Id,
                IsSuccess = true,
                OccurredAt = now,
                IpAddress = ip,
                DeviceInfo = deviceInfo
            });
            await _context.SaveChangesAsync();

            return new VerifyPinResponseDTO { Granted = true, Reason = "OK", CredentialType = "Face", CredentialRefId = match.Id, Timestamp = now };
        }

        private static bool TryValidateFacePayload(
            FaceBiometricPayloadDTO face,
            out string? error,
            out (byte[] Bytes, string MimeType, string Hash) front,
            out (byte[] Bytes, string MimeType, string Hash) left,
            out (byte[] Bytes, string MimeType, string Hash) right)
        {
            error = null;
            front = default;
            left = default;
            right = default;

            if (string.IsNullOrWhiteSpace(face.FrontImageBase64) ||
                string.IsNullOrWhiteSpace(face.LeftImageBase64) ||
                string.IsNullOrWhiteSpace(face.RightImageBase64))
            {
                error = "All three face images are required.";
                return false;
            }

            var frontValidation = ImageUtils.ValidateBase64Image(face.FrontImageBase64);
            if (!frontValidation.IsValid || frontValidation.ImageBytes == null || string.IsNullOrWhiteSpace(frontValidation.MimeType))
            {
                error = $"Invalid front image: {frontValidation.ErrorMessage ?? "Unsupported image format."}";
                return false;
            }

            var leftValidation = ImageUtils.ValidateBase64Image(face.LeftImageBase64);
            if (!leftValidation.IsValid || leftValidation.ImageBytes == null || string.IsNullOrWhiteSpace(leftValidation.MimeType))
            {
                error = $"Invalid left image: {leftValidation.ErrorMessage ?? "Unsupported image format."}";
                return false;
            }

            var rightValidation = ImageUtils.ValidateBase64Image(face.RightImageBase64);
            if (!rightValidation.IsValid || rightValidation.ImageBytes == null || string.IsNullOrWhiteSpace(rightValidation.MimeType))
            {
                error = $"Invalid right image: {rightValidation.ErrorMessage ?? "Unsupported image format."}";
                return false;
            }

            front = (frontValidation.ImageBytes, frontValidation.MimeType, ComputeHash(frontValidation.ImageBytes));
            left = (leftValidation.ImageBytes, leftValidation.MimeType, ComputeHash(leftValidation.ImageBytes));
            right = (rightValidation.ImageBytes, rightValidation.MimeType, ComputeHash(rightValidation.ImageBytes));
            return true;
        }

        private static string ComputeHash(byte[] bytes)
        {
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private async Task<bool> UserHasBuildingAccessAsync(int userId, int buildingId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
            {
                return false;
            }

            if (user.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return true;
            }

            if (user.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                var tenantBuildingId = await ResolveTenantBuildingIdAsync(userId);
                return tenantBuildingId.HasValue && tenantBuildingId.Value == buildingId;
            }

            var hasExplicit = await _context.UserBuildingPermissions
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.BuildingId == buildingId && p.IsActive);
            if (hasExplicit)
            {
                return true;
            }

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == buildingId);
            if (building == null)
            {
                return false;
            }

            return building.CustomerId == userId || building.CreatedBy == userId;
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

        // Access codes (QR or PIN)
        public async Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO> items, int total)> GetAccessCodesAsync(int? buildingId, int? intercomId, int page, int pageSize)
        {
            IQueryable<IntercomAccessCode> q;

            if (IsSuperAdmin())
            {
                // SuperAdmin: full visibility
                q = _context.Set<IntercomAccessCode>().AsNoTracking();
            }
            else
            {
                var userId = GetUserId();
                var role = GetUserRole();

                if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
                {
                    // Tenants: can only see their own codes, regardless of building permissions
                    q = _context.Set<IntercomAccessCode>()
                        .AsNoTracking()
                        .Where(c => c.CreatedBy == userId);
                }
                else
                {
                    // PM / FrontDesk / other roles: building-level visibility based on permissions/ownership
                    var explicitBuildingIds = await _context.UserBuildingPermissions
                        .AsNoTracking()
                        .Where(p => p.UserId == userId)
                        .Select(p => p.BuildingId)
                        .ToListAsync();

                    var ownedBuildingIds = await _context.Buildings
                        .AsNoTracking()
                        .Where(b => b.CustomerId == userId || b.CreatedBy == userId)
                        .Select(b => b.BuildingId)
                        .ToListAsync();

                    var allowedBuildingIds = explicitBuildingIds.Union(ownedBuildingIds).ToList();

                    q = _context.Set<IntercomAccessCode>()
                        .AsNoTracking()
                        .Where(c => allowedBuildingIds.Contains(c.BuildingId));
                }
            }

            if (buildingId.HasValue) q = q.Where(c => c.BuildingId == buildingId.Value);
            if (intercomId.HasValue) q = q.Where(c => c.IntercomId == intercomId.Value);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new AccessCodeDTO
                {
                    Id = c.Id,
                    BuildingId = c.BuildingId,
                    IntercomId = c.IntercomId,
                    TenantId = c.TenantId,
                    Code = c.CodePlain ?? c.CodeHash, // fallback to hash for legacy records
                    IsSingleUse = c.IsSingleUse,
                    ValidFrom = c.ValidFrom,
                    ExpiresAt = c.ExpiresAt,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<(bool Success, string Message, YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO? Code)> CreateAccessCodeAsync(YopoBackend.Modules.IntercomAccess.DTOs.CreateAccessCodeDTO dto, int currentUserId)
        {
            // Currently only PIN-type access codes are supported.
            var type = "PIN";

            var role = GetUserRole();
            int buildingIdToUse;

            if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                var tenantBuildingId = await ResolveTenantBuildingIdAsync(currentUserId);
                if (!tenantBuildingId.HasValue)
                    return (false, "Tenant building not found.", null);

                if (dto.BuildingId.HasValue && dto.BuildingId.Value != tenantBuildingId.Value)
                    return (false, "Not allowed for this building.", null);

                buildingIdToUse = tenantBuildingId.Value;
            }
            else
            {
                if (!dto.BuildingId.HasValue)
                    return (false, "BuildingId is required.", null);

                buildingIdToUse = dto.BuildingId.Value;
            }

            // Validate building exists
            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == buildingIdToUse);
            if (building == null) return (false, $"Building {buildingIdToUse} not found.", null);

            // Authz
            if (!string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                if (!await HasBuildingAccessAsync(buildingIdToUse))
                    return (false, "Not allowed for this building.", null);
            }

            int? intercomIdToUse = dto.IntercomId;
            if (dto.IntercomId.HasValue)
            {
                var intercom = await _context.Intercoms.AsNoTracking().FirstOrDefaultAsync(i => i.IntercomId == dto.IntercomId.Value);
                if (intercom == null) return (false, $"Intercom {dto.IntercomId.Value} not found.", null);
                if (intercom.BuildingId != buildingIdToUse) return (false, "Intercom does not belong to the specified building.", null);
            }

            // ExpiresAt can be null for infinity; if provided, must be in the future
            if (dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= DateTime.UtcNow)
                return (false, "Expiry must be in the future.", null);

            if (dto.ValidFrom.HasValue && dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= dto.ValidFrom.Value)
                return (false, "Expiry must be after valid-from.", null);

            int? tenantIdToUse = null;
            if (dto.TenantId.HasValue)
            {
                var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == dto.TenantId.Value);
                if (tenant == null)
                    return (false, $"Tenant {dto.TenantId.Value} not found.", null);
                if (tenant.BuildingId != buildingIdToUse)
                    return (false, "Tenant does not belong to the specified building.", null);

                tenantIdToUse = tenant.TenantId;
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Code);
            var entity = new IntercomAccessCode
            {
                BuildingId = buildingIdToUse,
                IntercomId = intercomIdToUse,
                TenantId = tenantIdToUse,
                CodeType = type,
                CodeHash = hash,
                CodePlain = dto.Code, // store raw PIN so it can be shown via API
                IsSingleUse = dto.IsSingleUse,
                ValidFrom = dto.ValidFrom,
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
                TenantId = entity.TenantId,
                Code = entity.CodePlain ?? entity.CodeHash, // should be plain for new records, hash fallback otherwise
                IsSingleUse = entity.IsSingleUse,
                ValidFrom = entity.ValidFrom,
                ExpiresAt = entity.ExpiresAt,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
            return (true, "Access code created.", result);
        }

        public async Task<(bool Success, string Message, YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO? Code)> UpdateAccessCodeAsync(int id, YopoBackend.Modules.IntercomAccess.DTOs.UpdateAccessCodeDTO dto, int currentUserId)
        {
            var entity = await _context.Set<IntercomAccessCode>().FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return (false, "Not found.", null);

            var role = GetUserRole();
            if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                // Tenants may only update codes they created themselves
                if (entity.CreatedBy != currentUserId)
                    return (false, "Not allowed.", null);
            }
            else if (!IsSuperAdmin())
            {
                // PM/FD/other roles require building access; SuperAdmin can update any
                if (!await HasBuildingAccessAsync(entity.BuildingId)) return (false, "Not allowed.", null);
            }

            // ExpiresAt can be null for infinity; if provided, must be in the future
            if (dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= DateTime.UtcNow)
                return (false, "Expiry must be in the future.", null);

            var effectiveValidFrom = dto.ValidFrom ?? entity.ValidFrom;
            var effectiveExpiresAt = dto.ExpiresAt.HasValue ? dto.ExpiresAt : entity.ExpiresAt;
            if (effectiveValidFrom.HasValue && effectiveExpiresAt.HasValue && effectiveExpiresAt.Value <= effectiveValidFrom.Value)
                return (false, "Expiry must be after valid-from.", null);

            // Update mutable fields only when explicitly provided
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                entity.CodeHash = BCrypt.Net.BCrypt.HashPassword(dto.Code);
                entity.CodePlain = dto.Code;
            }

            if (dto.IsSingleUse.HasValue)
            {
                entity.IsSingleUse = dto.IsSingleUse.Value;
            }

            if (dto.ValidFrom.HasValue)
            {
                entity.ValidFrom = dto.ValidFrom;
            }

            if (dto.ExpiresAt.HasValue)
            {
                entity.ExpiresAt = dto.ExpiresAt;
            }

            await _context.SaveChangesAsync();

            var result = new AccessCodeDTO
            {
                Id = entity.Id,
                BuildingId = entity.BuildingId,
                IntercomId = entity.IntercomId,
                TenantId = entity.TenantId,
                Code = entity.CodePlain ?? entity.CodeHash,
                IsSingleUse = entity.IsSingleUse,
                ValidFrom = entity.ValidFrom,
                ExpiresAt = entity.ExpiresAt,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };

            return (true, "Access code updated.", result);
        }

        public async Task<(bool Success, string Message, AccessCodeDTO? Code)> DeactivateAccessCodeAsync(int id, int currentUserId)
        {
            var entity = await _context.Set<IntercomAccessCode>().FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return (false, "Not found.", null);

            var role = GetUserRole();
            if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                // Tenants may only deactivate codes they created themselves
                if (entity.CreatedBy != currentUserId)
                    return (false, "Not allowed.", null);
            }
            else
            {
                // Other roles (PM/FD/SuperAdmin) require building access
                if (!await HasBuildingAccessAsync(entity.BuildingId)) return (false, "Not allowed.", null);
            }

            // Toggle active status
            entity.IsActive = !entity.IsActive;
            await _context.SaveChangesAsync();

            var result = new AccessCodeDTO
            {
                Id = entity.Id,
                BuildingId = entity.BuildingId,
                IntercomId = entity.IntercomId,
                TenantId = entity.TenantId,
                Code = entity.CodePlain ?? entity.CodeHash,
                IsSingleUse = entity.IsSingleUse,
                ValidFrom = entity.ValidFrom,
                ExpiresAt = entity.ExpiresAt,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };

            var message = entity.IsActive ? "Activated." : "Deactivated.";
            return (true, message, result);
        }

        public async Task<(bool Success, string Message)> DeleteAccessCodeAsync(int id, int currentUserId)
        {
            var entity = await _context.Set<IntercomAccessCode>().FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return (false, "Not found.");

            var role = GetUserRole();
            if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                // Tenants may only delete codes they created themselves
                if (entity.CreatedBy != currentUserId)
                    return (false, "Not allowed.");
            }
            else if (!IsSuperAdmin())
            {
                // PM/FD/other roles require building access; SuperAdmin can delete any
                if (!await HasBuildingAccessAsync(entity.BuildingId)) return (false, "Not allowed.");
            }

            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return (true, "Deleted.");
        }

        public async Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO> items, int total)> GetAccessLogsGlobalAsync(
            int? buildingId, int? intercomId, int? codeId, int page, int pageSize, DateTime? from, DateTime? to, bool? success, string? credentialType, int? userId)
        {
            IQueryable<IntercomAccessLog> q;
            var role = GetUserRole();

            if (!IsSuperAdmin())
            {
                var currentUserId = GetUserId();
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserId)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                var ownedBuildingIds = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.CustomerId == currentUserId || b.CreatedBy == currentUserId)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
                var allowedBuildingIds = explicitBuildingIds.Union(ownedBuildingIds).ToList();
                var allowedIntercomIds = await _context.Intercoms.AsNoTracking()
                    .Where(i => allowedBuildingIds.Contains(i.BuildingId))
                    .Select(i => i.IntercomId)
                    .ToListAsync();

                q = _context.IntercomAccessLogs.AsNoTracking().Where(l => allowedIntercomIds.Contains(l.IntercomId));

                if (string.Equals(role, Roles.Tenant, StringComparison.OrdinalIgnoreCase))
                {
                    // Tenants: restrict to logs attributed to themselves (their own codes)
                    q = q.Where(l => l.UserId == currentUserId);
                }
            }
            else
            {
                q = _context.IntercomAccessLogs.AsNoTracking();
            }

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
