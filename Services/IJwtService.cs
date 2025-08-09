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
        /// Revokes all tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID whose tokens should be revoked.</param>
        /// <returns>The number of tokens that were revoked.</returns>
        Task<int> RevokeAllUserTokensAsync(int userId);

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
