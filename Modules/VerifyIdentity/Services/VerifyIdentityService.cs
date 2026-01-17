using Microsoft.EntityFrameworkCore;
using YopoBackend.Auth;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.VerifyIdentity.DTOs;
using YopoBackend.Modules.VerifyIdentity.Models;
using YopoBackend.Services;
using YopoBackend.Utils;

namespace YopoBackend.Modules.VerifyIdentity.Services
{
    public class VerifyIdentityService : BaseAccessControlService, IVerifyIdentityService
    {
        private const int MaxDocumentSizeBytes = 10 * 1024 * 1024;
        private readonly ICloudinaryService _cloudinaryService;

        public VerifyIdentityService(ApplicationDbContext context, ICloudinaryService cloudinaryService) : base(context)
        {
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IdentityVerificationResponseDTO> CreateRequestAsync(int userId, string userRole, CreateIdentityVerificationDTO dto)
        {
            if (!IsTenantRole(userRole))
            {
                throw new UnauthorizedAccessException("Only tenants can upload identity documents.");
            }

            if (dto.Documents == null || dto.Documents.Count == 0)
            {
                throw new ArgumentException("At least one document is required.");
            }

            var buildingId = await ResolveTenantBuildingIdAsync(userId);
            if (!buildingId.HasValue)
            {
                throw new InvalidOperationException("Tenant is not assigned to a building.");
            }

            var request = new IdentityVerificationRequest
            {
                TenantId = userId,
                BuildingId = buildingId.Value,
                Status = IdentityVerificationStatus.Pending,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var document in dto.Documents)
            {
                var documentType = document.DocumentType?.Trim();
                if (string.IsNullOrWhiteSpace(documentType))
                {
                    throw new ArgumentException("DocumentType is required.");
                }

                var uploads = await UploadDocumentAsync(document);
                foreach (var upload in uploads)
                {
                    request.Documents.Add(new IdentityVerificationDocument
                    {
                        DocumentType = documentType,
                        DocumentUrl = upload.Url,
                        DocumentPublicId = upload.PublicId,
                        MimeType = upload.MimeType,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.IdentityVerificationRequests.Add(request);
            await _context.SaveChangesAsync();

            return MapToResponse(request);
        }

        public async Task<(List<IdentityVerificationResponseDTO> Requests, int TotalRecords)> GetRequestsAsync(
            int userId,
            string userRole,
            int? buildingId,
            int? tenantId,
            string? status,
            int page,
            int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var query = _context.IdentityVerificationRequests
                .AsNoTracking()
                .Include(r => r.Documents)
                .AsQueryable();

            if (IsTenantRole(userRole))
            {
                query = query.Where(r => r.TenantId == userId);
            }
            else if (IsManagerRole(userRole))
            {
                if (!IsSuperAdminRole(userRole))
                {
                    if (buildingId.HasValue)
                    {
                        var hasAccess = await HasBuildingAccessAsync(userId, buildingId.Value);
                        if (!hasAccess)
                        {
                            throw new UnauthorizedAccessException("You do not have access to this building.");
                        }
                        query = query.Where(r => r.BuildingId == buildingId.Value);
                    }
                    else
                    {
                        var buildingIds = await GetUserBuildingIdsAsync(userId);
                        if (!buildingIds.Any())
                        {
                            return (new List<IdentityVerificationResponseDTO>(), 0);
                        }
                        query = query.Where(r => r.BuildingId.HasValue && buildingIds.Contains(r.BuildingId.Value));
                    }
                }
                else if (buildingId.HasValue)
                {
                    query = query.Where(r => r.BuildingId == buildingId.Value);
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Only tenants or property managers can access this resource.");
            }

            if (tenantId.HasValue && tenantId.Value > 0)
            {
                query = query.Where(r => r.TenantId == tenantId.Value);
            }

            var normalizedStatus = NormalizeStatus(status);
            if (!string.IsNullOrWhiteSpace(status) && !IsAllowedStatus(normalizedStatus))
            {
                throw new ArgumentException($"Status must be {IdentityVerificationStatus.Pending} or {IdentityVerificationStatus.Done}.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
            {
                query = query.Where(r => r.Status == normalizedStatus);
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToResponse).ToList();
            return (responses, totalRecords);
        }

        public async Task<IdentityVerificationResponseDTO> UpdateDocumentsAsync(int requestId, int userId, string userRole, UpdateIdentityVerificationDocumentsDTO dto)
        {
            if (dto.Documents == null || dto.Documents.Count == 0)
            {
                throw new ArgumentException("At least one document is required.");
            }

            var request = await _context.IdentityVerificationRequests
                .Include(r => r.Documents)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new KeyNotFoundException("Verification request not found.");
            }

            await EnsureUpdateDeleteAccessAsync(request, userId, userRole, "update");

            var existingDocs = request.Documents.ToList();
            await DeleteDocumentsAsync(existingDocs);
            _context.IdentityVerificationDocuments.RemoveRange(existingDocs);
            request.Documents.Clear();

            foreach (var document in dto.Documents)
            {
                var documentType = document.DocumentType?.Trim();
                if (string.IsNullOrWhiteSpace(documentType))
                {
                    throw new ArgumentException("DocumentType is required.");
                }

                var uploads = await UploadDocumentAsync(document);
                foreach (var upload in uploads)
                {
                    request.Documents.Add(new IdentityVerificationDocument
                    {
                        DocumentType = documentType,
                        DocumentUrl = upload.Url,
                        DocumentPublicId = upload.PublicId,
                        MimeType = upload.MimeType,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (IsTenantRole(userRole))
            {
                request.Status = IdentityVerificationStatus.Pending;
                request.VerifiedBy = null;
                request.VerifiedAt = null;
            }
            else if (string.Equals(request.Status, IdentityVerificationStatus.Done, StringComparison.OrdinalIgnoreCase))
            {
                request.VerifiedBy = userId;
                request.VerifiedAt = DateTime.UtcNow;
            }
            else
            {
                request.VerifiedBy = null;
                request.VerifiedAt = null;
            }

            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(request);
        }

        public async Task DeleteRequestAsync(int requestId, int userId, string userRole)
        {
            var request = await _context.IdentityVerificationRequests
                .Include(r => r.Documents)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new KeyNotFoundException("Verification request not found.");
            }

            await EnsureUpdateDeleteAccessAsync(request, userId, userRole, "delete");

            await DeleteDocumentsAsync(request.Documents.ToList());

            _context.IdentityVerificationRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task<IdentityVerificationResponseDTO> UpdateStatusAsync(int requestId, int userId, string userRole, string status)
        {
            if (!IsManagerRole(userRole))
            {
                throw new UnauthorizedAccessException("Only property managers can verify identity documents.");
            }

            var normalizedStatus = NormalizeStatus(status);
            if (string.IsNullOrWhiteSpace(normalizedStatus))
            {
                throw new ArgumentException("Status is required.");
            }

            if (!IsAllowedStatus(normalizedStatus))
            {
                throw new ArgumentException($"Status must be {IdentityVerificationStatus.Pending} or {IdentityVerificationStatus.Done}.");
            }

            var request = await _context.IdentityVerificationRequests
                .Include(r => r.Documents)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new KeyNotFoundException("Verification request not found.");
            }

            if (!IsSuperAdminRole(userRole))
            {
                if (!request.BuildingId.HasValue)
                {
                    throw new UnauthorizedAccessException("Verification request is not tied to a building.");
                }

                var hasAccess = await HasBuildingAccessAsync(userId, request.BuildingId.Value);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You do not have access to this building.");
                }
            }

            request.Status = normalizedStatus;
            request.UpdatedAt = DateTime.UtcNow;

            if (string.Equals(normalizedStatus, IdentityVerificationStatus.Done, StringComparison.OrdinalIgnoreCase))
            {
                request.VerifiedBy = userId;
                request.VerifiedAt = DateTime.UtcNow;
            }
            else
            {
                request.VerifiedBy = null;
                request.VerifiedAt = null;
            }

            await _context.SaveChangesAsync();

            return MapToResponse(request);
        }

        private async Task<int?> ResolveTenantBuildingIdAsync(int tenantUserId)
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantUserId);
            if (unit != null)
            {
                return unit.BuildingId;
            }

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantUserId);
            if (tenant != null)
            {
                return tenant.BuildingId;
            }

            return null;
        }

        private async Task<bool> HasBuildingAccessAsync(int userId, int buildingId)
        {
            var user = await GetUserWithAccessControlAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return true;
            }

            var buildingIds = await GetUserBuildingIdsAsync(userId);
            return buildingIds.Contains(buildingId);
        }

        private async Task<List<int>> GetUserBuildingIdsAsync(int userId)
        {
            var explicitBuildingIds = await _context.UserBuildingPermissions
                .Where(p => p.UserId == userId && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();

            if (explicitBuildingIds.Any())
            {
                return explicitBuildingIds;
            }

            int? pmId = null;
            var user = await GetUserWithAccessControlAsync(userId);

            if (user?.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                pmId = user.Id;
            }
            else if (user != null)
            {
                pmId = await FindPropertyManagerForUserAsync(user.Id);
            }

            if (pmId.HasValue)
            {
                return await _context.Buildings
                    .Where(b => b.CustomerId == pmId.Value && b.IsActive)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
            }

            return new List<int>();
        }

        private async Task<List<DocumentUploadResult>> UploadDocumentAsync(IdentityVerificationDocumentUploadDTO document)
        {
            var hasFront = !string.IsNullOrWhiteSpace(document.DocumentImageFront);
            var hasBack = !string.IsNullOrWhiteSpace(document.DocumentImageBack);
            var hasPdf = !string.IsNullOrWhiteSpace(document.DocumentPdf);

            if (!hasFront && !hasBack && !hasPdf)
            {
                throw new ArgumentException("DocumentImageFront, DocumentImageBack, or DocumentPdf is required.");
            }

            if (hasPdf && (hasFront || hasBack))
            {
                throw new ArgumentException("Provide either DocumentPdf or document images, not both.");
            }

            var uploads = new List<DocumentUploadResult>();

            if (hasPdf)
            {
                uploads.Add(await UploadPdfAsync(document.DocumentPdf!));
                return uploads;
            }

            if (hasFront)
            {
                uploads.Add(await UploadImageAsync(document.DocumentImageFront!));
            }

            if (hasBack)
            {
                uploads.Add(await UploadImageAsync(document.DocumentImageBack!));
            }

            return uploads;
        }

        private async Task<DocumentUploadResult> UploadPdfAsync(string base64Payload)
        {
            var parsed = ParseBase64Payload(base64Payload);

            if (parsed.Bytes.Length == 0)
            {
                throw new ArgumentException("Document data is empty.");
            }

            if (parsed.Bytes.Length > MaxDocumentSizeBytes)
            {
                var sizeMb = parsed.Bytes.Length / (1024.0 * 1024.0);
                throw new ArgumentException($"Document size ({sizeMb:F1}MB) exceeds maximum allowed size (10MB).");
            }

            if (!IsPdf(parsed.Bytes))
            {
                throw new ArgumentException("DocumentPdf must be a PDF file.");
            }

            var upload = await _cloudinaryService.UploadIdentityDocumentAsync(parsed.Bytes, "application/pdf");
            return new DocumentUploadResult(upload.Url, upload.PublicId, "application/pdf");
        }

        private async Task<DocumentUploadResult> UploadImageAsync(string base64Payload)
        {
            var imageValidation = ImageUtils.ValidateBase64Image(base64Payload);
            if (!imageValidation.IsValid || imageValidation.ImageBytes == null || string.IsNullOrWhiteSpace(imageValidation.MimeType))
            {
                throw new ArgumentException($"Invalid document image: {imageValidation.ErrorMessage ?? "Unsupported file type."}");
            }

            var imageUpload = await _cloudinaryService.UploadIdentityDocumentAsync(imageValidation.ImageBytes, imageValidation.MimeType);
            return new DocumentUploadResult(imageUpload.Url, imageUpload.PublicId, imageValidation.MimeType);
        }

        private async Task DeleteDocumentsAsync(IEnumerable<IdentityVerificationDocument> documents)
        {
            foreach (var document in documents)
            {
                if (string.IsNullOrWhiteSpace(document.DocumentPublicId))
                {
                    continue;
                }

                var resourceType = GetResourceType(document);
                await _cloudinaryService.DeleteAssetAsync(document.DocumentPublicId, resourceType);
            }
        }

        private async Task EnsureUpdateDeleteAccessAsync(IdentityVerificationRequest request, int userId, string userRole, string action)
        {
            if (IsTenantRole(userRole))
            {
                if (request.TenantId != userId)
                {
                    throw new UnauthorizedAccessException("You do not have access to this verification request.");
                }

                if (string.Equals(request.Status, IdentityVerificationStatus.Done, StringComparison.OrdinalIgnoreCase))
                {
                    var verb = string.Equals(action, "update", StringComparison.OrdinalIgnoreCase) ? "update" : "delete";
                    throw new UnauthorizedAccessException($"You can't {verb} verified document, contact with property authority in chat box.");
                }

                return;
            }

            if (!IsManagerRole(userRole))
            {
                throw new UnauthorizedAccessException("Only tenants or property managers can access this resource.");
            }

            if (IsSuperAdminRole(userRole))
            {
                return;
            }

            if (!request.BuildingId.HasValue)
            {
                throw new UnauthorizedAccessException("Verification request is not tied to a building.");
            }

            var hasAccess = await HasBuildingAccessAsync(userId, request.BuildingId.Value);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have access to this building.");
            }
        }

        private static Base64Payload ParseBase64Payload(string base64Payload)
        {
            var trimmed = base64Payload.Trim();
            var data = trimmed;

            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = trimmed.Split(',', 2);
                if (parts.Length == 2)
                {
                    data = parts[1];
                }
            }

            try
            {
                var bytes = Convert.FromBase64String(data);
                return new Base64Payload(bytes);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid base64 format.");
            }
        }

        private static bool IsPdf(byte[] bytes)
        {
            return bytes.Length >= 4 &&
                   bytes[0] == 0x25 &&
                   bytes[1] == 0x50 &&
                   bytes[2] == 0x44 &&
                   bytes[3] == 0x46;
        }

        private static string? NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            var trimmed = status.Trim();

            if (string.Equals(trimmed, IdentityVerificationStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return IdentityVerificationStatus.Pending;
            }

            if (string.Equals(trimmed, IdentityVerificationStatus.Done, StringComparison.OrdinalIgnoreCase))
            {
                return IdentityVerificationStatus.Done;
            }

            return trimmed;
        }

        private static bool IsAllowedStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return string.Equals(status, IdentityVerificationStatus.Pending, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, IdentityVerificationStatus.Done, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetResourceType(IdentityVerificationDocument document)
        {
            if (!string.IsNullOrWhiteSpace(document.MimeType))
            {
                return document.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ? "image" : "raw";
            }

            if (!string.IsNullOrWhiteSpace(document.DocumentUrl))
            {
                var url = document.DocumentUrl.ToLowerInvariant();
                if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".webp") || url.EndsWith(".bmp"))
                {
                    return "image";
                }
            }

            return "raw";
        }

        private static bool IsTenantRole(string userRole)
        {
            return string.Equals(userRole, Roles.Tenant, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(userRole, UserTypeConstants.TENANT_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManagerRole(string userRole)
        {
            return string.Equals(userRole, Roles.PropertyManager, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(userRole, UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase) ||
                   IsSuperAdminRole(userRole);
        }

        private static bool IsSuperAdminRole(string userRole)
        {
            return string.Equals(userRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(userRole, UserTypeConstants.SUPER_ADMIN_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase);
        }

        private static IdentityVerificationResponseDTO MapToResponse(IdentityVerificationRequest request)
        {
            return new IdentityVerificationResponseDTO
            {
                RequestId = request.RequestId,
                TenantId = request.TenantId,
                BuildingId = request.BuildingId,
                Status = request.Status,
                VerifiedBy = request.VerifiedBy,
                VerifiedAt = request.VerifiedAt,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Documents = request.Documents
                    .Select(d => new IdentityVerificationDocumentResponseDTO
                    {
                        DocumentId = d.DocumentId,
                        DocumentType = d.DocumentType,
                        DocumentUrl = d.DocumentUrl,
                        MimeType = d.MimeType,
                        CreatedAt = d.CreatedAt
                    })
                    .ToList()
            };
        }

        private readonly record struct Base64Payload(byte[] Bytes);

        private readonly record struct DocumentUploadResult(string Url, string PublicId, string MimeType);
    }
}
