using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="IStudentProfileRepository"/>; registered as scoped. Reads are not tracked; every write is saved immediately.
/// PL: Implementacja <see cref="IStudentProfileRepository"/> na SQL Server (EF Core); rejestrowana jako scoped. Odczyty nie są śledzone; każdy zapis jest od razu utrwalany.
/// </summary>
public sealed class StudentProfileRepository : IStudentProfileRepository
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
    public StudentProfileRepository(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <inheritdoc />
    public async Task<StudentProfile> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _db.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        return profile ?? new StudentProfile(userId, "Student", AgentState.DefaultBlankLevel, AgentState.DefaultBlankGoal);
    }

    /// <inheritdoc />
    public async Task SaveAsync(StudentProfile profile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var rowVersion = await _db.StudentProfiles.AsNoTracking()
            .Where(p => p.UserId == profile.UserId)
            .Select(p => EF.Property<byte[]>(p, "RowVersion"))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Add/Update traverse the graph, so the owned JSON types and the complex JSON goals are saved too.
        var entry = rowVersion is null ? _db.StudentProfiles.Add(profile) : _db.StudentProfiles.Update(profile);

        // Reads are not tracked, so the row version is lost; restore it to avoid a false concurrency conflict.
        if (rowVersion is not null)
        {
            entry.Property("RowVersion").OriginalValue = rowVersion;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}