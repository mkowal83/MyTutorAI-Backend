namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Input of the token refresh; body of <c>POST /api/auth/refresh</c>. Validated by <see cref="RefreshRequestValidator"/>.
/// PL: Wejście odświeżenia tokenów; body <c>POST /api/auth/refresh</c>. Walidowane przez <see cref="RefreshRequestValidator"/>.
/// </summary>
/// <param name="RefreshToken">
/// EN: Plaintext refresh token issued by the last login or refresh; usable only once.
/// PL: Refresh token w postaci jawnej, wydany przy ostatnim logowaniu lub odświeżeniu; można go użyć tylko raz.
/// </param>
public sealed record RefreshRequest(string RefreshToken);
