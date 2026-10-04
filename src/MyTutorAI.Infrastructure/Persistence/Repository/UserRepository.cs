using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Users;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: SQL Server (EF Core) implementation of <see cref="IUserRepository"/>. Reads are not tracked; every write is saved immediately.
/// PL: Implementacja <see cref="IUserRepository"/> na SQL Server (EF Core). Odczyty nie są śledzone; każdy zapis jest od razu utrwalany.
/// </summary>
/// <param name="db">
/// EN: Scoped database context.
/// PL: Kontekst bazy danych o zasięgu scoped.
/// </param>
internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    // SQL Server: 2601 — duplikat w unikalnym indeksie, 2627 — naruszenie ograniczenia UNIQUE/PK.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
                                           {
                                               Number: UniqueIndexViolation or UniqueConstraintViolation,
                                           })
        {
            db.Entry(user).State = EntityState.Detached;
            throw new DuplicateEmailException(ex);
        }
    }
}
