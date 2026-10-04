using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// EN: EF Core mapping of <see cref="StudentProfile"/>: one row per user keyed by <see cref="StudentProfile.UserId"/>, with goals and all contexts stored as JSON columns and a <c>RowVersion</c> concurrency token; applied automatically by <see cref="AppDbContext"/>.
/// PL: Mapowanie EF Core dla <see cref="StudentProfile"/>: jeden wiersz na użytkownika kluczowany przez <see cref="StudentProfile.UserId"/>, z celami i wszystkimi kontekstami zapisanymi jako kolumny JSON oraz tokenem współbieżności <c>RowVersion</c>; stosowane automatycznie przez <see cref="AppDbContext"/>.
/// </summary>
public sealed class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    /// <summary>
    /// EN: Configures field-based property access, the key, required columns, JSON-owned goals and contexts, and the row version.
    /// PL: Konfiguruje dostęp do właściwości przez pola, klucz, wymagane kolumny, cele i konteksty jako posiadane typy w JSON oraz wersję wiersza.
    /// </summary>
    /// <param name="builder">
    /// EN: Entity type builder supplied by EF Core.
    /// PL: Builder typu encji dostarczany przez EF Core.
    /// </param>
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(p => p.UserId);

        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Level).IsRequired();
        builder.Property(p => p.Goal).IsRequired();
        builder.Property(p => p.NativeLanguage).IsRequired();
        builder.Property(p => p.TargetLanguage).IsRequired();
        builder.Property(p => p.ContextNotes);

        builder.Property(p => p.Preferences);

        builder.OwnsMany(p => p.Goals, goals => goals.ToJson());

        builder.OwnsOne(p => p.WorkContext, wc => wc.ToJson());
        builder.OwnsOne(p => p.CommunicationPreferences, cp => cp.ToJson());
        builder.OwnsOne(p => p.PersonalContext, pc => pc.ToJson());
        builder.OwnsOne(p => p.SemanticSignals, ss => ss.ToJson());
        builder.OwnsOne(p => p.PerformanceProfile, pp => pp.ToJson());

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}