namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Domain input for <see cref="SemanticWriteApplier"/>: candidate entries for each list of the learner's semantic profile, extracted from one conversation turn. Every list is copied on construction and <c>null</c> becomes an empty list, so properties are never null.
/// PL: Wejście domenowe dla <see cref="SemanticWriteApplier"/>: wpisy kandydujące dla każdej listy profilu semantycznego ucznia, wyciągnięte z jednej tury rozmowy. Każda lista jest kopiowana przy tworzeniu, a <c>null</c> staje się pustą listą, więc właściwości nigdy nie są null.
/// </summary>
public sealed record SemanticWriteInput(
    IReadOnlyList<string>? PersonalFacts = null,
    IReadOnlyList<string>? RelationshipHooks = null,
    IReadOnlyList<string>? UpcomingEvents = null,
    IReadOnlyList<string>? WorkSituations = null,
    IReadOnlyList<string>? Preferences = null,
    IReadOnlyList<string>? ConfidenceBlockers = null,
    IReadOnlyList<string>? MotivationDrivers = null,
    IReadOnlyList<string>? VocabularyTargets = null,
    IReadOnlyList<string>? ToneTargets = null,
    IReadOnlyList<string>? RecurringWeaknesses = null)
{
    /// <summary>
    /// EN: Durable facts about the learner's life (→ <c>PersonalContext.PersonalFacts</c>).
    /// PL: Trwałe fakty z życia ucznia (→ <c>PersonalContext.PersonalFacts</c>).
    /// </summary>
    public IReadOnlyList<string> PersonalFacts { get; init; } =
        PersonalFacts is null ? [] : [.. PersonalFacts];

    /// <summary>
    /// EN: Small personal details for natural conversation openers (→ <c>PersonalContext.RelationshipHooks</c>).
    /// PL: Drobne szczegóły osobiste do naturalnego otwierania rozmowy (→ <c>PersonalContext.RelationshipHooks</c>).
    /// </summary>
    public IReadOnlyList<string> RelationshipHooks { get; init; } =
        RelationshipHooks is null ? [] : [.. RelationshipHooks];

    /// <summary>
    /// EN: Upcoming high-stakes events (→ <c>PersonalContext.UpcomingEvents</c>).
    /// PL: Nadchodzące wydarzenia wysokiej stawki (→ <c>PersonalContext.UpcomingEvents</c>).
    /// </summary>
    public IReadOnlyList<string> UpcomingEvents { get; init; } =
        UpcomingEvents is null ? [] : [.. UpcomingEvents];

    /// <summary>
    /// EN: High-stakes work situations (→ <c>WorkContext.HighStakesSituations</c>).
    /// PL: Sytuacje zawodowe wysokiej stawki (→ <c>WorkContext.HighStakesSituations</c>).
    /// </summary>
    public IReadOnlyList<string> WorkSituations { get; init; } =
        WorkSituations is null ? [] : [.. WorkSituations];

    /// <summary>
    /// EN: Learning style and pace preferences (→ <c>StudentProfile.Preferences</c>).
    /// PL: Preferencje stylu i tempa nauki (→ <c>StudentProfile.Preferences</c>).
    /// </summary>
    public IReadOnlyList<string> Preferences { get; init; } =
        Preferences is null ? [] : [.. Preferences];

    /// <summary>
    /// EN: Stressors and confidence blockers (→ <c>SemanticSignals.ConfidenceBlockers</c>).
    /// PL: Stresory i blokady pewności siebie (→ <c>SemanticSignals.ConfidenceBlockers</c>).
    /// </summary>
    public IReadOnlyList<string> ConfidenceBlockers { get; init; } =
        ConfidenceBlockers is null ? [] : [.. ConfidenceBlockers];

    /// <summary>
    /// EN: Reasons the learner cares about learning (→ <c>SemanticSignals.MotivationDrivers</c>).
    /// PL: Powody, dla których uczniowi zależy na nauce (→ <c>SemanticSignals.MotivationDrivers</c>).
    /// </summary>
    public IReadOnlyList<string> MotivationDrivers { get; init; } =
        MotivationDrivers is null ? [] : [.. MotivationDrivers];

    /// <summary>
    /// EN: Vocabulary and areas for further practice (→ <c>SemanticSignals.VocabularyTargets</c>).
    /// PL: Słownictwo i obszary do dalszego ćwiczenia (→ <c>SemanticSignals.VocabularyTargets</c>).
    /// </summary>
    public IReadOnlyList<string> VocabularyTargets { get; init; } =
        VocabularyTargets is null ? [] : [.. VocabularyTargets];

    /// <summary>
    /// EN: How the learner wants to sound (→ <c>PerformanceProfile.ToneTargets</c>).
    /// PL: Jak uczeń chce brzmieć (→ <c>PerformanceProfile.ToneTargets</c>).
    /// </summary>
    public IReadOnlyList<string> ToneTargets { get; init; } =
        ToneTargets is null ? [] : [.. ToneTargets];

    /// <summary>
    /// EN: Recurring communication and language weaknesses (→ <c>PerformanceProfile.RecurringWeaknesses</c>).
    /// PL: Powtarzające się słabości komunikacyjne i językowe (→ <c>PerformanceProfile.RecurringWeaknesses</c>).
    /// </summary>
    public IReadOnlyList<string> RecurringWeaknesses { get; init; } =
        RecurringWeaknesses is null ? [] : [.. RecurringWeaknesses];
}