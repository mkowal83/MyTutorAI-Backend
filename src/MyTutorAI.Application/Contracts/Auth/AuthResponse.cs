namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Response DTO of <c>POST /api/auth/login</c> carrying the account data and a fresh token pair.
/// PL: DTO odpowiedzi <c>POST /api/auth/login</c> z danymi konta i nową parą tokenów.
/// </summary>
/// <param name="UserId">
/// EN: Account identifier (the <c>sub</c> claim of the access token).
/// PL: Identyfikator konta (claim <c>sub</c> access tokenu).
/// </param>
/// <param name="Email">
/// EN: Normalized (trimmed, lowercase) e-mail of the account.
/// PL: Znormalizowany (przycięty, małymi literami) e-mail konta.
/// </param>
/// <param name="DisplayName">
/// EN: Name shown to the student.
/// PL: Nazwa pokazywana uczniowi.
/// </param>
/// <param name="AccessToken">
/// EN: Short-lived JWT to send in the <c>Authorization: Bearer</c> header.
/// PL: Krótkożyjący JWT do wysyłania w nagłówku <c>Authorization: Bearer</c>.
/// </param>
/// <param name="AccessTokenExpiresAt">
/// EN: UTC expiration time of <paramref name="AccessToken"/>.
/// PL: Czas wygaśnięcia <paramref name="AccessToken"/> w UTC.
/// </param>
/// <param name="RefreshToken">
/// EN: Long-lived refresh token in plaintext; returned only here, the server stores just its hash.
/// PL: Długożyjący refresh token w postaci jawnej; zwracany tylko tutaj, serwer przechowuje wyłącznie jego hash.
/// </param>
/// <param name="RefreshTokenExpiresAt">
/// EN: UTC expiration time of <paramref name="RefreshToken"/>.
/// PL: Czas wygaśnięcia <paramref name="RefreshToken"/> w UTC.
/// </param>
public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

