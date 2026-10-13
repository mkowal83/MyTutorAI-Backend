using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory.Core;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="ICoreMemoryRepository"/>; registered as scoped. Reads are not tracked; every write is saved immediately.
/// PL: Implementacja <see cref="ICoreMemoryRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty nie są śledzone; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class CoreMemoryRepository : ICoreMemoryRepository
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
    public CoreMemoryRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public async Task<CoreMemory> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var core = await _db.CoreMemories.AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Property<Guid>(c, "UserId") == userId, cancellationToken)
            .ConfigureAwait(false);

        return core ?? CoreMemory.Default;
    }

    /// <inheritdoc />
    public async Task SaveAsync(Guid userId, CoreMemory core, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(core);

        var rowVersion = await _db.CoreMemories.AsNoTracking()
            .Where(c => EF.Property<Guid>(c, "UserId") == userId)
            .Select(c => EF.Property<byte[]>(c, "RowVersion"))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var entry = _db.Entry(core);
        entry.Property("UserId").CurrentValue = userId;
        entry.State = rowVersion is null ? EntityState.Added : EntityState.Modified;

        // Reads are not tracked, so the row version is lost; restore it to avoid a false concurrency conflict.
        if (rowVersion is not null)
        {
            entry.Property("RowVersion").OriginalValue = rowVersion;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

