using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using YopoBackend.Modules.UserCRUD.DTOs;

namespace YopoBackend.Services
{
    /// <summary>
    /// Firebase ID token verification using Firebase Admin SDK.
    /// </summary>
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly IConfiguration _configuration;
        private FirebaseAuth? _firebaseAuth;
        private static readonly object InitLock = new();

        public FirebaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SocialUserInfoDTO?> VerifyIdTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                return null;
            }

            try
            {
                if (!TryEnsureFirebaseAuth())
                {
                    return null;
                }

                var decoded = await _firebaseAuth!.VerifyIdTokenAsync(idToken);
                var email = decoded.Claims.TryGetValue("email", out var emailObj) ? emailObj?.ToString() : null;
                if (string.IsNullOrWhiteSpace(email))
                {
                    return null;
                }

                var name = decoded.Claims.TryGetValue("name", out var nameObj) ? nameObj?.ToString() : null;
                var picture = decoded.Claims.TryGetValue("picture", out var pictureObj) ? pictureObj?.ToString() : null;
                var phoneNumber = decoded.Claims.TryGetValue("phone_number", out var phoneObj) ? phoneObj?.ToString() : null;
                if (string.IsNullOrWhiteSpace(phoneNumber) &&
                    decoded.Claims.TryGetValue("phoneNumber", out var phoneAltObj))
                {
                    phoneNumber = phoneAltObj?.ToString();
                }

                var emailVerified = false;
                if (decoded.Claims.TryGetValue("email_verified", out var verifiedObj))
                {
                    if (verifiedObj is bool boolValue)
                    {
                        emailVerified = boolValue;
                    }
                    else if (bool.TryParse(verifiedObj?.ToString(), out var parsed))
                    {
                        emailVerified = parsed;
                    }
                }

                string? provider = null;
                if (decoded.Claims.TryGetValue("firebase", out var firebaseObj) &&
                    firebaseObj is IDictionary<string, object> firebaseDict &&
                    firebaseDict.TryGetValue("sign_in_provider", out var providerObj))
                {
                    provider = providerObj?.ToString();
                }

                return new SocialUserInfoDTO
                {
                    Email = email,
                    Name = name,
                    PictureUrl = picture,
                    EmailVerified = emailVerified,
                    Provider = provider,
                    PhoneNumber = phoneNumber
                };
            }
            catch (FirebaseAuthException)
            {
                return null;
            }
        }

        private bool TryEnsureFirebaseAuth()
        {
            if (_firebaseAuth != null)
            {
                return true;
            }

            lock (InitLock)
            {
                if (_firebaseAuth != null)
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

                _firebaseAuth = FirebaseAuth.DefaultInstance;
                return true;
            }
        }
    }
}
