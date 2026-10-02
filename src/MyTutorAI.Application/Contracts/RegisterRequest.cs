namespace MyTutorAI.Application.Contracts;

/// <summary>
/// EN: Input of the account registration; body of <c>POST /api/auth/register</c>. Validated by <see cref="Auth.RegisterRequestValidator"/>.
/// PL: Dane wejściowe rejestracji konta; body <c>POST /api/auth/register</c>. Walidowane przez <see cref="Auth.RegisterRequestValidator"/>.
/// </summary>
/// <param name="Email">
/// EN: E-mail address; normalized (trim + lowercase) before the uniqueness check and storage.
/// PL: Adres e-mail; normalizowany (trim + małe litery) przed sprawdzeniem unikalności i zapisem.
/// </param>
/// <param name="Password">
/// EN: Plaintext password; only its hash is stored and it is never logged.
/// PL: Hasło w postaci jawnej; zapisywany jest wyłącznie jego hash i nigdy nie trafia do logów.
/// </param>
/// <param name="DisplayName">
/// EN: Name shown to the student; trimmed before storage.
/// PL: Nazwa pokazywana uczniowi; przycinana przed zapisem.
/// </param>
public sealed record RegisterRequest(string Email, string Password, string DisplayName)
{
    /// <summary>
    /// EN: Hides the password and e-mail so the record never leaks them through <c>ToString</c> (e.g. in logs).
    /// PL: Ukrywa hasło i e-mail, aby rekord nigdy nie ujawnił ich przez <c>ToString</c> (np. w logach).
    /// </summary>
    /// <returns>
    /// EN: Textual representation without sensitive data.
    /// PL: Reprezentacja tekstowa bez danych wrażliwych.
    /// </returns>
    public override string ToString() => $"{nameof(RegisterRequest)} {{ *** }}";
}
