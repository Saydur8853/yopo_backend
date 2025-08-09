using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YopoBackend.Data;

namespace YopoBackend.Attributes
{
    /// <summary>
    /// Authorization attribute that checks if the current user has access to a specific module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireModuleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly int _moduleId;

        /// <summary>
        /// Initializes a new instance of the RequireModuleAttribute class.
        /// </summary>
        /// <param name="moduleId">The ID of the module that the user must have access to.</param>
        public RequireModuleAttribute(int moduleId)
        {
            _moduleId = moduleId;
        }

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get database context from DI container
            var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            if (dbContext == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Check if user has access to the required module
                var hasAccess = await dbContext.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .Join(dbContext.UserTypes,
                          u => u.UserTypeId,
                          ut => ut.Id,
                          (u, ut) => ut)
                    .Join(dbContext.UserTypeModulePermissions.Where(p => p.IsActive),
                          ut => ut.Id,
                          p => p.UserTypeId,
                          (ut, p) => p)
                    .Join(dbContext.Modules.Where(m => m.IsActive),
                          p => p.ModuleId,
                          m => m.Id,
                          (p, m) => m.Id)
                    .AnyAsync(moduleId => moduleId == _moduleId);

                if (!hasAccess)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception)
            {
                // If there's an error checking permissions, deny access
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }
}
