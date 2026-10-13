using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory.Episodic;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="IEpisodicMemoryRepository"/>; registered as scoped. Reads are not tracked; every write is saved immediately.
/// PL: Implementacja <see cref="IEpisodicMemoryRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty nie są śledzone; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class EpisodicMemoryRepository : IEpisodicMemoryRepository
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
    public EpisodicMemoryRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public async Task<EpisodicMemory> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var episodic = await _db.Episodes.AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "UserId") == userId, cancellationToken)
            .ConfigureAwait(false);

        return episodic ?? new EpisodicMemory();
    }

    /// <inheritdoc />
    public async Task SaveAsync(Guid userId, EpisodicMemory episodic, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(episodic);

        var rowVersion = await _db.Episodes.AsNoTracking()
            .Where(e => EF.Property<Guid>(e, "UserId") == userId)
            .Select(e => EF.Property<byte[]>(e, "RowVersion"))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        _db.Entry(episodic).Property("UserId").CurrentValue = userId;

        // Add/Update traverse the graph, so the owned JSON episodes are saved too.
        var entry = rowVersion is null ? _db.Episodes.Add(episodic) : _db.Episodes.Update(episodic);

        // Reads are not tracked, so the row version is lost; restore it to avoid a false concurrency conflict.
        if (rowVersion is not null)
        {
            entry.Property("RowVersion").OriginalValue = rowVersion;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
