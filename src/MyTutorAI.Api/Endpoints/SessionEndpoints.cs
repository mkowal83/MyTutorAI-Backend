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
    /// EN: Maps <c>POST /api/sessions</c>, which validates the request and opens a new session via <see cref="StartSessionHandler"/> (200, 400 validation problem or 401), and <c>GET /api/sessions/{sessionId}</c>, which returns the session state and its Working memory history via <see cref="GetSessionStateHandler"/> (200, 401 or 404), and <c>POST /api/sessions/{sessionId}/messages</c>, which validates the learner's message and returns the tutor's reply via <see cref="SendMessageHandler"/> (200, 400 validation problem, 401, 404, 409 for a closed session or 502 for an LLM failure), and <c>POST /api/sessions/{sessionId}/close</c>, which compresses the session into an episode and closes it via <see cref="CloseSessionHandler"/> (200, 401, 404, 409 for an already closed or empty session or 502 for an LLM failure).
    /// PL: Mapuje <c>POST /api/sessions</c>, który waliduje żądanie i otwiera nową sesję przez <see cref="StartSessionHandler"/> (200, 400 problem walidacji lub 401), oraz <c>GET /api/sessions/{sessionId}</c>, który zwraca stan sesji i jej historię z pamięci Working przez <see cref="GetSessionStateHandler"/> (200, 401 lub 404), oraz <c>POST /api/sessions/{sessionId}/messages</c>, który waliduje wiadomość ucznia i zwraca odpowiedź tutora przez <see cref="SendMessageHandler"/> (200, 400 problem walidacji, 401, 404, 409 dla zamkniętej sesji lub 502 dla awarii LLM), oraz <c>POST /api/sessions/{sessionId}/close</c>, który kompresuje sesję do epizodu i zamyka ją przez <see cref="CloseSessionHandler"/> (200, 401, 404, 409 dla już zamkniętej lub pustej sesji lub 502 dla awarii LLM).
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

        group.MapPost("/{sessionId:guid}/messages", SendMessageAsync)
            .AddEndpointFilter<ValidationFilter<SendMessageRequest>>()
            .WithName("Sessions_SendMessage")
            .WithSummary("Sends a student message — the tutor reply is generated via LLM structured output")
            .WithTags("Sessions")
            .Produces<SendMessageResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/{sessionId:guid}/close", CloseAsync)
            .WithName("Sessions_Close")
            .WithSummary("Closes the session and compresses it into an episode")
            .WithTags("Sessions")
            .Produces<CloseSessionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return endpoints;
    }

    private static Task<StartSessionResponse> StartAsync(
        StartSessionRequest request, StartSessionHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(request, cancellationToken);


    private static Task<SessionStateResponse> GetStateAsync(
        Guid sessionId, GetSessionStateHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(sessionId, cancellationToken);

    private static Task<SendMessageResponse> SendMessageAsync(
        Guid sessionId,
        SendMessageRequest request,
        SendMessageHandler handler,
        CancellationToken cancellationToken)
        => handler.HandleAsync(sessionId, request, cancellationToken);

    private static Task<CloseSessionResponse> CloseAsync(
        Guid sessionId, CloseSessionHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(sessionId, cancellationToken);
}
