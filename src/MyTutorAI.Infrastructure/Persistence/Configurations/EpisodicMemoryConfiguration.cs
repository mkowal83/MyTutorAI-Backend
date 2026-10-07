using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Episodic;
using System.Text.Json;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="EpisodicMemory"/>: one row per user keyed by the <c>UserId</c> shadow property, episodes stored as a JSON column, and a <c>RowVersion</c> concurrency token.
/// PL: Mapowanie EF Core dla <see cref="EpisodicMemory"/>: jeden wiersz na użytkownika kluczowany shadow property <c>UserId</c>, epizody zapisane jako kolumna JSON oraz token współbieżności <c>RowVersion</c>.
/// </summary>
public sealed class EpisodicMemoryConfiguration : IEntityTypeConfiguration<EpisodicMemory>
{
    // EN: Compares mistake lists element by element so EF Core detects changes inside the converted JSON value.
    // PL: Porównuje listy błędów element po elemencie, aby EF Core wykrywał zmiany wewnątrz skonwertowanej wartości JSON.
    private static readonly ValueComparer<IReadOnlyList<MistakeLog>> MistakesComparer = new(
        (a, b) => (a ?? Array.Empty<MistakeLog>()).SequenceEqual(b ?? Array.Empty<MistakeLog>()),
        v => v.Aggregate(0, (hash, m) => HashCode.Combine(hash, m.GetHashCode())),
        v => v.ToList());

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EpisodicMemory> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<Guid>("UserId");
        builder.HasKey("UserId");

        builder.Ignore(e => e.Count);

        builder.Property(e => e.MaxEpisodes).IsRequired();

        builder.OwnsMany(e => e.Episodes, episode =>
        {
            episode.UsePropertyAccessMode(PropertyAccessMode.Field);
            episode.ToJson();

            episode.Property(ep => ep.Summary).IsRequired();
            episode.Property(ep => ep.PersonalFacts);
            episode.Property(ep => ep.NextStep).IsRequired();

            episode.Property(ep => ep.Mistakes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<MistakeLog>>(v, (JsonSerializerOptions?)null) ?? new List<MistakeLog>())
                .Metadata.SetValueComparer(MistakesComparer);
        });

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
