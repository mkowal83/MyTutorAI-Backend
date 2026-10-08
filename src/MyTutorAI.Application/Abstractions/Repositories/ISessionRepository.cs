using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port to the durable store of learning sessions (phase and status), many per user; implemented in Infrastructure.
/// PL: Port do trwałego magazynu sesji nauki (faza i status), wielu na użytkownika; implementowany w Infrastructure.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// EN: Loads the given session of the given user.
    /// PL: Wczytuje wskazaną sesję danego użytkownika.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session; a session of another user is never returned.
    /// PL: Właściciel sesji; sesja innego użytkownika nigdy nie jest zwracana.
    /// </param>
    /// <param name="sessionId">
    /// EN: Identifier of the session to load.
    /// PL: Identyfikator sesji do wczytania.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read.
    /// PL: Anuluje odczyt z bazy danych.
    /// </param>
    /// <returns>
    /// EN: The session, or <c>null</c> when it does not exist or belongs to another user.
    /// PL: Sesja albo <c>null</c>, gdy nie istnieje lub należy do innego użytkownika.
    /// </returns>
    Task<Session?> GetAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Inserts a new session and saves it immediately.
    /// PL: Wstawia nową sesję i od razu ją utrwala.
    /// </summary>
    /// <param name="session">
    /// EN: Session to insert; must not be null.
    /// PL: Sesja do wstawienia; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database write.
    /// PL: Anuluje zapis do bazy danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="session"/> is null.
    /// PL: <paramref name="session"/> jest null.
    /// </exception>
    Task AddAsync(Session session, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Persists changes of an existing session (phase, status) and saves them immediately.
    /// PL: Utrwala zmiany istniejącej sesji (faza, status) i od razu je zapisuje.
    /// </summary>
    /// <param name="session">
    /// EN: Previously stored session with changes to save; must not be null.
    /// PL: Wcześniej zapisana sesja ze zmianami do utrwalenia; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database write.
    /// PL: Anuluje zapis do bazy danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="session"/> is null.
    /// PL: <paramref name="session"/> jest null.
    /// </exception>
    Task SaveAsync(Session session, CancellationToken cancellationToken);
}
