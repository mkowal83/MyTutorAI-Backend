namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown by a handler when the requested session does not exist or belongs to another user; the API maps it to 404, the same as a missing resource.
/// PL: Zgłaszany przez handler, gdy żądana sesja nie istnieje lub należy do innego użytkownika; API mapuje go na 404, tak samo jak brak zasobu.
/// </summary>
public sealed class SessionNotFoundException : Exception
{
    /// <summary>
    /// EN: Creates the exception with a message naming the session and the user.
    /// PL: Tworzy wyjątek z komunikatem wskazującym sesję i użytkownika.
    /// </summary>
    /// <param name="userId">
    /// EN: User in whose context the lookup was performed.
    /// PL: Użytkownik, w którego kontekście wykonano wyszukiwanie.
    /// </param>
    /// <param name="sessionId">
    /// EN: Identifier of the requested session.
    /// PL: Identyfikator żądanej sesji.
    /// </param>
    public SessionNotFoundException(Guid userId, Guid sessionId)
        : base($"Session {sessionId} does not exist for user {userId}.")
    {
        UserId = userId;
        SessionId = sessionId;
    }

    /// <summary>
    /// EN: Identifier of the user in whose context the lookup was performed.
    /// PL: Identyfikator użytkownika, w którego kontekście wykonano wyszukiwanie.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// EN: Identifier of the requested session.
    /// PL: Identyfikator żądanej sesji.
    /// </summary>
    public Guid SessionId { get; }
}
