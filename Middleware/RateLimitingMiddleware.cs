using System.Collections.Concurrent;

namespace YopoBackend.Middleware
{
    /// <summary>
    /// Middleware for rate limiting authentication endpoints.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
        
        public RateLimitingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value?.ToLower();
            
            // Apply rate limiting only to authentication endpoints
            if (endpoint != null && (endpoint.Contains("/login") || endpoint.Contains("/register")))
            {
                var clientIp = GetClientIP(context);
                var now = DateTime.UtcNow;
                var windowMinutes = int.Parse(_configuration["RateLimit:WindowMinutes"] ?? "15");
                var maxRequests = int.Parse(_configuration["RateLimit:MaxRequests"] ?? "5");

                // Clean up old entries
                CleanupOldEntries(windowMinutes);

                var key = $"{clientIp}_{endpoint}";
                var rateLimitInfo = _requests.GetOrAdd(key, new RateLimitInfo());

                bool rateLimitExceeded = false;
                lock (rateLimitInfo)
                {
                    // Reset counter if window has passed
                    if (now.Subtract(rateLimitInfo.FirstRequestTime).TotalMinutes > windowMinutes)
                    {
                        rateLimitInfo.RequestCount = 0;
                        rateLimitInfo.FirstRequestTime = now;
                    }

                    rateLimitInfo.RequestCount++;

                    if (rateLimitInfo.RequestCount > maxRequests)
                    {
                        rateLimitExceeded = true;
                    }

                    // Set first request time if this is the first request
                    if (rateLimitInfo.RequestCount == 1)
                    {
                        rateLimitInfo.FirstRequestTime = now;
                    }
                }

                if (rateLimitExceeded)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }
            }

            await _next(context);
        }

        private static string GetClientIP(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                   ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.ToString() 
                   ?? "unknown";
        }

        private static void CleanupOldEntries(int windowMinutes)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes * 2); // Keep for 2 windows
            var keysToRemove = _requests
                .Where(kvp => kvp.Value.FirstRequestTime < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _requests.TryRemove(key, out _);
            }
        }

        private class RateLimitInfo
        {
            public int RequestCount { get; set; }
            public DateTime FirstRequestTime { get; set; } = DateTime.UtcNow;
        }
    }
}
