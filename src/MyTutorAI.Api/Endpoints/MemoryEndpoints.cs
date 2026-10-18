using MyTutorAI.Application.Contracts.Memory;
using MyTutorAI.Application.Memory;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the memory group <c>/api/memory</c>; every endpoint requires an access token and returns only the data of the logged-in user. The group has no rate limiting.
/// PL: Endpointy Minimal API grupy pamięci <c>/api/memory</c>; każdy endpoint wymaga access tokenu i zwraca wyłącznie dane zalogowanego użytkownika. Grupa nie ma rate limitingu.
/// </summary>
public static class MemoryEndpoints
{
    /// <summary>
    /// EN: Maps <c>GET /api/memory</c>, which returns a read-only snapshot of the learner's memory layers as <see cref="MemorySnapshotResponse"/>, and <c>GET /api/memory/prompt-preview</c>, which renders the tutor system prompt without calling the LLM and returns it as <see cref="PromptPreviewResponse"/>; both return 200, or 401 without a valid access token.
    /// PL: Mapuje <c>GET /api/memory</c>, który zwraca migawkę warstw pamięci ucznia (tylko odczyt) jako <see cref="MemorySnapshotResponse"/>, oraz <c>GET /api/memory/prompt-preview</c>, który renderuje prompt systemowy tutora bez wywołania LLM i zwraca go jako <see cref="PromptPreviewResponse"/>; oba zwracają 200 albo 401 bez ważnego access tokenu.
    /// </summary>
    /// <param name="endpoints">
    /// EN: Route builder the endpoints are added to.
    /// PL: Builder tras, do którego dodawane są endpointy.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="endpoints"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="endpoints"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IEndpointRouteBuilder MapMemoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/memory")
            .RequireAuthorization();

        group.MapGet("/", GetSnapshotAsync)
            .WithName("Memory_GetSnapshot")
            .WithSummary("Returns a dump of all memory layers for the logged-in student")
            .WithTags("Memory")
            .Produces<MemorySnapshotResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/prompt-preview", GetPromptPreviewAsync)
            .WithName("Memory_GetPromptPreview")
            .WithSummary("Renders (without invoking an LLM) a preview of the \"tutor-system\" system prompt")
            .WithTags("Memory")
            .Produces<PromptPreviewResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static Task<MemorySnapshotResponse> GetSnapshotAsync(
        GetMemorySnapshotHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(cancellationToken);

    private static Task<PromptPreviewResponse> GetPromptPreviewAsync(
        string? topic,
        SessionPhase? phase,
        GetPromptPreviewHandler handler,
        CancellationToken cancellationToken)
        => handler.HandleAsync(topic, phase, cancellationToken);

}
