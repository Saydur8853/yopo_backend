using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YopoBackend.Data;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Services
{
    /// <summary>
    /// Service for handling JWT token generation and validation with database storage.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationHours;

        /// <summary>
        /// Initializes a new instance of the JwtService class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="context">The database context.</param>
        public JwtService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                        _configuration["Jwt:SecretKey"] ?? 
                        "YourDefaultSecretKeyThatShouldBeAtLeast32CharactersLong";
            _issuer = _configuration["Jwt:Issuer"] ?? "YopoBackend";
            _audience = _configuration["Jwt:Audience"] ?? "YopoBackend";
            _expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="ipAddress">Optional IP address.</param>
        /// <returns>A tuple containing the JWT token string and its expiration date.</returns>
        public async Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user, string? deviceInfo = null, string? ipAddress = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var expiresAt = DateTime.UtcNow.AddHours(_expirationHours);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.EmailAddress),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim("UserTypeId", user.UserTypeId.ToString()),
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("IsEmailVerified", user.IsEmailVerified.ToString())
                }),
                Expires = expiresAt,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Store token in database
            var userToken = new UserToken
            {
                UserId = user.Id,
                TokenValue = tokenString,
                TokenType = "Access",
                ExpiresAt = expiresAt,
                IsActive = true,
                IsRevoked = false,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            return (tokenString, expiresAt);
        }

        /// <summary>
        /// Validates a JWT token and returns the user ID if valid.
        /// Also checks if the token exists in the database and is not revoked.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The user ID if the token is valid; otherwise, null.</returns>
        public async Task<int?> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Check if token exists in database and is valid
                    var userToken = await _context.UserTokens
                        .FirstOrDefaultAsync(t => t.TokenValue == token && t.UserId == userId);

                    if (userToken != null && userToken.IsValid)
                    {
                        // Update last used timestamp
                        userToken.LastUsedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        
                        return userId;
                    }
                }
            }
            catch
            {
                // Token validation failed
            }

            return null;
        }

        /// <summary>
        /// Extracts the user ID from a JWT token without validation.
        /// </summary>
        /// <param name="token">The JWT token from which to extract the user ID.</param>
        /// <returns>The user ID if found; otherwise, null.</returns>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Token reading failed
            }

            return null;
        }

        /// <summary>
        /// Revokes a specific token, making it invalid.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <returns>True if the token was successfully revoked; otherwise, false.</returns>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.TokenValue == token);

                if (userToken != null)
                {
                    userToken.IsRevoked = true;
                    userToken.IsActive = false;
                    userToken.RevokedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                // Token revocation failed
            }

            return false;
        }

        /// <summary>
        /// Revokes all tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID whose tokens should be revoked.</param>
        /// <returns>The number of tokens that were revoked.</returns>
        public async Task<int> RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var userTokens = await _context.UserTokens
                    .Where(t => t.UserId == userId && t.IsActive && !t.IsRevoked)
                    .ToListAsync();

                var revokedCount = 0;
                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                    token.IsActive = false;
                    token.RevokedAt = DateTime.UtcNow;
                    revokedCount++;
                }

                if (revokedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return revokedCount;
            }
            catch
            {
                // Token revocation failed
                return 0;
            }
        }

        /// <summary>
        /// Updates the last used timestamp for a token.
        /// </summary>
        /// <param name="token">The token to update.</param>
        /// <returns>True if the token was successfully updated; otherwise, false.</returns>
        public async Task<bool> UpdateTokenLastUsedAsync(string token)
        {
            try
            {
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.TokenValue == token);

                if (userToken != null)
                {
                    userToken.LastUsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                // Token update failed
            }

            return false;
        }

        /// <summary>
        /// Cleans up expired tokens from the database.
        /// </summary>
        /// <returns>The number of tokens that were cleaned up.</returns>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.UserTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.UserTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                    return expiredTokens.Count;
                }

                return 0;
            }
            catch
            {
                // Cleanup failed
                return 0;
            }
        }

        /// <summary>
        /// Gets all active tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of active tokens for the user.</returns>
        public async Task<List<UserToken>> GetUserActiveTokensAsync(int userId)
        {
            try
            {
                return await _context.UserTokens
                    .Where(t => t.UserId == userId && t.IsValid)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                // Failed to get tokens
                return new List<UserToken>();
            }
        }
    }
}
