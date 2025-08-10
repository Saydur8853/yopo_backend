using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YopoBackend.Data;
using YopoBackend.Extensions;
using YopoBackend.Modules.InvitationCRUD.Services;
using YopoBackend.Modules.UserTypeCRUD.Services;
using YopoBackend.Modules.UserCRUD.Services;
using YopoBackend.Modules.BuildingCRUD.Services;
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Middleware;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register core services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Register module services
// Module: UserTypeCRUD (Module ID: 1 - defined in ModuleConstants.USER_TYPE_MODULE_ID)
builder.Services.AddScoped<IUserTypeService, UserTypeService>();

// Module: InvitationCRUD (Module ID: 2 - defined in ModuleConstants.INVITATION_MODULE_ID)
builder.Services.AddScoped<IInvitationService, InvitationService>();

// Module: UserCRUD (Module ID: 3 - defined in ModuleConstants.USER_MODULE_ID)
builder.Services.AddScoped<IUserService, UserService>();

// Module: BuildingCRUD (Module ID: 4)
builder.Services.AddScoped<IBuildingService, BuildingService>();

// Configure MySQL Database
var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                  builder.Configuration["Jwt:SecretKey"] ?? 
                  "YourDefaultSecretKeyThatShouldBeAtLeast32CharactersLong";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YopoBackend";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "YopoBackend";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Yopo Backend API", 
        Version = "v1",
        Description = "Yopo Backend API with module management system and JWT authentication"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Order tags alphabetically with Modules first
    c.TagActionsBy(api => new[] { GetControllerDisplayOrder(api.ActionDescriptor.RouteValues["controller"]) });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Helper function to define controller display order
static string GetControllerDisplayOrder(string? controllerName)
{
    return controllerName?.ToLower() switch
    {
        "modules" => "01-Modules",
        "usertypes" => "02-UserTypes",
        "invitations" => "03-Invitations",
        "users" => "04-Users",
        "buildings" => "05-Buildings",
        _ => $"99-{controllerName}"
    };
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// TEMPORARY: Enable Swagger unconditionally for debugging
var enableSwaggerEnvVar = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
var enableSwagger = true; // Force enable for debugging

// Log Swagger configuration for debugging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("ENABLE_SWAGGER Environment Variable: '{EnableSwaggerVar}'", enableSwaggerEnvVar ?? "null");
logger.LogInformation("Swagger Enabled: {SwaggerEnabled}", enableSwagger);

// Configure Swagger BEFORE static files
if (enableSwagger)
{
    logger.LogInformation("Configuring Swagger middleware...");
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentname}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Yopo Backend API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger path
        c.DocumentTitle = "Yopo Backend API Documentation";
    });
    logger.LogInformation("Swagger middleware configured successfully.");
}

// Configure default files (index.html) - must come after Swagger
app.UseDefaultFiles();

// Enable static files serving
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add rate limiting middleware (comment out for development if needed)
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is migrated and initialize default data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (app.Environment.IsProduction())
        {
            // In production (AWS), use migrations for proper schema management
            dbLogger.LogInformation("Production environment detected. Running database migrations...");
            await context.Database.MigrateAsync();
            dbLogger.LogInformation("Database migrations completed successfully!");
        }
        else
        {
            // For development: Use EnsureCreated for simplicity
            // COMMENTED OUT: This was causing data loss on every run
            // if (app.Environment.IsDevelopment())
            // {
            //     context.Database.EnsureDeleted();
            //     Console.WriteLine("Development mode: Dropped existing database");
            // }
            
            context.Database.EnsureCreated();
            dbLogger.LogInformation("Development database connection established successfully!");
        }
        
        dbLogger.LogInformation("Available tables: Modules, UserTypes, UserTypeModulePermissions, Invitations, Users, UserTokens, Buildings");
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
        throw;
    }
}

// 🚀 AUTOMATIC INITIALIZATION: This ensures Super Admin always has ALL module access!
// Every time you add new modules, Super Admin will automatically get access
await app.TryInitializeDefaultDataAsync();

app.Run();
