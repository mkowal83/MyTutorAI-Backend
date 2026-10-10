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
    /// EN: Maps <c>POST /api/sessions</c>, which validates the request and opens a new session via <see cref="StartSessionHandler"/> (200, 400 validation problem or 401), and <c>GET /api/sessions/{sessionId}</c>, which returns the session state and its Working memory history via <see cref="GetSessionStateHandler"/> (200, 401 or 404).
    /// PL: Mapuje <c>POST /api/sessions</c>, który waliduje żądanie i otwiera nową sesję przez <see cref="StartSessionHandler"/> (200, 400 problem walidacji lub 401), oraz <c>GET /api/sessions/{sessionId}</c>, który zwraca stan sesji i jej historię z pamięci Working przez <see cref="GetSessionStateHandler"/> (200, 401 lub 404).
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

        group.MapGet("/{sessionId:guid}", GetStateAsync)
            .WithName("Sessions_GetState")
            .WithSummary("Returns the session state and the full working memory history")
            .WithTags("Sessions")
            .Produces<SessionStateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static Task<StartSessionResponse> StartAsync(
        StartSessionRequest request, StartSessionHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(request, cancellationToken);


    private static Task<SessionStateResponse> GetStateAsync(
        Guid sessionId, GetSessionStateHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(sessionId, cancellationToken);
}
