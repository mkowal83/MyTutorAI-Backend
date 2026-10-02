namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown by the user repository when the store rejects an account because its e-mail is already registered.
/// PL: Zgłaszany przez repozytorium użytkowników, gdy magazyn odrzuca konto, bo jego e-mail jest już zarejestrowany.
/// </summary>
/// <remarks>
/// EN: The message deliberately omits the e-mail address, so it cannot leak into logs.
/// PL: Komunikat celowo nie zawiera adresu e-mail, aby nie trafił do logów.
/// </remarks>
public sealed class DuplicateEmailException : Exception
{
    /// <summary>
    /// EN: Creates the exception with the store error as the root cause.
    /// PL: Tworzy wyjątek z błędem magazynu jako przyczyną źródłową.
    /// </summary>
    /// <param name="innerException">
    /// EN: Error reported by the store, e.g. a unique index violation.
    /// PL: Błąd zgłoszony przez magazyn, np. naruszenie unikalnego indeksu.
    /// </param>
    public DuplicateEmailException(Exception innerException)
        : base("An account with this email address already exists.", innerException)
    {
    }
}
