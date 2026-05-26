using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BarberSync.Api.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileUpload = context.ApiDescription.ParameterDescriptions.Any(p =>
            p.Type == typeof(IFormFile) ||
            p.Type?.GetProperties().Any(prop => prop.PropertyType == typeof(IFormFile)) == true);

        if (!hasFileUpload)
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["image"] = new() { Type = "string", Format = "binary" },
                            ["appointmentId"] = new() { Type = "string", Format = "uuid", Nullable = true },
                            ["clientId"] = new() { Type = "string", Format = "uuid", Nullable = true },
                            ["professionalId"] = new() { Type = "string", Format = "uuid", Nullable = true },
                            ["tenantId"] = new() { Type = "string", Format = "uuid", Nullable = true },
                            ["branchId"] = new() { Type = "string", Format = "uuid", Nullable = true }
                        },
                        Required = new HashSet<string> { "image" }
                    }
                }
            }
        };
    }
}
