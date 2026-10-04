namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Authentication failure in a use case (e.g. invalid login credentials); the Api maps it to HTTP 401.
/// PL: Niepowodzenie uwierzytelnienia w przypadku użycia (np. błędne dane logowania); Api mapuje go na HTTP 401.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    /// <summary>
    /// EN: Creates the exception with a reason for the failure.
    /// PL: Tworzy wyjątek z przyczyną niepowodzenia.
    /// </summary>
    /// <param name="message">
    /// EN: Reason returned to the client as the problem <c>detail</c>; must not reveal whether the account exists.
    /// PL: Przyczyna zwracana klientowi jako <c>detail</c> problemu; nie może zdradzać, czy konto istnieje.
    /// </param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
