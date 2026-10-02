using Microsoft.EntityFrameworkCore;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Infrastructure.Persistence;

/// <summary>
/// EN: EF Core context of the SQL Server store holding all data that outlives a single session. Entity mappings are applied from the <c>Configurations</c> folder of this assembly.
/// PL: Kontekst EF Core magazynu SQL Server przechowującego wszystkie dane trwalsze niż jedna sesja. Mapowania encji są stosowane z folderu <c>Configurations</c> tego assembly.
/// </summary>
/// <param name="options">
/// EN: Context options configured in DI (provider and connection string).
/// PL: Opcje kontekstu skonfigurowane w DI (provider i connection string).
/// </param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// EN: User accounts.
    /// PL: Konta użytkowników.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
