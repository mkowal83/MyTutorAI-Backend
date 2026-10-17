using Microsoft.Extensions.Options;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Auth;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Options;
using MyTutorAI.Application.Users;
using MyTutorAI.Domain.Entities;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Use case exchanging a refresh token for a new token pair; the presented token is revoked on every use (rotation). Presenting an already revoked token is treated as account takeover and revokes all of the user's refresh tokens. Registered as scoped.
/// PL: Przypadek użycia wymieniający refresh token na nową parę tokenów; przedstawiony token jest unieważniany przy każdym użyciu (rotacja). Przedstawienie już unieważnionego tokenu jest traktowane jako przejęcie konta i unieważnia wszystkie refresh tokeny użytkownika. Rejestrowany jako scoped.
/// </summary>
public sealed class RefreshTokenHandler
{
    private const string InvalidTokenMessage = "Invalid or expired refresh token.";

    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClock _clock;
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// EN: Creates the handler.
    /// PL: Tworzy handler.
    /// </summary>
    /// <param name="refreshTokens">
    /// EN: Store of issued refresh tokens (hashes only), used to look up, revoke and add tokens.
    /// PL: Magazyn wydanych refresh tokenów (wyłącznie hashe), używany do wyszukania, unieważnienia i dodania tokenów.
    /// </param>
    /// <param name="users">
    /// EN: Account store used to load the token owner.
    /// PL: Magazyn kont używany do wczytania właściciela tokenu.
    /// </param>
    /// <param name="jwtTokenService">
    /// EN: Issues the access token and generates the new refresh token.
    /// PL: Wydaje access token i generuje nowy refresh token.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the current time used for the expiration check, revocation and new expiration timestamps.
    /// PL: Źródło bieżącego czasu używane do sprawdzenia wygaśnięcia, unieważnienia i nowych czasów wygaśnięcia.
    /// </param>
    /// <param name="jwtOptions">
    /// EN: Token lifetimes; the value is read once at construction.
    /// PL: Czasy życia tokenów; wartość jest odczytywana raz przy konstrukcji.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IJwtTokenService jwtTokenService,
        IClock clock,
        IOptions<JwtOptions> jwtOptions)
    {
        ArgumentNullException.ThrowIfNull(refreshTokens);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(jwtOptions);

        _refreshTokens = refreshTokens;
        _users = users;
        _jwtTokenService = jwtTokenService;
        _clock = clock;
        _jwtOptions = jwtOptions.Value;
    }


    /// <summary>
    /// EN: Validates the presented refresh token, revokes it with a link to its successor and issues a new token pair; only the SHA-256 hash of the new refresh token is stored. Every rejection uses the same message, so the caller cannot tell why the token was refused.
    /// PL: Weryfikuje przedstawiony refresh token, unieważnia go z powiązaniem do następcy i wydaje nową parę tokenów; w bazie zapisywany jest wyłącznie hash SHA-256 nowego refresh tokenu. Każde odrzucenie ma ten sam komunikat, więc wywołujący nie może ustalić, dlaczego token odrzucono.
    /// </summary>
    /// <param name="request">
    /// EN: Request with the plaintext refresh token; must not be null.
    /// PL: Żądanie z refresh tokenem w postaci jawnej; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <returns>
    /// EN: The token owner's data with the new access token, the new refresh token and their expiration times.
    /// PL: Dane właściciela tokenu z nowym access tokenem, nowym refresh tokenem i czasami ich wygaśnięcia.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="UnauthorizedException">
    /// EN: The token is empty, unknown, expired, already revoked (all of the user's tokens are revoked first) or its owner no longer exists; the API maps it to 401.
    /// PL: Token jest pusty, nieznany, wygasły, już unieważniony (najpierw unieważniane są wszystkie tokeny użytkownika) lub jego właściciel już nie istnieje; API mapuje to na 401.
    /// </exception>
    public async Task<AuthResponse> HandleAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedException(InvalidTokenMessage);
        }

        var presentedHash = RefreshTokenHasher.Hash(request.RefreshToken);

        var stored = await _refreshTokens.GetByTokenHashAsync(presentedHash, cancellationToken)
            ?? throw new UnauthorizedException(InvalidTokenMessage);

        var now = _clock.UtcNow;

        // EN: An already revoked token (rotated or logged out) is presented -> treat it as a compromise of the whole chain and revoke all of the user's active tokens.
        // PL: Przedstawiono już unieważniony token (zrotowany lub wylogowany) -> traktuj to jako przejęcie całego łańcucha i unieważnij wszystkie aktywne tokeny użytkownika.
        if (stored.RevokedAt is not null)
        {
            await _refreshTokens.RevokeAllForUserAsync(stored.UserId, now, cancellationToken);
            throw new UnauthorizedException(InvalidTokenMessage);
        }

        if (stored.ExpiresAt <= now)
        {
            throw new UnauthorizedException(InvalidTokenMessage);
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new UnauthorizedException(InvalidTokenMessage);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var accessTokenExpiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = RefreshTokenHasher.Hash(newRefreshToken);
        var refreshTokenExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);

        stored.Revoke(now, replacedByTokenHash: newRefreshTokenHash);
        await _refreshTokens.SaveAsync(stored, cancellationToken);

        var rotated = new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: newRefreshTokenHash,
            expiresAt: refreshTokenExpiresAt,
            createdAt: now);

        await _refreshTokens.AddAsync(rotated, cancellationToken);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            AccessToken: accessToken,
            AccessTokenExpiresAt: accessTokenExpiresAt,
            RefreshToken: newRefreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt);
    }
}

