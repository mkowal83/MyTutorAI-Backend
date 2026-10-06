using Microsoft.EntityFrameworkCore;
using MyTutorAI.Domain.Entities;
using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Infrastructure.Persistence;

/// <summary>
/// EN: EF Core context of the SQL Server store holding all data that outlives a single session. Entity mappings are applied from every <see cref="IEntityTypeConfiguration{TEntity}"/> in this assembly.
/// PL: Kontekst EF Core magazynu SQL Server przechowującego wszystkie dane trwalsze niż jedna sesja. Mapowania encji są stosowane ze wszystkich <see cref="IEntityTypeConfiguration{TEntity}"/> w tym assembly.
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

    /// <summary>
    /// EN: Issued refresh tokens, stored only as hashes and keyed by user.
    /// PL: Wydane refresh tokeny, przechowywane wyłącznie jako hashe i powiązane z użytkownikiem.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// EN: Core memory layer, one record per user keyed by the <c>UserId</c> shadow property.
    /// PL: Warstwa pamięci Core, jeden rekord na użytkownika kluczowany shadow property <c>UserId</c>.
    /// </summary>
    public DbSet<CoreMemory> CoreMemories => Set<CoreMemory>();

    /// <summary>
    /// EN: Learner profiles forming the semantic memory layer, one per user.
    /// PL: Profile uczniów tworzące warstwę pamięci semantycznej, jeden na użytkownika.
    /// </summary>
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    /// <summary>
    /// EN: Learning progress of learners (part of the semantic memory layer), one record per user keyed by the <c>UserId</c> shadow property.
    /// PL: Postęp nauki uczniów (część warstwy pamięci semantycznej), jeden rekord na użytkownika kluczowany shadow property <c>UserId</c>.
    /// </summary>
    public DbSet<LearningProgress> Progress => Set<LearningProgress>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
