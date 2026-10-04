using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Entities;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="RefreshToken"/>, applied automatically by <see cref="AppDbContext"/>. <see cref="PropertyAccessMode.Field"/> lets EF write to backing fields, because <see cref="RefreshToken.RevokedAt"/> and <see cref="RefreshToken.ReplacedByTokenHash"/> change only through <see cref="RefreshToken.Revoke"/>.
/// PL: Mapowanie EF Core encji <see cref="RefreshToken"/>, stosowane automatycznie przez <see cref="AppDbContext"/>. <see cref="PropertyAccessMode.Field"/> pozwala EF zapisywać pola zapasowe, bo <see cref="RefreshToken.RevokedAt"/> i <see cref="RefreshToken.ReplacedByTokenHash"/> zmieniają się wyłącznie przez <see cref="RefreshToken.Revoke"/>.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// EN: Configures the key, a unique index on <see cref="RefreshToken.TokenHash"/>, an index on <see cref="RefreshToken.UserId"/> and a <c>RowVersion</c> shadow property for optimistic concurrency (a conflicting save throws <c>DbUpdateConcurrencyException</c>).
    /// PL: Konfiguruje klucz, unikalny indeks na <see cref="RefreshToken.TokenHash"/>, indeks na <see cref="RefreshToken.UserId"/> oraz właściwość cienia <c>RowVersion</c> do optymistycznej współbieżności (konfliktowy zapis zgłasza <c>DbUpdateConcurrencyException</c>).
    /// </summary>
    /// <param name="builder">
    /// EN: Entity type builder supplied by EF Core.
    /// PL: Builder typu encji dostarczany przez EF Core.
    /// </param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Supports RevokeAllForUserAsync (scan of the given user's active tokens).
        builder.Property(t => t.UserId).IsRequired();
        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.RevokedAt);
        builder.Property(t => t.ReplacedByTokenHash);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
