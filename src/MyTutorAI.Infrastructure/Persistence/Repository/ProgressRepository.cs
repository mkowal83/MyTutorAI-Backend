using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="IProgressRepository"/>; registered as scoped. Reads are not tracked; every write is saved immediately.
/// PL: Implementacja <see cref="IProgressRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty nie są śledzone; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class ProgressRepository : IProgressRepository
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
    public ProgressRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public async Task<LearningProgress> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var progress = await _db.Progress.AsNoTracking()
            .FirstOrDefaultAsync(p => EF.Property<Guid>(p, "UserId") == userId, cancellationToken)
            .ConfigureAwait(false);

        return progress ?? new LearningProgress();
    }

    /// <inheritdoc />
    public async Task SaveAsync(Guid userId, LearningProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var exists = await _db.Progress.AsNoTracking()
            .AnyAsync(p => EF.Property<Guid>(p, "UserId") == userId, cancellationToken)
            .ConfigureAwait(false);

        var entry = _db.Entry(progress);
        entry.Property("UserId").CurrentValue = userId;
        entry.State = exists ? EntityState.Modified : EntityState.Added;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

