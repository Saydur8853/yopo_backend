using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.AnnouncementCRUD.DTOs;

namespace YopoBackend.Services
{
    public class FcmNotificationService : IFcmNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FcmNotificationService> _logger;
        private readonly ApplicationDbContext _context;
        private static readonly object InitLock = new();
        private static bool _initialized;
        private const int MaxTokensPerRequest = 500;

        public FcmNotificationService(
            IConfiguration configuration,
            ILogger<FcmNotificationService> logger,
            ApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> SendAnnouncementAsync(AnnouncementResponseDTO announcement)
        {
            if (!TryEnsureFirebaseApp())
            {
                _logger.LogWarning("Firebase app not configured. Skipping FCM for announcement {AnnouncementId}.", announcement.AnnouncementId);
                return false;
            }

            var tokens = await GetAnnouncementTokensAsync(announcement.BuildingId);
            if (tokens.Count == 0)
            {
                _logger.LogInformation("No FCM tokens found for announcement {AnnouncementId} building {BuildingId}.",
                    announcement.AnnouncementId, announcement.BuildingId);
                return false;
            }

            var disableBatch = (Environment.GetEnvironmentVariable("FIREBASE_DISABLE_BATCH")
                ?? _configuration["Firebase:DisableBatch"])?.ToLowerInvariant() == "true";
            var title = string.IsNullOrWhiteSpace(announcement.Subject) ? "New announcement" : announcement.Subject;
            var body = announcement.Body ?? string.Empty;
            var notification = new Notification
            {
                Title = title,
                Body = body
            };
            var data = new Dictionary<string, string>
            {
                ["type"] = "announcement",
                ["announcementId"] = announcement.AnnouncementId.ToString(),
                ["buildingId"] = announcement.BuildingId.ToString()
            };

            try
            {
                if (disableBatch)
                {
                    return await SendTokensIndividuallyAsync(tokens, notification, data, announcement.AnnouncementId);
                }

                var anySuccess = false;
                foreach (var batch in BatchTokens(tokens, MaxTokensPerRequest))
                {
                    var message = new MulticastMessage
                    {
                        Tokens = batch,
                        Notification = notification,
                        Data = data
                    };

                    var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
                    if (response.SuccessCount > 0)
                    {
                        anySuccess = true;
                    }

                    if (response.FailureCount > 0)
                    {
                        _logger.LogWarning(
                            "FCM announcement {AnnouncementId} had {FailureCount} failures out of {Total}.",
                            announcement.AnnouncementId,
                            response.FailureCount,
                            response.Responses.Count);
                    }
                }

                return anySuccess;
            }
            catch (FirebaseMessagingException ex) when (IsBatchNotFound(ex))
            {
                _logger.LogWarning(ex, "FCM batch endpoint not available. Falling back to per-token sends.");
                return await SendTokensIndividuallyAsync(tokens, notification, data, announcement.AnnouncementId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send FCM announcement {AnnouncementId}.", announcement.AnnouncementId);
                return false;
            }
        }

        private async Task<List<string>> GetAnnouncementTokensAsync(int buildingId)
        {
            var building = await _context.Buildings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);
            if (building == null)
            {
                return new List<string>();
            }

            var pmId = building.CustomerId;
            var ecosystemUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && (u.Id == pmId || u.InviteById == pmId || u.CreatedBy == pmId))
                .Select(u => new { u.Id, u.UserTypeId })
                .ToListAsync();

            var nonTenantIds = ecosystemUsers
                .Where(u => u.UserTypeId != UserTypeConstants.TENANT_USER_TYPE_ID)
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            var permissions = await _context.UserBuildingPermissions
                .AsNoTracking()
                .Where(p => nonTenantIds.Contains(p.UserId) && p.IsActive)
                .Select(p => new { p.UserId, p.BuildingId })
                .ToListAsync();

            var usersWithAnyPermissions = permissions
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            var allowedByBuilding = permissions
                .Where(p => p.BuildingId == buildingId)
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            var withoutPermissions = nonTenantIds
                .Except(usersWithAnyPermissions);

            var tenantUserIds = await _context.Units
                .AsNoTracking()
                .Where(u => u.BuildingId == buildingId && u.TenantId.HasValue)
                .Select(u => u.TenantId!.Value)
                .Distinct()
                .ToListAsync();

            var recipientIds = withoutPermissions
                .Concat(allowedByBuilding)
                .Concat(tenantUserIds)
                .Distinct()
                .ToList();

            if (recipientIds.Count == 0)
            {
                return new List<string>();
            }

            return await _context.UserFcmTokens
                .AsNoTracking()
                .Where(t => t.IsActive && recipientIds.Contains(t.UserId))
                .Select(t => t.Token)
                .Distinct()
                .ToListAsync();
        }

        private static IEnumerable<List<string>> BatchTokens(List<string> tokens, int batchSize)
        {
            for (var i = 0; i < tokens.Count; i += batchSize)
            {
                yield return tokens.Skip(i).Take(batchSize).ToList();
            }
        }

        private async Task<bool> SendTokensIndividuallyAsync(
            List<string> tokens,
            Notification notification,
            Dictionary<string, string> data,
            int announcementId)
        {
            var anySuccess = false;

            foreach (var token in tokens)
            {
                var message = new Message
                {
                    Token = token,
                    Notification = notification,
                    Data = data
                };

                try
                {
                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    anySuccess = true;
                }
                catch (FirebaseMessagingException ex)
                {
                    _logger.LogWarning(ex, "Failed to send FCM announcement {AnnouncementId} to a device token.", announcementId);
                }
            }

            return anySuccess;
        }

        private static bool IsBatchNotFound(Exception ex)
        {
            if (ex is FirebaseMessagingException firebaseEx && firebaseEx.InnerException is GoogleApiException googleEx)
            {
                return googleEx.HttpStatusCode == HttpStatusCode.NotFound;
            }

            return ex is GoogleApiException apiEx && apiEx.HttpStatusCode == HttpStatusCode.NotFound;
        }

        private bool TryEnsureFirebaseApp()
        {
            if (_initialized)
            {
                return true;
            }

            lock (InitLock)
            {
                if (_initialized)
                {
                    return true;
                }

                var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
                    ?? _configuration["Firebase:ProjectId"];
                var serviceAccountJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON")
                    ?? _configuration["Firebase:ServiceAccountJson"];
                var serviceAccountPath = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_PATH")
                    ?? _configuration["Firebase:ServiceAccountPath"];
                var useAdc = (Environment.GetEnvironmentVariable("FIREBASE_USE_ADC")
                    ?? _configuration["Firebase:UseAdc"])?.ToLowerInvariant() == "true";

                GoogleCredential? credential = null;
                if (!string.IsNullOrWhiteSpace(serviceAccountJson))
                {
                    credential = GoogleCredential.FromJson(serviceAccountJson);
                }
                else if (!string.IsNullOrWhiteSpace(serviceAccountPath))
                {
                    credential = GoogleCredential.FromFile(serviceAccountPath);
                }
                else if (useAdc)
                {
                    credential = GoogleCredential.GetApplicationDefault();
                }
                else
                {
                    return false;
                }

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                }

                _initialized = true;
                return true;
            }
        }
    }
}
