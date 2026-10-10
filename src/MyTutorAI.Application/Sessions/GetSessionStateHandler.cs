using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Contracts.Sessions.Dtos;
using MyTutorAI.Application.Exceptions;
using System.Data;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case that returns the state of a session of the current user, loaded through <see cref="ISessionRepository"/>, together with its full turn history from the Working memory, loaded through <see cref="IWorkingMemoryStore"/>. A session of another user is treated as non-existent.
/// PL: Przypadek użycia zwracający stan sesji bieżącego użytkownika, wczytany przez <see cref="ISessionRepository"/>, razem z pełną historią tur z pamięci Working, wczytaną przez <see cref="IWorkingMemoryStore"/>. Sesja innego użytkownika jest traktowana jak nieistniejąca.
/// </summary>
public sealed class GetSessionStateHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly ISessionRepository _sessions;
    private readonly IWorkingMemoryStore _workingMemory;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="sessions">
    /// EN: Store from which the session is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywana jest sesja. Nie może być null.
    /// </param>
    /// <param name="workingMemory">
    /// EN: Store of the session's Working memory (Redis), from which the turn history is read. Must not be null.
    /// PL: Magazyn pamięci Working sesji (Redis), z którego czytana jest historia tur. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetSessionStateHandler(
        ICurrentUser currentUser,
        ISessionRepository sessions,
        IWorkingMemoryStore workingMemory)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(workingMemory);

        _currentUser = currentUser;
        _sessions = sessions;
        _workingMemory = workingMemory;
    }

    /// <summary>
    /// EN: Returns the state and the full turn history of the given session of the current user.
    /// PL: Zwraca stan i pełną historię tur wskazanej sesji bieżącego użytkownika.
    /// </summary>
    /// <param name="sessionId">
    /// EN: Identifier of the session to read.
    /// PL: Identyfikator sesji do odczytania.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels reading the session from the database and the turn history from the Working memory.
    /// PL: Anuluje odczyt sesji z bazy danych i historii tur z pamięci Working.
    /// </param>
    /// <returns>
    /// EN: Session identifier, topic, phase, status, start time, turn count and turns in insertion order; the count is <c>0</c> and the list empty when the Working memory entry does not exist or has expired.
    /// PL: Identyfikator sesji, temat, faza, status, czas rozpoczęcia, liczba tur i tury w kolejności dodania; liczba wynosi <c>0</c>, a lista jest pusta, gdy wpis pamięci Working nie istnieje lub wygasł.
    /// </returns>
    /// <exception cref="SessionNotFoundException">
    /// EN: The session does not exist or belongs to another user.
    /// PL: Sesja nie istnieje lub należy do innego użytkownika.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: The current request has no authenticated user id.
    /// PL: Bieżące żądanie nie ma identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    public async Task<SessionStateResponse> HandleAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var session = await _sessions.GetAsync(userId, sessionId, cancellationToken) ?? throw new SessionNotFoundException(userId, sessionId);

        var working = await _workingMemory.GetAsync(userId, sessionId, cancellationToken);
        var messages = working.History
            .Select(turn => new ChatTurnDto(turn.Role, turn.Content))
            .ToList();

        return new SessionStateResponse(
            session.Id,
            session.Topic,
            session.Phase,
            session.Status,
            session.StartedAt,
            working.Count,
            messages);
    }
}