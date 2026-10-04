namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Input of the login; body of <c>POST /api/auth/login</c>. Validated by <see cref="LoginRequestValidator"/>.
/// PL: Dane wejściowe logowania; body <c>POST /api/auth/login</c>. Walidowane przez <see cref="LoginRequestValidator"/>.
/// </summary>
/// <param name="Email">
/// EN: E-mail address; trimmed and lowercased by the handler before lookup.
/// PL: Adres e-mail; przycinany i zamieniany na małe litery przez handler przed wyszukaniem.
/// </param>
/// <param name="Password">
/// EN: Plaintext password checked against the stored hash.
/// PL: Hasło w postaci jawnej sprawdzane względem zapisanego hasha.
/// </param>
public sealed record LoginRequest(string Email, string Password)
{
    /// <summary>
    /// EN: Hides the password and e-mail so the record never leaks them through <c>ToString</c> (e.g. in logs).
    /// PL: Ukrywa hasło i e-mail, aby rekord nigdy nie ujawnił ich przez <c>ToString</c> (np. w logach).
    /// </summary>
    /// <returns>
    /// EN: Textual representation without sensitive data.
    /// PL: Reprezentacja tekstowa bez danych wrażliwych.
    /// </returns>
    public override string ToString() => $"{nameof(LoginRequest)} {{ *** }}";
}