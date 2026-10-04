using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Core;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="CoreMemory"/>: one row per user, keyed by the shadow property <c>UserId</c>, with a <c>RowVersion</c> concurrency token; applied automatically by <see cref="AppDbContext"/>.
/// PL: Mapowanie EF Core dla <see cref="CoreMemory"/>: jeden wiersz na użytkownika, kluczowany shadow property <c>UserId</c>, z tokenem współbieżności <c>RowVersion</c>; stosowane automatycznie przez <see cref="AppDbContext"/>.
/// </summary>
public sealed class CoreMemoryConfiguration : IEntityTypeConfiguration<CoreMemory>
{
    /// <summary>
    /// EN: Configures the key, the required identity columns, the prime directives column and the row version.
    /// PL: Konfiguruje klucz, wymagane kolumny tożsamości, kolumnę nadrzędnych dyrektyw i wersję wiersza.
    /// </summary>
    /// <param name="builder">
    /// EN: Entity type builder supplied by EF Core.
    /// PL: Builder typu encji dostarczany przez EF Core.
    /// </param>
    public void Configure(EntityTypeBuilder<CoreMemory> builder)
    {
        builder.Property<Guid>("UserId");
        builder.HasKey("UserId");

        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.CreatorName).IsRequired();
        builder.Property(c => c.Role).IsRequired();

        builder.Property(c => c.PrimeDirectives);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}