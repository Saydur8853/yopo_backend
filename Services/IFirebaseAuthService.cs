using YopoBackend.Modules.UserCRUD.DTOs;

namespace YopoBackend.Services
{
    /// <summary>
    /// Service for verifying Firebase ID tokens.
    /// </summary>
    public interface IFirebaseAuthService
    {
        /// <summary>
        /// Verifies the Firebase ID token and returns social user info.
        /// </summary>
        /// <param name="idToken">Firebase ID token.</param>
        /// <returns>Verified social user info, or null when invalid.</returns>
        Task<SocialUserInfoDTO?> VerifyIdTokenAsync(string idToken);
    }
}
