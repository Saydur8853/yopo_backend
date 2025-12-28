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
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Middleware;
using DotNetEnv;
using Microsoft.AspNetCore.Rewrite;
using YopoBackend.Hubs;
using YopoBackend.Modules.AnnouncementCRUD.Services;
using YopoBackend.Modules.TicketCRUD.Services;
using YopoBackend.Modules.TermsConditionsCRUD.Services;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Register core services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Register module services
// Module: UserTypeCRUD (Module ID: 1 - defined in ModuleConstants.USER_TYPE_MODULE_ID)
builder.Services.AddScoped<IUserTypeService, UserTypeService>();

// Module: InvitationCRUD (Module ID: 2 - defined in ModuleConstants.INVITATION_MODULE_ID)
builder.Services.AddScoped<IInvitationService, InvitationService>();

// Module: UserCRUD (Module ID: 3 - defined in ModuleConstants.USER_MODULE_ID)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Module: BuildingCRUD (Module ID: 4 - defined in ModuleConstants.BUILDING_MODULE_ID)
builder.Services.AddScoped<YopoBackend.Modules.BuildingCRUD.Services.IBuildingService, YopoBackend.Modules.BuildingCRUD.Services.BuildingService>();

// Module: FloorCRUD
builder.Services.AddScoped<YopoBackend.Modules.FloorCRUD.Services.IFloorService, YopoBackend.Modules.FloorCRUD.Services.FloorService>();

// Module: UnitCRUD
builder.Services.AddScoped<YopoBackend.Modules.UnitCRUD.Services.IUnitService, YopoBackend.Modules.UnitCRUD.Services.UnitService>();

// Module: AmenityCRUD
builder.Services.AddScoped<YopoBackend.Modules.AmenityCRUD.Services.IAmenityService, YopoBackend.Modules.AmenityCRUD.Services.AmenityService>();

// Module: TenantCRUD
builder.Services.AddScoped<YopoBackend.Modules.TenantCRUD.Services.ITenantService, YopoBackend.Modules.TenantCRUD.Services.TenantService>();

// Module: IntercomCRUD
builder.Services.AddScoped<YopoBackend.Modules.IntercomCRUD.Services.IIntercomService, YopoBackend.Modules.IntercomCRUD.Services.IntercomService>();

// Module: Intercom Access Control
builder.Services.AddScoped<YopoBackend.Modules.IntercomAccess.Services.IIntercomAccessService, YopoBackend.Modules.IntercomAccess.Services.IntercomAccessService>();

// Module: CCTV (Module ID: 10)
// Module: CCTV (Module ID: 10)
builder.Services.AddScoped<YopoBackend.Modules.CCTVCRUD.Services.ICCTVService, YopoBackend.Modules.CCTVCRUD.Services.CCTVService>();

// Module: Messaging
builder.Services.AddScoped<YopoBackend.Modules.Messaging.Services.IMessageService, YopoBackend.Modules.Messaging.Services.MessageService>();

// Module: AnnouncementCRUD
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

// Module: ThreadSocial
builder.Services.AddScoped<YopoBackend.Modules.ThreadSocial.Services.IThreadSocialService, YopoBackend.Modules.ThreadSocial.Services.ThreadSocialService>();

// Module: TicketCRUD
builder.Services.AddScoped<ITicketService, TicketService>();

// Module: TermsConditionsCRUD
builder.Services.AddScoped<ITermsAndConditionsService, TermsAndConditionsService>();

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

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "You are not authenticated."
                });
                return context.Response.WriteAsync(payload);
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Access denied to the specified customer."
                });
                return context.Response.WriteAsync(payload);
            }
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
        "users" => "01-Users",
        "invitations" => "02-Invitations",
        "buildings" => "03-Buildings",
        "floors" => "04-Floors",
        "units" => "05-Units",
        "amenities" => "06-Amenities",
        "tenants" => "07-Tenants",
        "intercoms" => "08-Intercoms",
        "intercomaccess" => "09-Intercom Access",
        "cctv" => "10-CCTV",
        "accesscodes" => "09-Intercom Access",
        "modules" => "10-Modules",
        "usertypes" => "11-UserTypes",
        "accesslogs" => "12-Audit/Access Logs",
        "tickets" => "14-Tickets",
        _ => controllerName ?? "Other"
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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

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

// Add URL rewriting to redirect root path to auth.html
app.UseRewriter(new RewriteOptions()
    .AddRedirect("^$", "auth.html"));

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
app.MapHub<MessageHub>("/messageHub");
app.MapHub<ThreadSocialHub>("/threadSocialHub");
app.MapHub<AnnouncementHub>("/announcementHub");
app.MapHub<TicketHub>("/ticketHub");

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
        
        dbLogger.LogInformation("Available tables: Modules, UserTypes, UserTypeModulePermissions, Invitations, Users, UserTokens");
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
        throw;
    }
}

// 🚀 AUTOMATIC INITIALIZATION: This ensures Super Admin always has ALL module access!
// Every time you add new modules, Super Admin will automatically get access

// Add module checking before initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var moduleChecker = new YopoBackend.ModuleChecker(context);
    await moduleChecker.CheckModulesAsync();
}

await app.TryInitializeDefaultDataAsync();

app.Run();
