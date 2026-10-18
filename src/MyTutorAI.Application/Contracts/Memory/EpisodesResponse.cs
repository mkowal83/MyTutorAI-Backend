using MyTutorAI.Application.Contracts.Sessions.Dtos;

namespace MyTutorAI.Application.Contracts.Memory;

/// <summary>
/// EN: Response DTO of <c>GET /api/memory/episodes</c>: all retained episodes of the logged-in learner's closed sessions.
/// PL: DTO odpowiedzi <c>GET /api/memory/episodes</c>: wszystkie przechowywane epizody zamkniętych sesji zalogowanego ucznia.
/// </summary>
/// <param name="Episodes">
/// EN: Retained episodes ordered from newest to oldest; empty when there are none.
/// PL: Przechowywane epizody uporządkowane od najnowszego do najstarszego; pusta lista, gdy ich nie ma.
/// </param>
/// <param name="Count">
/// EN: Number of items in <paramref name="Episodes"/>.
/// PL: Liczba elementów w <paramref name="Episodes"/>.
/// </param>
public sealed record EpisodesResponse(
    IReadOnlyList<EpisodeDto> Episodes,
    int Count);
