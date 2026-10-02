using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: Maps <see cref="User"/> to the <c>Users</c> table. Get-only properties are mapped explicitly and materialized through the entity constructor; the unique index on <see cref="User.Email"/> guards against concurrent duplicate registrations.
/// PL: Mapuje <see cref="User"/> na tabelę <c>Users</c>. Właściwości tylko do odczytu są mapowane jawnie i materializowane przez konstruktor encji; unikalny indeks na <see cref="User.Email"/> chroni przed równoległą rejestracją duplikatów.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(User.EmailMaxLength);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(User.DisplayNameMaxLength);

        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
