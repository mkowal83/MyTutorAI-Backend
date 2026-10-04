namespace MyTutorAI.Domain.Entities;

/// <summary>
/// EN: Issued refresh token of a user, stored only as a hash. It is active until revoked (logout or rotation) or expired.
/// PL: Wydany refresh token użytkownika, przechowywany wyłącznie jako hash. Jest aktywny, dopóki nie zostanie unieważniony (wylogowanie lub rotacja) albo nie wygaśnie.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// EN: Creates a new, not revoked refresh token.
    /// PL: Tworzy nowy, nieunieważniony refresh token.
    /// </summary>
    /// <param name="id">
    /// EN: Token identifier; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator tokenu; nie może być <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="userId">
    /// EN: Identifier of the token owner; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator właściciela tokenu; nie może być <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="tokenHash">
    /// EN: Hash of the plaintext token computed by the caller; must not be null, empty or whitespace.
    /// PL: Hash tokenu w postaci jawnej wyliczony przez wywołującego; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="expiresAt">
    /// EN: Expiration timestamp.
    /// PL: Czas wygaśnięcia.
    /// </param>
    /// <param name="createdAt">
    /// EN: Issuance timestamp.
    /// PL: Czas wydania.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="id"/> or <paramref name="userId"/> is empty, or <paramref name="tokenHash"/> is empty or whitespace (<see cref="ArgumentNullException"/> for null).
    /// PL: <paramref name="id"/> lub <paramref name="userId"/> jest pusty albo <paramref name="tokenHash"/> jest pusty lub składa się z samych białych znaków (<see cref="ArgumentNullException"/> dla null).
    /// </exception>
    public RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Refresh token id cannot be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// EN: Token identifier.
    /// PL: Identyfikator tokenu.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// EN: Identifier of the token owner.
    /// PL: Identyfikator właściciela tokenu.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// EN: Hash of the plaintext token; the plaintext is never stored.
    /// PL: Hash tokenu w postaci jawnej; postać jawna nigdy nie jest zapisywana.
    /// </summary>
    public string TokenHash { get; }

    /// <summary>
    /// EN: Expiration timestamp.
    /// PL: Czas wygaśnięcia.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>
    /// EN: Issuance timestamp.
    /// PL: Czas wydania.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// EN: Revocation timestamp, or <c>null</c> if the token has not been revoked.
    /// PL: Czas unieważnienia albo <c>null</c>, jeśli token nie został unieważniony.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// EN: Hash of the token that replaced this one during rotation, or <c>null</c>.
    /// PL: Hash tokenu, który zastąpił ten token podczas rotacji, albo <c>null</c>.
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// EN: Revokes the token on rotation or logout. Calling it again overwrites the previous revocation data.
    /// PL: Unieważnia token przy rotacji lub wylogowaniu. Ponowne wywołanie nadpisuje poprzednie dane unieważnienia.
    /// </summary>
    /// <param name="revokedAt">
    /// EN: Revocation timestamp.
    /// PL: Czas unieważnienia.
    /// </param>
    /// <param name="replacedByTokenHash">
    /// EN: Hash of the new token on rotation, or <c>null</c> on logout.
    /// PL: Hash nowego tokenu przy rotacji albo <c>null</c> przy wylogowaniu.
    /// </param>
    public void Revoke(DateTimeOffset revokedAt, string? replacedByTokenHash = null)
    {
        RevokedAt = revokedAt;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    /// <summary>
    /// EN: Checks whether the token can still be used at the given moment.
    /// PL: Sprawdza, czy token może być jeszcze użyty w danej chwili.
    /// </summary>
    /// <param name="now">
    /// EN: Moment of the check.
    /// PL: Chwila sprawdzenia.
    /// </param>
    /// <returns>
    /// EN: <c>true</c> when the token is not revoked and <see cref="ExpiresAt"/> is later than <paramref name="now"/>.
    /// PL: <c>true</c>, gdy token nie jest unieważniony, a <see cref="ExpiresAt"/> jest późniejszy niż <paramref name="now"/>.
    /// </returns>
    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
