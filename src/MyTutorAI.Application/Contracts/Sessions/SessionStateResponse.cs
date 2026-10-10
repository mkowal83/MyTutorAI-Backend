using MyTutorAI.Application.Contracts.Sessions.Dtos;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Contracts.Sessions;


/// <summary>
/// EN: Response DTO with the state of an existing session and its full Working memory turn history.
/// PL: DTO odpowiedzi ze stanem istniejącej sesji i pełną historią tur z pamięci Working.
/// </summary>
/// <param name="SessionId">
/// EN: Identifier of the session.
/// PL: Identyfikator sesji.
/// </param>
/// <param name="Topic">
/// EN: Trimmed session topic.
/// PL: Przycięty temat sesji.
/// </param>
/// <param name="Phase">
/// EN: Current conversation phase.
/// PL: Bieżąca faza rozmowy.
/// </param>
/// <param name="Status">
/// EN: Lifecycle status: active or closed.
/// PL: Status cyklu życia: aktywna lub zamknięta.
/// </param>
/// <param name="StartedAt">
/// EN: UTC moment the session was started.
/// PL: Moment rozpoczęcia sesji w UTC.
/// </param>
/// <param name="WorkingMemoryCount">
/// EN: Number of turns in the Working memory; <c>0</c> when the entry does not exist or has expired.
/// PL: Liczba tur w pamięci Working; <c>0</c>, gdy wpis nie istnieje lub wygasł.
/// </param>
/// <param name="Messages">
/// EN: All turns of the Working memory in insertion order; empty when the entry does not exist or has expired.
/// PL: Wszystkie tury pamięci Working w kolejności dodania; pusta, gdy wpis nie istnieje lub wygasł.
/// </param>
public sealed record SessionStateResponse(
    Guid SessionId,
    string Topic,
    SessionPhase Phase,
    SessionStatus Status,
    DateTimeOffset StartedAt,
    int WorkingMemoryCount,
    IReadOnlyList<ChatTurnDto> Messages);