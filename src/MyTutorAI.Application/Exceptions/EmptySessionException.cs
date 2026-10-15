namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown when closing a session whose Working memory has no turns, because a session without any exchange cannot be closed. The API maps it to 409 and does not expose the message to the client.
/// PL: Zgłaszany przy zamykaniu sesji, której pamięć Working nie ma żadnych tur, bo sesji bez żadnej wymiany nie można zamknąć. API mapuje go na 409 i nie ujawnia komunikatu klientowi.
/// </summary>
public sealed class EmptySessionException : Exception
{
    /// <summary>
    /// EN: Creates the exception for the given session.
    /// PL: Tworzy wyjątek dla podanej sesji.
    /// </summary>
    /// <param name="sessionId">
    /// EN: Identifier of the session that cannot be closed; included in the message for logs.
    /// PL: Identyfikator sesji, której nie można zamknąć; dołączany do komunikatu na potrzeby logów.
    /// </param>
    public EmptySessionException(Guid sessionId)
        : base($"Session {sessionId} cannot be closed — WorkingMemory is empty.")
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// EN: Identifier of the session that cannot be closed.
    /// PL: Identyfikator sesji, której nie można zamknąć.
    /// </summary>
    public Guid SessionId { get; }
}