using MyTutorAI.Application.Contracts.Sessions.Dtos;
using MyTutorAI.Domain.Memory.Episodic;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTutorAI.Application.Llm.Mapping;

/// <summary>
/// EN: Maps domain episodes to the API DTO <see cref="EpisodeDto"/>.
/// PL: Mapuje epizody domenowe na DTO API <see cref="EpisodeDto"/>.
/// </summary>
public static class EpisodeMapper
{
    /// <summary>
    /// EN: Creates the API view of a domain episode; the lists are passed through without copying.
    /// PL: Tworzy widok API epizodu domenowego; listy są przekazywane bez kopiowania.
    /// </summary>
    /// <param name="episode">
    /// EN: Episode to map; must not be null.
    /// PL: Epizod do zmapowania; nie może być null.
    /// </param>
    /// <returns>
    /// EN: DTO with the same summary, personal facts, mistakes and next step.
    /// PL: DTO z tym samym podsumowaniem, faktami osobistymi, błędami i następnym krokiem.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="episode"/> is null.
    /// PL: <paramref name="episode"/> jest null.
    /// </exception>
    public static EpisodeDto ToDto(Episode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);

        return new EpisodeDto(
            episode.Summary,
            episode.PersonalFacts,
            episode.Mistakes,
            episode.NextStep);
    }
}