namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: The learner's personal context, used to build rapport and personal examples.
/// PL: Kontekst osobisty ucznia, używany do budowania relacji i osobistych przykładów.
/// </summary>
/// <param name="PersonalFacts">
/// EN: Lasting life circumstances; copied, <c>null</c> gives an empty list.
/// PL: Trwałe okoliczności życiowe; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="RelationshipHooks">
/// EN: Small details that make natural conversation starters; copied, <c>null</c> gives an empty list.
/// PL: Drobne szczegóły, od których można naturalnie zacząć rozmowę; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="UpcomingEvents">
/// EN: Upcoming events important to the learner; copied, <c>null</c> gives an empty list.
/// PL: Nadchodzące wydarzenia ważne dla ucznia; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="ScheduleConstraints">
/// EN: Constraints on the learning schedule; copied, <c>null</c> gives an empty list.
/// PL: Ograniczenia harmonogramu nauki; kopiowane, <c>null</c> daje pustą listę.
/// </param>
public sealed record PersonalContext(
    IReadOnlyList<string>? PersonalFacts = null,
    IReadOnlyList<string>? RelationshipHooks = null,
    IReadOnlyList<string>? UpcomingEvents = null,
    IReadOnlyList<string>? ScheduleConstraints = null)
{
    /// <summary>
    /// EN: Lasting life circumstances, e.g. children, pets, city; never null.
    /// PL: Trwałe okoliczności życiowe, np. dzieci, zwierzęta, miasto; nigdy null.
    /// </summary>
    public IReadOnlyList<string> PersonalFacts { get; init; } =
        PersonalFacts is null ? [] : [.. PersonalFacts];

    /// <summary>
    /// EN: Small details that make natural conversation starters; never null.
    /// PL: Drobne szczegóły, od których można naturalnie zacząć rozmowę; nigdy null.
    /// </summary>
    public IReadOnlyList<string> RelationshipHooks { get; init; } =
        RelationshipHooks is null ? [] : [.. RelationshipHooks];

    /// <summary>
    /// EN: Upcoming presentations, interviews, deadlines and high-stakes meetings; never null.
    /// PL: Nadchodzące prezentacje, rozmowy kwalifikacyjne, terminy i spotkania o wysokiej stawce; nigdy null.
    /// </summary>
    public IReadOnlyList<string> UpcomingEvents { get; init; } =
        UpcomingEvents is null ? [] : [.. UpcomingEvents];

    /// <summary>
    /// EN: Constraints on the learning schedule; never null.
    /// PL: Ograniczenia harmonogramu nauki; nigdy null.
    /// </summary>
    public IReadOnlyList<string> ScheduleConstraints { get; init; } =
        ScheduleConstraints is null ? [] : [.. ScheduleConstraints];
}