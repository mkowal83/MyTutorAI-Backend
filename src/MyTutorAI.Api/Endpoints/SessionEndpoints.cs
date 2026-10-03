using MyTutorAI.Api.Validation;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Sessions;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the Sessions group under <c>/api/sessions</c>; all require an authenticated user.
/// PL: Endpointy Minimal API grupy Sesje pod <c>/api/sessions</c>; wszystkie wymagają uwierzytelnionego użytkownika.
/// </summary>
public static class SessionEndpoints
{
    /// <summary>
    /// EN: Maps <c>POST /api/sessions</c>, which validates the request and opens a new session via <see cref="StartSessionHandler"/>; responds 200, 400 (validation problem) or 401.
    /// PL: Mapuje <c>POST /api/sessions</c>, który waliduje żądanie i otwiera nową sesję przez <see cref="StartSessionHandler"/>; odpowiada 200, 400 (problem walidacji) lub 401.
    /// </summary>
    /// <param name="endpoints">
    /// EN: Route builder to add the endpoints to.
    /// PL: Builder tras, do którego dodawane są endpointy.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="endpoints"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="endpoints"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sessions")
            .RequireAuthorization();

        group.MapPost("/", StartAsync)
            .AddEndpointFilter<ValidationFilter<StartSessionRequest>>()
            .WithName("Sessions_Start")
            .WithSummary("Opens a new session")
            .WithTags("Sessions")
            .Produces<StartSessionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static Task<StartSessionResponse> StartAsync(
        StartSessionRequest request, StartSessionHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(request, cancellationToken);
}
