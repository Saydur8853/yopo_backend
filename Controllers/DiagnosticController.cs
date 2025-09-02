using Microsoft.AspNetCore.Mvc;

namespace YopoBackend.Controllers;

/// <summary>
/// Diagnostic controller for debugging deployment issues
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly ILogger<DiagnosticController> _logger;
    private readonly IWebHostEnvironment _environment;

    public DiagnosticController(ILogger<DiagnosticController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Get basic diagnostic information about the application
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetDiagnosticInfo()
    {
        var enableSwaggerEnv = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
        var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        var info = new
        {
            Environment = _environment.EnvironmentName,
            IsDevelopment = _environment.IsDevelopment(),
            IsProduction = _environment.IsProduction(),
            EnableSwaggerEnvVar = enableSwaggerEnv ?? "null",
            AspNetCoreEnvironment = aspNetCoreEnv ?? "null",
            MachineName = Environment.MachineName,
            ApplicationName = _environment.ApplicationName,
            ContentRootPath = _environment.ContentRootPath,
            WebRootPath = _environment.WebRootPath,
            Timestamp = DateTime.UtcNow,
            AllEnvironmentVariables = Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Where(e => e.Key.ToString()!.StartsWith("ENABLE_") || 
                           e.Key.ToString()!.StartsWith("ASPNETCORE_") || 
                           e.Key.ToString()!.StartsWith("yopo_"))
                .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "null")
        };

        _logger.LogInformation("Diagnostic info requested: {@DiagnosticInfo}", info);
        
        return Ok(info);
    }

    /// <summary>
    /// Test if the API is responding
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { Message = "Pong!", Timestamp = DateTime.UtcNow });
    }
}
