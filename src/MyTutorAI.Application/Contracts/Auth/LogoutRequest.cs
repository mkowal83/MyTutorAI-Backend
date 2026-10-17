namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Input of the logout; body of <c>POST /api/auth/logout</c>. Validated by <see cref="LogoutRequestValidator"/>.
/// PL: Wejście wylogowania; body <c>POST /api/auth/logout</c>. Walidowane przez <see cref="LogoutRequestValidator"/>.
/// </summary>
/// <param name="RefreshToken">
/// EN: Plaintext refresh token of the device being logged out; required when <paramref name="AllDevices"/> is <c>false</c>, ignored otherwise.
/// PL: Refresh token wylogowywanego urządzenia w postaci jawnej; wymagany, gdy <paramref name="AllDevices"/> ma wartość <c>false</c>, w przeciwnym razie ignorowany.
/// </param>
/// <param name="AllDevices">
/// EN: <c>true</c> revokes all refresh tokens of the current user (logout from all devices); defaults to <c>false</c>.
/// PL: <c>true</c> unieważnia wszystkie refresh tokeny bieżącego użytkownika (wylogowanie ze wszystkich urządzeń); domyślnie <c>false</c>.
/// </param>
public sealed record LogoutRequest(string? RefreshToken, bool AllDevices = false);