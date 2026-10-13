namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown by a handler when the request conflicts with the current state of the resource, e.g. a message sent to a closed session. The API maps it to 409 and does not expose the message to the client.
/// PL: Zgłaszany przez handler, gdy żądanie jest sprzeczne z bieżącym stanem zasobu, np. wiadomość wysłana do zamkniętej sesji. API mapuje go na 409 i nie ujawnia komunikatu klientowi.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>
    /// EN: Creates the exception.
    /// PL: Tworzy wyjątek.
    /// </summary>
    /// <param name="message">
    /// EN: Description of the conflict for logs.
    /// PL: Opis konfliktu na potrzeby logów.
    /// </param>
    public ConflictException(string message)
        : base(message)
    {
    }
}