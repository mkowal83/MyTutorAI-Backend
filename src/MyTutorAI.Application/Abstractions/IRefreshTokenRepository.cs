using MyTutorAI.Domain.Entities;

namespace MyTutorAI.Application.Abstractions;

/// <summary>
/// EN: Port for persisting issued refresh tokens (SQL Server); implemented in Infrastructure and registered as scoped. Tokens are stored and looked up only by hash.
/// PL: Port utrwalania wydanych refresh tokenów (SQL Server); implementowany w Infrastructure i rejestrowany jako scoped. Tokeny są zapisywane i wyszukiwane wyłącznie po hashu.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// EN: Finds a token by its hash, regardless of whether it is active, revoked or expired.
    /// PL: Wyszukuje token po hashu, niezależnie od tego, czy jest aktywny, unieważniony czy wygasły.
    /// </summary>
    /// <param name="tokenHash">
    /// EN: Hash computed by <c>RefreshTokenHasher.Hash</c>.
    /// PL: Hash wyliczony przez <c>RefreshTokenHasher.Hash</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database query.
    /// PL: Anuluje zapytanie do bazy danych.
    /// </param>
    /// <returns>
    /// EN: The token, or <c>null</c> if no token has this hash.
    /// PL: Token albo <c>null</c>, jeśli żaden token nie ma tego hasha.
    /// </returns>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Stores a newly issued token and saves it immediately.
    /// PL: Zapisuje nowo wydany token i od razu go utrwala.
    /// </summary>
    /// <param name="token">
    /// EN: New token; must not be null.
    /// PL: Nowy token; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database write.
    /// PL: Anuluje zapis do bazy danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="token"/> is null.
    /// PL: <paramref name="token"/> jest null.
    /// </exception>
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Persists changes to an existing token, e.g. after revocation or rotation, and saves them immediately.
    /// PL: Utrwala zmiany istniejącego tokenu, np. po unieważnieniu lub rotacji, i od razu je zapisuje.
    /// </summary>
    /// <param name="token">
    /// EN: Modified token; must not be null.
    /// PL: Zmodyfikowany token; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database write.
    /// PL: Anuluje zapis do bazy danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="token"/> is null.
    /// PL: <paramref name="token"/> jest null.
    /// </exception>
    Task SaveAsync(RefreshToken token, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Revokes every not yet revoked token of the user, including expired ones, in one save; intended for logout from all devices.
    /// PL: Unieważnia każdy jeszcze nieunieważniony token użytkownika, także wygasły, w jednym zapisie; przeznaczone do wylogowania ze wszystkich urządzeń.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the tokens; an unknown user means no change.
    /// PL: Właściciel tokenów; nieznany użytkownik oznacza brak zmian.
    /// </param>
    /// <param name="revokedAt">
    /// EN: Revocation timestamp written to each token.
    /// PL: Czas unieważnienia zapisywany w każdym tokenie.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    Task RevokeAllForUserAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken);
}

