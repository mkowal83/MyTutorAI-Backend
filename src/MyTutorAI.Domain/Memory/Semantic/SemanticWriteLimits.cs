using System.Runtime.CompilerServices;

namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Maximum number of entries kept in each semantic profile list by <see cref="SemanticWriteApplier"/>; when a list grows beyond its limit, the oldest entries are dropped. Every limit is validated on construction and must be positive.
/// PL: Maksymalna liczba wpisów zachowywanych w każdej liście profilu semantycznego przez <see cref="SemanticWriteApplier"/>; gdy lista przekroczy limit, najstarsze wpisy są usuwane. Każdy limit jest walidowany przy tworzeniu i musi być dodatni.
/// </summary>
/// <exception cref="ArgumentOutOfRangeException">
/// EN: A limit passed to the constructor is zero or negative.
/// PL: Limit przekazany do konstruktora jest zerem lub liczbą ujemną.
/// </exception>
public sealed record SemanticWriteLimits(
    int PersonalFacts = SemanticWriteLimits.HighWaterMark,
    int RelationshipHooks = SemanticWriteLimits.StandardWaterMark,
    int UpcomingEvents = SemanticWriteLimits.StandardWaterMark,
    int WorkSituations = SemanticWriteLimits.StandardWaterMark,
    int Preferences = SemanticWriteLimits.HighWaterMark,
    int ConfidenceBlockers = SemanticWriteLimits.StandardWaterMark,
    int MotivationDrivers = SemanticWriteLimits.StandardWaterMark,
    int VocabularyTargets = SemanticWriteLimits.HighWaterMark,
    int ToneTargets = SemanticWriteLimits.StandardWaterMark,
    int RecurringWeaknesses = SemanticWriteLimits.StandardWaterMark)
{
    /// <summary>
    /// EN: Default limit of the higher-capacity lists (personal facts, preferences, vocabulary targets).
    /// PL: Domyślny limit list o większej pojemności (fakty osobiste, preferencje, cele słownictwa).
    /// </summary>
    public const int HighWaterMark = 8;

    /// <summary>
    /// EN: Default limit of the remaining lists.
    /// PL: Domyślny limit pozostałych list.
    /// </summary>
    public const int StandardWaterMark = 6;

    /// <summary>
    /// EN: Shared instance with all default limits; used by <see cref="SemanticWriteApplier.Apply"/> when no limits are passed.
    /// PL: Współdzielona instancja ze wszystkimi domyślnymi limitami; używana przez <see cref="SemanticWriteApplier.Apply"/>, gdy nie przekazano limitów.
    /// </summary>
    public static SemanticWriteLimits Default { get; } = new();

    /// <summary>
    /// EN: Maximum number of entries in <c>PersonalContext.PersonalFacts</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>PersonalContext.PersonalFacts</c>; dodatnia.
    /// </summary>
    public int PersonalFacts { get; init; } = Positive(PersonalFacts);

    /// <summary>
    /// EN: Maximum number of entries in <c>PersonalContext.RelationshipHooks</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>PersonalContext.RelationshipHooks</c>; dodatnia.
    /// </summary>
    public int RelationshipHooks { get; init; } = Positive(RelationshipHooks);

    /// <summary>
    /// EN: Maximum number of entries in <c>PersonalContext.UpcomingEvents</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>PersonalContext.UpcomingEvents</c>; dodatnia.
    /// </summary>
    public int UpcomingEvents { get; init; } = Positive(UpcomingEvents);

    /// <summary>
    /// EN: Maximum number of entries in <c>WorkContext.HighStakesSituations</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>WorkContext.HighStakesSituations</c>; dodatnia.
    /// </summary>
    public int WorkSituations { get; init; } = Positive(WorkSituations);

    /// <summary>
    /// EN: Maximum number of entries in <c>StudentProfile.Preferences</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>StudentProfile.Preferences</c>; dodatnia.
    /// </summary>
    public int Preferences { get; init; } = Positive(Preferences);

    /// <summary>
    /// EN: Maximum number of entries in <c>SemanticSignals.ConfidenceBlockers</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>SemanticSignals.ConfidenceBlockers</c>; dodatnia.
    /// </summary>
    public int ConfidenceBlockers { get; init; } = Positive(ConfidenceBlockers);

    /// <summary>
    /// EN: Maximum number of entries in <c>SemanticSignals.MotivationDrivers</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>SemanticSignals.MotivationDrivers</c>; dodatnia.
    /// </summary>
    public int MotivationDrivers { get; init; } = Positive(MotivationDrivers);

    /// <summary>
    /// EN: Maximum number of entries in <c>SemanticSignals.VocabularyTargets</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>SemanticSignals.VocabularyTargets</c>; dodatnia.
    /// </summary>
    public int VocabularyTargets { get; init; } = Positive(VocabularyTargets);

    /// <summary>
    /// EN: Maximum number of entries in <c>PerformanceProfile.ToneTargets</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>PerformanceProfile.ToneTargets</c>; dodatnia.
    /// </summary>
    public int ToneTargets { get; init; } = Positive(ToneTargets);

    /// <summary>
    /// EN: Maximum number of entries in <c>PerformanceProfile.RecurringWeaknesses</c>; positive.
    /// PL: Maksymalna liczba wpisów w <c>PerformanceProfile.RecurringWeaknesses</c>; dodatnia.
    /// </summary>
    public int RecurringWeaknesses { get; init; } = Positive(RecurringWeaknesses);

    private static int Positive(int value, [CallerArgumentExpression(nameof(value))] string? name = null) =>
        value > 0
            ? value
            : throw new ArgumentOutOfRangeException(name, value, "Limit must be positive.");
}
