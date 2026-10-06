using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: EF Core mapping of <see cref="LearningProgress"/> to the <c>Progress</c> table: one row per user keyed by the <c>UserId</c> shadow property, members accessed through backing fields, <see cref="LearningProgress.RecentMistakes"/> stored as a primitive collection in a single column, and a <c>RowVersion</c> shadow property used as the concurrency token.
/// PL: Mapowanie EF Core <see cref="LearningProgress"/> na tabelę <c>Progress</c>: jeden wiersz na użytkownika kluczowany shadow property <c>UserId</c>, dostęp do składowych przez pola, <see cref="LearningProgress.RecentMistakes"/> zapisywane jako kolekcja prymitywna w jednej kolumnie oraz shadow property <c>RowVersion</c> używane jako token współbieżności.
/// </summary>
public sealed class LearningProgressConfiguration : IEntityTypeConfiguration<LearningProgress>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LearningProgress> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<Guid>("UserId");
        builder.HasKey("UserId");

        builder.Property(p => p.RecentMistakes);
        builder.Property(p => p.HomeworkStatus).IsRequired();
        builder.Property(p => p.NextFocus);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}