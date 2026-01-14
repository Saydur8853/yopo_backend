using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YopoBackend.Modules.AnnouncementCRUD.DTOs;

namespace YopoBackend.Services
{
    public class FcmNotificationService : IFcmNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FcmNotificationService> _logger;
        private static readonly object InitLock = new();
        private static bool _initialized;

        public FcmNotificationService(IConfiguration configuration, ILogger<FcmNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAnnouncementAsync(AnnouncementResponseDTO announcement)
        {
            if (!TryEnsureFirebaseApp())
            {
                _logger.LogWarning("Firebase app not configured. Skipping FCM for announcement {AnnouncementId}.", announcement.AnnouncementId);
                return false;
            }

            var title = string.IsNullOrWhiteSpace(announcement.Subject) ? "New announcement" : announcement.Subject;
            var body = announcement.Body ?? string.Empty;

            var message = new Message
            {
                Topic = BuildBuildingTopic(announcement.BuildingId),
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "announcement",
                    ["announcementId"] = announcement.AnnouncementId.ToString(),
                    ["buildingId"] = announcement.BuildingId.ToString()
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send FCM announcement {AnnouncementId}.", announcement.AnnouncementId);
                return false;
            }
        }

        private static string BuildBuildingTopic(int buildingId) => $"building_{buildingId}";

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
