using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="ISessionRepository"/>; registered as scoped. Reads are tracked by the scoped context; every write is saved immediately.
/// PL: Implementacja <see cref="ISessionRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty są śledzone przez kontekst o zasięgu scoped; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class SessionRepository : ISessionRepository
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
    public SessionRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public Task<Session?> GetAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken) =>
        _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="DbUpdateConcurrencyException">
    /// EN: The row was changed by another request since it was loaded (<c>RowVersion</c> mismatch); the API maps it to 409.
    /// PL: Wiersz został zmieniony przez inne żądanie od czasu wczytania (niezgodny <c>RowVersion</c>); API mapuje to na 409.
    /// </exception>
    public async Task SaveAsync(Session session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        _db.Sessions.Update(session);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
