using MyTutorAI.Api.Validation;
using MyTutorAI.Application.Contracts;
using MyTutorAI.Application.Demo;
using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Application.Options;

namespace MyTutorAI.Api.Endpoints;


/// <summary>
/// EN: Minimal API endpoints of the optional demo features, grouped under <c>/api/demo</c> and not tied to a specific student's memory.
/// PL: Endpointy Minimal API opcjonalnych funkcji demo, zgrupowane pod <c>/api/demo</c> i niezwiązane z pamięcią konkretnego ucznia.
/// </summary>
public static class DemoEndpoints
{
    /// <summary>
    /// EN: Maps <c>POST /api/demo/dialogue</c> only when <see cref="FeaturesOptions.AllowDialogue"/> is true; otherwise the route does not exist and returns a routing 404. The flag is read once from <paramref name="configuration"/> at startup.
    /// PL: Mapuje <c>POST /api/demo/dialogue</c> tylko, gdy <see cref="FeaturesOptions.AllowDialogue"/> ma wartość true; w przeciwnym razie trasa nie istnieje i zwraca 404 routingu. Flaga jest odczytywana raz z <paramref name="configuration"/> przy starcie.
    /// </summary>
    /// <param name="endpoints">
    /// EN: Route builder the endpoints are added to.
    /// PL: Builder tras, do którego dodawane są endpointy.
    /// </param>
    /// <param name="configuration">
    /// EN: Application configuration holding the <c>Features</c> section; a missing section means all demo features are disabled.
    /// PL: Konfiguracja aplikacji z sekcją <c>Features</c>; brak sekcji oznacza, że wszystkie funkcje demo są wyłączone.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="endpoints"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="endpoints"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var features = configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>()
            ?? new FeaturesOptions();

        // TODO(auth): przywrócić .RequireAuthorization() razem z AddAuthentication/UseAuthorization.
        var group = endpoints.MapGroup("/api/demo");

        if (features.AllowDialogue)
        {
            group.MapPost("/dialogue", GenerateDialogueAsync)
                .AddEndpointFilter<ValidationFilter<DialogueGenerationRequest>>()
                //.RequireRateLimiting(MyTutorAI.Infrastructure.DependencyInjection.LlmRateLimiterPolicyName)
                .WithName("Demo_GenerateDialogue")
                .WithSummary("Generuje demonstracyjny dialog ćwiczeniowy o zadanym temacie (LLM)")
                .WithTags("Demo")
                .Produces<DialogueGenerationResponse>(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                //.ProducesProblem(StatusCodes.Status401Unauthorized) // TODO(auth)
                .ProducesProblem(StatusCodes.Status429TooManyRequests)
                .ProducesProblem(StatusCodes.Status502BadGateway);
        }

        return endpoints;
    }

    private static Task<DialogueGenerationResponse> GenerateDialogueAsync( DialogueGenerationRequest request,
                                                                           GenerateDialogueHandler handler,
                                                                           CancellationToken cancellationToken)
                                                                           => handler.HandleAsync(request, cancellationToken);
}
