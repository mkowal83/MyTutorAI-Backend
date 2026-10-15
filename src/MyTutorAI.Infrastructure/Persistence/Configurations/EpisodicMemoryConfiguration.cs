using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Episodic;
using System.Text.Json;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="EpisodicMemory"/>: one row per user keyed by the <c>UserId</c> shadow property, episodes stored as a complex collection in the <c>Episodes</c> JSON column (each episode's mistakes serialized as a JSON string), and a <c>RowVersion</c> concurrency token.
/// PL: Mapowanie EF Core dla <see cref="EpisodicMemory"/>: jeden wiersz na użytkownika kluczowany shadow property <c>UserId</c>, epizody zapisane jako kolekcja typu złożonego w kolumnie JSON <c>Episodes</c> (błędy każdego epizodu serializowane jako string JSON) oraz token współbieżności <c>RowVersion</c>.
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

        // EN: Complex collection (no shadow ordinal key), so updating a detached episodic memory rewrites the whole JSON column.
        //     Mapped through the List<Episode> backing field because complex collections require IList<T>.
        // PL: Kolekcja typu złożonego (bez shadow klucza ordinal), więc aktualizacja odłączonej pamięci epizodycznej nadpisuje całą kolumnę JSON.
        //     Mapowana przez pole List<Episode>, bo kolekcje typów złożonych wymagają IList<T>.
        builder.Ignore(e => e.Episodes);
        builder.ComplexCollection<List<Episode>, Episode>("_episodes", episode =>
        {
            episode.UsePropertyAccessMode(PropertyAccessMode.Field);
            episode.ToJson("Episodes");

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
