using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YopoBackend.Swagger
{
    public class AccessCodesExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
            if (!string.Equals(relativePath, "api/access-codes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!operation.Responses.TryGetValue("200", out var response))
            {
                return;
            }

            if (!response.Content.TryGetValue("application/json", out var mediaType))
            {
                mediaType = new OpenApiMediaType();
                response.Content["application/json"] = mediaType;
            }

            mediaType.Example = new OpenApiObject
            {
                ["data"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiInteger(12),
                        ["buildingId"] = new OpenApiInteger(3),
                        ["intercomId"] = new OpenApiInteger(5),
                        ["tenantId"] = new OpenApiInteger(42),
                        ["code"] = new OpenApiString("839204"),
                        ["expiresAt"] = new OpenApiString("2025-12-31T23:59:59Z"),
                        ["isActive"] = new OpenApiBoolean(true),
                        ["createdAt"] = new OpenApiString("2025-01-15T10:30:00Z")
                    }
                },
                ["totalCount"] = new OpenApiInteger(1),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20),
                ["totalPages"] = new OpenApiInteger(1),
                ["hasPreviousPage"] = new OpenApiBoolean(false),
                ["hasNextPage"] = new OpenApiBoolean(false)
            };
        }
    }
}
