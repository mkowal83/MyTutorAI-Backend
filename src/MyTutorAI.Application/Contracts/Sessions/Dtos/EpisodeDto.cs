using MyTutorAI.Domain.Memory.Episodic;

namespace MyTutorAI.Application.Contracts.Sessions.Dtos;

/// <summary>
/// EN: API view of one episode: the compressed record of a completed session.
/// PL: Widok API jednego epizodu: skompresowanego zapisu zakończonej sesji.
/// </summary>
/// <param name="Summary">
/// EN: One-sentence summary of the session; never empty.
/// PL: Jednozdaniowe podsumowanie sesji; nigdy nie jest puste.
/// </param>
/// <param name="PersonalFacts">
/// EN: Facts about the learner extracted from the session; may be empty.
/// PL: Fakty o uczniu wyciągnięte z sesji; lista może być pusta.
/// </param>
/// <param name="Mistakes">
/// EN: Learner mistakes recorded during the session; may be empty.
/// PL: Błędy ucznia zapisane w trakcie sesji; lista może być pusta.
/// </param>
/// <param name="NextStep">
/// EN: What to practise in the next session; <see cref="string.Empty"/> when the LLM gave none.
/// PL: Co ćwiczyć w następnej sesji; <see cref="string.Empty"/>, gdy LLM go nie podał.
/// </param>
public sealed record EpisodeDto(
    string Summary,
    IReadOnlyList<string> PersonalFacts,
    IReadOnlyList<MistakeLog> Mistakes,
    string NextStep);

