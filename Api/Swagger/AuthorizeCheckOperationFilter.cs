using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        var hasAuthorize = metadata.OfType<IAuthorizeData>().Any();

        if (!hasAuthorize)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document, null)] = new List<string>()
        });

        var responses = operation.Responses ??= new OpenApiResponses();

        responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized"
        });

        responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden"
        });
    }
}