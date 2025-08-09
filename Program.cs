using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YopoBackend.Data;
using YopoBackend.Extensions;
using YopoBackend.Modules.InvitationCRUD.Services;
using YopoBackend.Modules.UserTypeCRUD.Services;
using YopoBackend.Services;
using YopoBackend.Constants;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register core services
builder.Services.AddScoped<IModuleService, ModuleService>();

// Register module services
// Module: UserTypeCRUD (Module ID: 1 - defined in ModuleConstants.USER_TYPE_MODULE_ID)
builder.Services.AddScoped<IUserTypeService, UserTypeService>();

// Module: InvitationCRUD (Module ID: 2 - defined in ModuleConstants.INVITATION_MODULE_ID)
builder.Services.AddScoped<IInvitationService, InvitationService>();

// Configure MySQL Database
var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Yopo Backend API", 
        Version = "v1",
        Description = "Yopo Backend API with module management system"
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Yopo Backend API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger path
    });
}

// Configure default files (index.html) - must come before UseStaticFiles
app.UseDefaultFiles();

// Enable static files serving
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and initialize default data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // For development: Drop and recreate database to handle schema changes
        if (app.Environment.IsDevelopment())
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Development mode: Dropped existing database");
        }
        
        context.Database.EnsureCreated();
        Console.WriteLine("Database connection established successfully!");
        Console.WriteLine("Available tables: Modules, UserTypes, UserTypeModulePermissions, Invitations");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
        throw;
    }
}

// 🚀 AUTOMATIC INITIALIZATION: This ensures Super Admin always has ALL module access!
// Every time you add new modules, Super Admin will automatically get access
await app.TryInitializeDefaultDataAsync();

app.Run();
