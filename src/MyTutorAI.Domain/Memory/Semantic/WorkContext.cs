namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: The learner's professional context: role, company, industry, high-stakes situations and the people they speak with.
/// PL: Kontekst zawodowy ucznia: stanowisko, firma, branża, sytuacje o wysokiej stawce i osoby, z którymi rozmawia.
/// </summary>
/// <param name="Profession">
/// EN: Learner's profession or position; <c>null</c> when unknown.
/// PL: Zawód lub stanowisko ucznia; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="CompanyName">
/// EN: Learner's employer; <c>null</c> when unknown.
/// PL: Pracodawca ucznia; <c>null</c>, gdy nieznany.
/// </param>
/// <param name="Industry">
/// EN: Learner's industry; <c>null</c> when unknown.
/// PL: Branża ucznia; <c>null</c>, gdy nieznana.
/// </param>
/// <param name="HighStakesSituations">
/// EN: High-stakes situations in which the learner uses the language; copied, <c>null</c> gives an empty list.
/// PL: Sytuacje o wysokiej stawce, w których uczeń używa języka; kopiowane, <c>null</c> daje pustą listę.
/// </param>
/// <param name="Stakeholders">
/// EN: People the learner communicates with in the language; copied, <c>null</c> gives an empty list.
/// PL: Osoby, z którymi uczeń komunikuje się w tym języku; kopiowane, <c>null</c> daje pustą listę.
/// </param>
public sealed record WorkContext(
    string? Profession = null,
    string? CompanyName = null,
    string? Industry = null,
    IReadOnlyList<string>? HighStakesSituations = null,
    IReadOnlyList<string>? Stakeholders = null)
{
    /// <summary>
    /// EN: High-stakes situations in which the learner uses the language; never null.
    /// PL: Sytuacje o wysokiej stawce, w których uczeń używa języka; nigdy null.
    /// </summary>
    public IReadOnlyList<string> HighStakesSituations { get; init; } =
        HighStakesSituations is null ? [] : [.. HighStakesSituations];

    /// <summary>
    /// EN: People the learner communicates with in the language; never null.
    /// PL: Osoby, z którymi uczeń komunikuje się w tym języku; nigdy null.
    /// </summary>
    public IReadOnlyList<string> Stakeholders { get; init; } =
        Stakeholders is null ? [] : [.. Stakeholders];
}