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
/// EN: Use case logging in an existing account with e-mail and password and issuing a pair of tokens (short-lived access token + long-lived refresh token). An unknown e-mail and a wrong password end with the same error, so the caller cannot tell which e-mails have accounts.
/// PL: Przypadek użycia logujący istniejące konto e-mailem i hasłem oraz wydający parę tokenów (krótkożyjący access token + długożyjący refresh token). Nieznany e-mail i błędne hasło kończą się tym samym błędem, więc wywołujący nie może ustalić, które e-maile mają konta.
/// </summary>
public sealed class LoginHandler
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    // Hash of a random password, computed once per process with the configured hasher; never matches a real password.
    private static string? _dummyPasswordHash;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IClock _clock;
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// EN: Creates the handler.
    /// PL: Tworzy handler.
    /// </summary>
    /// <param name="users">
    /// EN: Account store used to find the user by normalized e-mail.
    /// PL: Magazyn kont używany do wyszukania użytkownika po znormalizowanym e-mailu.
    /// </param>
    /// <param name="passwordHasher">
    /// EN: Verifies the provided password against the stored hash.
    /// PL: Weryfikuje podane hasło względem zapisanego hasha.
    /// </param>
    /// <param name="jwtTokenService">
    /// EN: Issues the access token and generates the refresh token.
    /// PL: Wydaje access token i generuje refresh token.
    /// </param>
    /// <param name="refreshTokens">
    /// EN: Store of issued refresh tokens (hashes only).
    /// PL: Magazyn wydanych refresh tokenów (wyłącznie hashe).
    /// </param>
    /// <param name="clock">
    /// EN: Source of the issuance time used to compute expiration timestamps.
    /// PL: Źródło czasu wydania, na podstawie którego liczone są czasy wygaśnięcia.
    /// </param>
    /// <param name="jwtOptions">
    /// EN: Token lifetimes; the value is read once at construction.
    /// PL: Czasy życia tokenów; wartość jest odczytywana raz przy konstrukcji.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public LoginHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokens,
        IClock clock,
        IOptions<JwtOptions> jwtOptions)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        ArgumentNullException.ThrowIfNull(refreshTokens);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(jwtOptions);

        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokens = refreshTokens;
        _clock = clock;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// EN: Verifies the credentials, issues a new token pair and stores only the SHA-256 hash of the refresh token in the database. Each successful login adds a new refresh token without revoking earlier ones, so several devices can stay logged in. For an unknown e-mail the password is still verified against a dummy hash, so response time does not reveal whether the account exists.
    /// PL: Weryfikuje dane logowania, wydaje nową parę tokenów i zapisuje w bazie wyłącznie hash SHA-256 refresh tokenu. Każde udane logowanie dodaje nowy refresh token bez unieważniania wcześniejszych, więc kilka urządzeń może pozostać zalogowanych. Dla nieznanego e-maila hasło jest i tak weryfikowane względem fikcyjnego hasha, więc czas odpowiedzi nie zdradza, czy konto istnieje.
    /// </summary>
    /// <param name="request">
    /// EN: Validated login request; must not be null. The e-mail is trimmed and lowercased before lookup.
    /// PL: Zwalidowane żądanie logowania; nie może być null. E-mail jest przycinany i zamieniany na małe litery przed wyszukaniem.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <returns>
    /// EN: Account data with the access token, the plaintext refresh token and their expiration timestamps (UTC).
    /// PL: Dane konta z access tokenem, refresh tokenem w postaci jawnej oraz ich czasami wygaśnięcia (UTC).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="UnauthorizedException">
    /// EN: The e-mail is unknown or the password does not match; the message is the same in both cases.
    /// PL: E-mail jest nieznany lub hasło nie pasuje; komunikat jest taki sam w obu przypadkach.
    /// </exception>
    public async Task<AuthResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            // Verify against a dummy hash so an unknown e-mail costs the same time as a wrong password.
            _passwordHasher.Verify(GetDummyPasswordHash(), request.Password);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var verification = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var now = _clock.UtcNow;

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var accessTokenExpiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);

        var storedToken = new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: RefreshTokenHasher.Hash(refreshToken),
            expiresAt: refreshTokenExpiresAt,
            createdAt: now);

        await _refreshTokens.AddAsync(storedToken, cancellationToken);

        return new AuthResponse(
            UserId: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            AccessToken: accessToken,
            AccessTokenExpiresAt: accessTokenExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt);
    }

    // A concurrent first call may compute the hash twice; either result is valid, so no lock is needed.
    private string GetDummyPasswordHash() =>
        _dummyPasswordHash ??= _passwordHasher.Hash(Guid.NewGuid().ToString("N"));
}
