using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Services
{
    /// <summary>
    /// Interface for JWT token generation and validation services.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user and stores it in the database.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="ipAddress">Optional IP address.</param>
        /// <returns>A tuple containing the JWT token string and its expiration date.</returns>
        Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user, string? deviceInfo = null, string? ipAddress = null);

        /// <summary>
        /// Generates a refresh token for the specified user and stores it in the database.
        /// </summary>
        /// <param name="userId">The user ID for whom to generate the refresh token.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="ipAddress">Optional IP address.</param>
        /// <returns>A tuple containing the refresh token string and its expiration date.</returns>
        Task<(string Token, DateTime ExpiresAt)> GenerateRefreshTokenAsync(int userId, string? deviceInfo = null, string? ipAddress = null);

        /// <summary>
        /// Gets a valid refresh token record for the provided token string.
        /// </summary>
        /// <param name="refreshToken">The refresh token from the client.</param>
        /// <returns>The refresh token record if valid; otherwise, null.</returns>
        Task<UserToken?> GetValidRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Rotates a refresh token by revoking the current one and issuing a new one.
        /// </summary>
        /// <param name="refreshToken">The refresh token record to rotate.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="ipAddress">Optional IP address.</param>
        /// <returns>A tuple containing the new refresh token string and its expiration date.</returns>
        Task<(string Token, DateTime ExpiresAt)> RotateRefreshTokenAsync(UserToken refreshToken, string? deviceInfo = null, string? ipAddress = null);

        /// <summary>
        /// Validates a JWT token and returns the user ID if valid.
        /// Also checks if the token exists in the database and is not revoked.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The user ID if the token is valid; otherwise, null.</returns>
        Task<int?> ValidateTokenAsync(string token);

        /// <summary>
        /// Extracts the user ID from a JWT token without validation.
        /// </summary>
        /// <param name="token">The JWT token from which to extract the user ID.</param>
        /// <returns>The user ID if found; otherwise, null.</returns>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// Revokes a specific token, making it invalid.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <returns>True if the token was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Revokes a specific refresh token, making it invalid.
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke.</param>
        /// <returns>True if the token was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes all tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID whose tokens should be revoked.</param>
        /// <returns>The number of tokens that were revoked.</returns>
        Task<int> RevokeAllUserTokensAsync(int userId);

        /// <summary>
        /// Revokes all refresh tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID whose refresh tokens should be revoked.</param>
        /// <returns>The number of refresh tokens that were revoked.</returns>
        Task<int> RevokeAllRefreshTokensAsync(int userId);

        /// <summary>
        /// Updates the last used timestamp for a token.
        /// </summary>
        /// <param name="token">The token to update.</param>
        /// <returns>True if the token was successfully updated; otherwise, false.</returns>
        Task<bool> UpdateTokenLastUsedAsync(string token);

        /// <summary>
        /// Cleans up expired tokens from the database.
        /// </summary>
        /// <returns>The number of tokens that were cleaned up.</returns>
        Task<int> CleanupExpiredTokensAsync();

        /// <summary>
        /// Gets all active tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of active tokens for the user.</returns>
        Task<List<UserToken>> GetUserActiveTokensAsync(int userId);
    }
}
