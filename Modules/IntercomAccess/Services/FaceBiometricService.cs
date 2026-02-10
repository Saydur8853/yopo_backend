using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Models;
using YopoBackend.Constants;
using YopoBackend.Services;
using YopoBackend.Utils;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public class FaceBiometricService : IFaceBiometricService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public FaceBiometricService(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<FaceBiometricRecordDTO?> GetAsync(int userId)
        {
            var record = await _context.Set<IntercomFaceBiometric>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            return record == null ? null : MapToDto(record);
        }

        public async Task<(bool Success, string Message, FaceBiometricRecordDTO? Record)> UpsertAsync(int userId, FaceBiometricUploadDTO dto)
        {
            ValidateDevicePlatform(dto.DeviceInfo?.Platform);

            var front = await SaveImageAsync(dto.FrontImageBase64!, "front");
            var left = await SaveImageAsync(dto.LeftImageBase64!, "left");
            var right = await SaveImageAsync(dto.RightImageBase64!, "right");

            var record = await _context.Set<IntercomFaceBiometric>()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (record == null)
            {
                record = new IntercomFaceBiometric
                {
                    UserId = userId,
                    FrontImageUrl = front.Url,
                    FrontImagePublicId = front.PublicId,
                    LeftImageUrl = left.Url,
                    LeftImagePublicId = left.PublicId,
                    RightImageUrl = right.Url,
                    RightImagePublicId = right.PublicId,
                    FrontImageHash = front.Hash,
                    LeftImageHash = left.Hash,
                    RightImageHash = right.Hash,
                    FrontImageMimeType = front.MimeType,
                    LeftImageMimeType = left.MimeType,
                    RightImageMimeType = right.MimeType,
                    DevicePlatform = dto.DeviceInfo?.Platform?.Trim(),
                    DeviceModel = dto.DeviceInfo?.Model?.Trim(),
                    AppVersion = dto.DeviceInfo?.AppVersion?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Add(record);
            }
            else
            {
                await DeleteCloudinaryImageAsync(record.FrontImagePublicId);
                await DeleteCloudinaryImageAsync(record.LeftImagePublicId);
                await DeleteCloudinaryImageAsync(record.RightImagePublicId);

                record.FrontImageUrl = front.Url;
                record.FrontImagePublicId = front.PublicId;
                record.LeftImageUrl = left.Url;
                record.LeftImagePublicId = left.PublicId;
                record.RightImageUrl = right.Url;
                record.RightImagePublicId = right.PublicId;
                record.FrontImageHash = front.Hash;
                record.LeftImageHash = left.Hash;
                record.RightImageHash = right.Hash;
                record.FrontImageMimeType = front.MimeType;
                record.LeftImageMimeType = left.MimeType;
                record.RightImageMimeType = right.MimeType;
                record.DevicePlatform = dto.DeviceInfo?.Platform?.Trim();
                record.DeviceModel = dto.DeviceInfo?.Model?.Trim();
                record.AppVersion = dto.DeviceInfo?.AppVersion?.Trim();
                record.IsActive = true;
                record.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await UpsertTempIntercomAsync(userId, front.Url, left.Url, right.Url);

            return (true, "Uploaded successfully", MapToDto(record));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int userId)
        {
            var record = await _context.Set<IntercomFaceBiometric>()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (record == null)
            {
                return (false, "Not found.");
            }

            await DeleteCloudinaryImageAsync(record.FrontImagePublicId);
            await DeleteCloudinaryImageAsync(record.LeftImagePublicId);
            await DeleteCloudinaryImageAsync(record.RightImagePublicId);

            _context.Remove(record);
            await _context.SaveChangesAsync();
            return (true, "Deleted.");
        }

        private async Task<(string Url, string PublicId, string MimeType, string Hash)> SaveImageAsync(string base64, string label)
        {
            var validation = ImageUtils.ValidateBase64Image(base64);
            if (!validation.IsValid || validation.ImageBytes == null || string.IsNullOrWhiteSpace(validation.MimeType))
            {
                throw new ArgumentException($"Invalid image: {validation.ErrorMessage ?? "Unsupported image format."}");
            }

            var upload = await _cloudinaryService.UploadIdentityDocumentAsync(validation.ImageBytes, validation.MimeType);

            return (upload.Url, upload.PublicId, validation.MimeType, ComputeHash(validation.ImageBytes));
        }

        private async Task DeleteCloudinaryImageAsync(string? publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            await _cloudinaryService.DeleteAssetAsync(publicId, "image");
        }

        private static string ComputeHash(byte[] bytes)
        {
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static void ValidateDevicePlatform(string? platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
            {
                throw new ArgumentException("DeviceInfo.platform is required.");
            }

            var normalized = platform.Trim().ToLowerInvariant();
            if (normalized != "android" && normalized != "ios")
            {
                throw new ArgumentException("DeviceInfo.platform must be android or ios.");
            }
        }

        private static FaceBiometricRecordDTO MapToDto(IntercomFaceBiometric record)
        {
            return new FaceBiometricRecordDTO
            {
                Id = record.Id,
                UserId = record.UserId,
                Files = new List<string> { record.FrontImageUrl, record.LeftImageUrl, record.RightImageUrl },
                DeviceInfo = new DeviceInfoDTO
                {
                    Platform = record.DevicePlatform ?? string.Empty,
                    Model = record.DeviceModel,
                    AppVersion = record.AppVersion
                },
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }

        private async Task UpsertTempIntercomAsync(int userId, string frontUrl, string leftUrl, string rightUrl)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null || user.UserTypeId != UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                return;
            }

            int? buildingId = null;
            int? unitId = null;
            YopoBackend.Modules.TenantCRUD.Models.Tenant? tenant = null;

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == userId);

            if (unit != null)
            {
                buildingId = unit.BuildingId;
                unitId = unit.UnitId;
                tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UnitId == unit.UnitId);

                if (tenant == null)
                {
                    tenant = await _context.Tenants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TenantId == userId);
                }
            }
            else
            {
                tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == userId);

                if (tenant != null)
                {
                    buildingId = tenant.BuildingId;
                    unitId = tenant.UnitId;
                }
            }

            if (tenant == null || !buildingId.HasValue)
            {
                return;
            }

            var existing = await _context.Set<TempIntercom>()
                .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId && t.BuildingId == buildingId.Value);

            if (existing == null)
            {
                existing = new TempIntercom
                {
                    TenantId = tenant.TenantId,
                    TenantName = tenant.TenantName,
                    UnitId = unitId ?? tenant.UnitId,
                    BuildingId = buildingId.Value,
                    FrontImageUrl = frontUrl,
                    LeftImageUrl = leftUrl,
                    RightImageUrl = rightUrl,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Add(existing);
            }
            else
            {
                existing.TenantName = tenant.TenantName;
                existing.UnitId = unitId ?? tenant.UnitId;
                existing.FrontImageUrl = frontUrl;
                existing.LeftImageUrl = leftUrl;
                existing.RightImageUrl = rightUrl;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
