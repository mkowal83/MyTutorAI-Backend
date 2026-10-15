using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Domain.Memory.Episodic;

namespace MyTutorAI.Application.Llm.Mapping;

/// <summary>
/// EN: Maps the LLM structured output <see cref="EpisodeResponse"/> to the domain <see cref="Episode"/>.
/// PL: Mapuje structured output LLM <see cref="EpisodeResponse"/> na domenowy <see cref="Episode"/>.
/// </summary>
public static class EpisodeResponseMapper
{
    /// <summary>
    /// EN: Creates a domain episode from the LLM response; the <see cref="Episode"/> constructor trims texts and drops blank facts.
    /// PL: Tworzy epizod domenowy z odpowiedzi LLM; konstruktor <see cref="Episode"/> przycina teksty i odrzuca puste fakty.
    /// </summary>
    /// <param name="response">
    /// EN: Validated LLM response; must not be null.
    /// PL: Zwalidowana odpowiedź LLM; nie może być null.
    /// </param>
    /// <returns>
    /// EN: New episode with the response's summary, facts, mistakes and next step.
    /// PL: Nowy epizod z podsumowaniem, faktami, błędami i następnym krokiem z odpowiedzi.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="response"/> is null.
    /// PL: <paramref name="response"/> jest null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: The response summary is null, empty or whitespace.
    /// PL: Podsumowanie w odpowiedzi jest null, puste lub zawiera same białe znaki.
    /// </exception>
    public static Episode ToDomain(EpisodeResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var mistakes = response.Mistakes.Select(m => new MistakeLog(m.UserPhrase, m.Correction, m.Rule)).ToArray();

        return new Episode(
            summary: response.Summary,
            personalFacts: response.PersonalFacts,
            mistakes: mistakes,
            nextStep: response.NextStep);
    }
}