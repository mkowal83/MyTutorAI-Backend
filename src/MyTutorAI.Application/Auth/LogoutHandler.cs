using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Auth;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Logout use case: revokes the refresh token of one device or all refresh tokens of the current user. Idempotent: an unknown, expired, already revoked or another user's token is a silent success with no change; registered as scoped.
/// PL: Przypadek użycia wylogowania: unieważnia refresh token jednego urządzenia albo wszystkie refresh tokeny bieżącego użytkownika. Idempotentny: nieznany, wygasły, już unieważniony lub cudzy token to cichy sukces bez zmian; rejestrowany jako scoped.
/// </summary>
public sealed class LogoutHandler
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    /// <summary>
    /// EN: Creates the handler with its ports.
    /// PL: Tworzy handler z jego portami.
    /// </summary>
    /// <param name="refreshTokens">
    /// EN: Repository of stored refresh tokens.
    /// PL: Repozytorium zapisanych refresh tokenów.
    /// </param>
    /// <param name="currentUser">
    /// EN: Identity of the authenticated user; logout affects only this user's tokens.
    /// PL: Tożsamość uwierzytelnionego użytkownika; wylogowanie dotyczy tylko jego tokenów.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the current time used for the activity check and the revocation timestamp.
    /// PL: Źródło bieżącego czasu używane do sprawdzenia aktywności i jako czas unieważnienia.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any of the arguments is null.
    /// PL: Którykolwiek z argumentów jest null.
    /// </exception>
    public LogoutHandler(
        IRefreshTokenRepository refreshTokens,
        ICurrentUser currentUser,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(refreshTokens);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(clock);

        _refreshTokens = refreshTokens;
        _currentUser = currentUser;
        _clock = clock;
    }

    /// <summary>
    /// EN: Logs out the current user from one device or from all devices.
    /// PL: Wylogowuje bieżącego użytkownika z jednego urządzenia albo ze wszystkich urządzeń.
    /// </summary>
    /// <param name="request">
    /// EN: With <c>AllDevices</c> set, all tokens of the current user are revoked; otherwise only the matching active token of the current user is revoked.
    /// PL: Przy ustawionym <c>AllDevices</c> unieważniane są wszystkie tokeny bieżącego użytkownika; w przeciwnym razie tylko pasujący aktywny token bieżącego użytkownika.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <returns>
    /// EN: Task completed after the revocation is saved, or immediately when there is nothing to revoke.
    /// PL: Task kończący się po zapisaniu unieważnienia albo od razu, gdy nie ma czego unieważnić.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    public async Task HandleAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = _clock.UtcNow;
        var userId = _currentUser.UserId;

        if (request.AllDevices)
        {
            await _refreshTokens.RevokeAllForUserAsync(userId, now, cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var presentedHash = RefreshTokenHasher.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(presentedHash, cancellationToken);

        if (stored is null || stored.UserId != userId || !stored.IsActive(now))
        {
            return;
        }

        stored.Revoke(now);
        await _refreshTokens.SaveAsync(stored, cancellationToken);
    }
}