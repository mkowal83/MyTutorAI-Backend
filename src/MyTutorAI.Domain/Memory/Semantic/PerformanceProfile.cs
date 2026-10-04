namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: The learner's communication performance profile: target scenarios, desired tone, strengths, weaknesses and success criteria.
/// PL: Profil wykonania komunikacyjnego ucznia: scenariusze docelowe, pożądany ton, mocne strony, słabości i kryteria sukcesu.
/// </summary>
/// <param name="TargetScenarios">
/// EN: Scenarios to practice; copied, <c>null</c> gives an empty list.
/// PL: Scenariusze do ćwiczenia; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="ToneTargets">
/// EN: How the learner wants to sound; copied, <c>null</c> gives an empty list.
/// PL: Jak uczeń chce brzmieć; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="Strengths">
/// EN: Communication strengths; copied, <c>null</c> gives an empty list.
/// PL: Mocne strony komunikacyjne; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="RecurringWeaknesses">
/// EN: Recurring communication or language weaknesses; copied, <c>null</c> gives an empty list.
/// PL: Powtarzające się słabości komunikacyjne lub językowe; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="SuccessCriteria">
/// EN: Observable success criteria; copied, <c>null</c> gives an empty list.
/// PL: Obserwowalne kryteria sukcesu; kopiowane, <c>null</c> daje pustą listę.
/// </param>
public sealed record PerformanceProfile(
    IReadOnlyList<string>? TargetScenarios = null,
    IReadOnlyList<string>? ToneTargets = null,
    IReadOnlyList<string>? Strengths = null,
    IReadOnlyList<string>? RecurringWeaknesses = null,
    IReadOnlyList<string>? SuccessCriteria = null)
{
    /// <summary>
    /// EN: Target scenarios to practice; never null.
    /// PL: Docelowe scenariusze do ćwiczenia; nigdy null.
    /// </summary>
    public IReadOnlyList<string> TargetScenarios { get; init; } =
        TargetScenarios is null ? [] : [.. TargetScenarios];

    /// <summary>
    /// EN: How the learner wants to sound, e.g. calm, executive, natural; never null.
    /// PL: Jak uczeń chce brzmieć, np. spokojnie, menedżersko, naturalnie; nigdy null.
    /// </summary>
    public IReadOnlyList<string> ToneTargets { get; init; } =
        ToneTargets is null ? [] : [.. ToneTargets];

    /// <summary>
    /// EN: Communication strengths; never null.
    /// PL: Mocne strony komunikacyjne; nigdy null.
    /// </summary>
    public IReadOnlyList<string> Strengths { get; init; } =
        Strengths is null ? [] : [.. Strengths];

    /// <summary>
    /// EN: Recurring communication or language weaknesses; never null.
    /// PL: Powtarzające się słabości komunikacyjne lub językowe; nigdy null.
    /// </summary>
    public IReadOnlyList<string> RecurringWeaknesses { get; init; } =
        RecurringWeaknesses is null ? [] : [.. RecurringWeaknesses];

    /// <summary>
    /// EN: Observable success criteria; never null.
    /// PL: Obserwowalne kryteria sukcesu; nigdy null.
    /// </summary>
    public IReadOnlyList<string> SuccessCriteria { get; init; } =
        SuccessCriteria is null ? [] : [.. SuccessCriteria];
}
