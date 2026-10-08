using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="Session"/>: key <see cref="Session.Id"/>, an index on <see cref="Session.UserId"/>, required topic, phase, status and start time, field access, and a <c>RowVersion</c> shadow property for optimistic concurrency.
/// PL: Mapowanie EF Core dla <see cref="Session"/>: klucz <see cref="Session.Id"/>, indeks na <see cref="Session.UserId"/>, wymagane temat, faza, status i czas rozpoczęcia, dostęp przez pola oraz shadow property <c>RowVersion</c> do optymistycznej współbieżności.
/// </summary>
public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();
        builder.HasIndex(s => s.UserId);

        builder.Property(s => s.Topic).IsRequired();
        builder.Property(s => s.Phase).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.StartedAt).IsRequired();

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
