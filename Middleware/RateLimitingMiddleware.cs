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
            
            // Apply rate limiting to authentication endpoints and PIN verification endpoint
            if (endpoint != null && (endpoint.Contains("/login") || endpoint.Contains("/register") || endpoint.Contains("/access/verify")))
            {
                var clientIp = GetClientIP(context);
                var now = DateTime.UtcNow;
                var windowMinutes = int.Parse(_configuration["RateLimit:WindowMinutes"] ?? "15");
                var maxRequests = int.Parse(_configuration["RateLimit:MaxRequests"] ?? "5");

                // Clean up old entries
                CleanupOldEntries(windowMinutes);

                // Build a key that differentiates verify attempts per intercom as well as per IP
                var isVerifyEndpoint = endpoint.Contains("/access/verify");
                string keySuffix;
                if (isVerifyEndpoint)
                {
                    var intercomId = ExtractIntercomId(endpoint);
                    keySuffix = $"/access/verify/{intercomId}";
                }
                else
                {
                    keySuffix = endpoint;
                }

                var key = $"{clientIp}_{keySuffix}";
                var rateLimitInfo = _requests.GetOrAdd(key, new RateLimitInfo());

                bool rateLimitExceeded = false;
                int backoffMs = 0;
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

                    // Simple backoff: for verify endpoint, start adding delay after half the maxRequests
                    if (!rateLimitExceeded && isVerifyEndpoint)
                    {
                        var threshold = Math.Max(1, maxRequests / 2);
                        if (rateLimitInfo.RequestCount > threshold)
                        {
                            backoffMs = (rateLimitInfo.RequestCount - threshold) * 100; // 100ms per request over threshold
                        }
                    }

                    // Set first request time if this is the first request
                    if (rateLimitInfo.RequestCount == 1)
                    {
                        rateLimitInfo.FirstRequestTime = now;
                    }
                }

                if (backoffMs > 0)
                {
                    await Task.Delay(backoffMs);
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

        private static string ExtractIntercomId(string endpoint)
        {
            // Expected pattern: /api/intercoms/{intercomId}/access/verify
            try
            {
                var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var idx = Array.IndexOf(segments, "intercoms");
                if (idx >= 0 && idx + 1 < segments.Length)
                {
                    var idSeg = segments[idx + 1];
                    return int.TryParse(idSeg, out _) ? idSeg : "unknown";
                }
            }
            catch
            {
                // ignore and fall through
            }
            return "unknown";
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
