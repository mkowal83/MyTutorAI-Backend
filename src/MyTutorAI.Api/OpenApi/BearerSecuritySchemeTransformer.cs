using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MyTutorAI.Api.OpenApi;

/// <summary>
/// EN: Adds the JWT Bearer security scheme to the OpenAPI document and a security requirement to every operation whose endpoint requires authorization, so Swagger UI shows the "Authorize" button and sends the access token. Must be registered as both a document and an operation transformer.
/// PL: Dodaje schemat bezpieczeństwa JWT Bearer do dokumentu OpenAPI oraz wymaganie bezpieczeństwa do każdej operacji, której endpoint wymaga autoryzacji, dzięki czemu Swagger UI pokazuje przycisk "Authorize" i wysyła access token. Musi być zarejestrowany zarówno jako transformer dokumentu, jak i operacji.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string SchemeName = "Bearer";

    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Access token from POST /api/auth/login (without the 'Bearer ' prefix).",
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();

        if (requiresAuthorization)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, context.Document)] = [],
            });
        }

        return Task.CompletedTask;
    }
}
