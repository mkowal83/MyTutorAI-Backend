namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Motivational and thematic signals extracted from conversations.
/// PL: Sygnały motywacyjne i tematyczne wydobyte z rozmów.
/// </summary>
/// <param name="ConfidenceBlockers">
/// EN: Stressors and blockers that undermine fluency; copied, <c>null</c> gives an empty list.
/// PL: Stresory i blokady osłabiające płynność; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="MotivationDrivers">
/// EN: Reasons the learner genuinely cares; copied, <c>null</c> gives an empty list.
/// PL: Powody, dla których uczniowi naprawdę zależy; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="RecurringTopics">
/// EN: Recurring conversation topics; copied, <c>null</c> gives an empty list.
/// PL: Powracające tematy rozmów; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="VocabularyTargets">
/// EN: Words or vocabulary areas for further practice; copied, <c>null</c> gives an empty list.
/// PL: Słowa lub obszary słownictwa do dalszego ćwiczenia; kopiowane, <c>null</c> daje pustą listę.
/// </param>
public sealed record SemanticSignals(
    IReadOnlyList<string>? ConfidenceBlockers = null,
    IReadOnlyList<string>? MotivationDrivers = null,
    IReadOnlyList<string>? RecurringTopics = null,
    IReadOnlyList<string>? VocabularyTargets = null)
{
    /// <summary>
    /// EN: Stressors and blockers that undermine fluency; never null.
    /// PL: Stresory i blokady osłabiające płynność; nigdy null.
    /// </summary>
    public IReadOnlyList<string> ConfidenceBlockers { get; init; } =
        ConfidenceBlockers is null ? [] : [.. ConfidenceBlockers];

    /// <summary>
    /// EN: Reasons the learner genuinely cares; never null.
    /// PL: Powody, dla których uczniowi naprawdę zależy; nigdy null.
    /// </summary>
    public IReadOnlyList<string> MotivationDrivers { get; init; } =
        MotivationDrivers is null ? [] : [.. MotivationDrivers];

    /// <summary>
    /// EN: Recurring conversation topics; never null.
    /// PL: Powracające tematy rozmów; nigdy null.
    /// </summary>
    public IReadOnlyList<string> RecurringTopics { get; init; } =
        RecurringTopics is null ? [] : [.. RecurringTopics];

    /// <summary>
    /// EN: Words or vocabulary areas for further practice; never null.
    /// PL: Słowa lub obszary słownictwa do dalszego ćwiczenia; nigdy null.
    /// </summary>
    public IReadOnlyList<string> VocabularyTargets { get; init; } =
        VocabularyTargets is null ? [] : [.. VocabularyTargets];
}