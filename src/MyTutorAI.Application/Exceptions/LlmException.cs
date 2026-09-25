namespace MyTutorAI.Application.Exceptions;


/// <summary>
/// EN: Thrown when the LLM fails to return a valid structured output response after all repair-loop attempts are exhausted.
/// PL: Zgłaszany, gdy LLM nie zwraca poprawnej odpowiedzi structured output po wyczerpaniu wszystkich prób pętli naprawczej.
/// </summary>
/// <remarks>
/// EN: The Api maps it to HTTP 502 ProblemDetails, not 500, so clients can tell an external LLM failure apart from a backend error. Handlers propagate it before any persistence write, so the stored state stays unchanged.
/// PL: Api mapuje go na HTTP 502 ProblemDetails, a nie 500, dzięki czemu klient odróżnia awarię zewnętrznego LLM od błędu backendu. Handlery przepuszczają go przed jakimkolwiek zapisem, więc zapisany stan pozostaje bez zmian.
/// <para>
/// EN: <see cref="Exception.InnerException"/> carries the error of the last attempt; cancellation (<see cref="OperationCanceledException"/>) is never wrapped in this exception.
/// PL: <see cref="Exception.InnerException"/> zawiera błąd ostatniej próby; anulowanie (<see cref="OperationCanceledException"/>) nigdy nie jest opakowywane w ten wyjątek.
/// </para>
/// </remarks>
public sealed class LlmException : Exception
{
    /// <summary>
    /// EN: Creates the exception with a message and no known root cause.
    /// PL: Tworzy wyjątek z komunikatem, bez znanej przyczyny źródłowej.
    /// </summary>
    /// <param name="message">
    /// EN: Description of the LLM communication failure.
    /// PL: Opis niepowodzenia komunikacji z LLM.
    /// </param>
    public LlmException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// EN: Creates the exception with a message and the root cause of the failure.
    /// PL: Tworzy wyjątek z komunikatem i przyczyną źródłową niepowodzenia.
    /// </summary>
    /// <param name="message">
    /// EN: Description of the LLM communication failure.
    /// PL: Opis niepowodzenia komunikacji z LLM.
    /// </param>
    /// <param name="innerException">
    /// EN: Error of the last failed attempt, e.g. <see cref="HttpRequestException"/> or <see cref="System.Text.Json.JsonException"/>.
    /// PL: Błąd ostatniej nieudanej próby, np. <see cref="HttpRequestException"/> lub <see cref="System.Text.Json.JsonException"/>.
    /// </param>
    public LlmException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
