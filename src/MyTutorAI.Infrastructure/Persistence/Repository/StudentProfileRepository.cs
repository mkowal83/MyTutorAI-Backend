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

        var exists = await _db.StudentProfiles.AsNoTracking()
            .AnyAsync(p => p.UserId == profile.UserId, cancellationToken)
            .ConfigureAwait(false);

        _db.Entry(profile).State = exists ? EntityState.Modified : EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}