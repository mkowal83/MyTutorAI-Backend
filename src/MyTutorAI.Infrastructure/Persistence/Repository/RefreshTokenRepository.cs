using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Entities;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="IRefreshTokenRepository"/>; registered as scoped. Reads are tracked so a loaded token can be modified and passed to <see cref="SaveAsync"/>; every write is saved immediately.
/// PL: Implementacja <see cref="IRefreshTokenRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty są śledzone, więc wczytany token można zmodyfikować i przekazać do <see cref="SaveAsync"/>; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// EN: Creates the repository.
    /// PL: Tworzy repozytorium.
    /// </summary>
    /// <param name="db">
    /// EN: Scoped database context.
    /// PL: Kontekst bazy danych o zasięgu scoped.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="db"/> is null.
    /// PL: <paramref name="db"/> jest null.
    /// </exception>
    public RefreshTokenRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RevokeAllForUserAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken)
    {
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in active)
        {
            token.Revoke(revokedAt);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
