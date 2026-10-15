using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTutorAI.Domain.Memory.Episodic;

/// <summary>
/// EN: Applies the episode of a closed session to the learner's memory, so the next session starts aware of it.
/// PL: Nanosi epizod zamkniętej sesji na pamięć ucznia, aby następna sesja zaczynała się ze świadomością poprzedniej.
/// </summary>
public static class EpisodeApplier
{
    /// <summary>
    /// EN: Maximum number of episode mistakes copied into <see cref="LearningProgress.RecentMistakes"/>; the first ones from the episode's list are taken.
    /// PL: Maksymalna liczba błędów z epizodu kopiowanych do <see cref="LearningProgress.RecentMistakes"/>; brane są pierwsze z listy epizodu.
    /// </summary>
    public const int RecentMistakesLimit = 5;

    /// <summary>
    /// EN: Adds the episode to episodic memory (oldest episodes above the limit are dropped), replaces recent mistakes, sets the next focus and assigned homework when a next step is given, and merges personal facts without duplicates.
    /// PL: Dodaje epizod do pamięci epizodycznej (najstarsze epizody ponad limit są usuwane), zastępuje ostatnie błędy, ustawia fokus następnej sesji i przydzielone zadanie domowe, gdy podano następny krok, oraz scala fakty osobiste bez duplikatów.
    /// </summary>
    /// <remarks>
    /// EN: Mutates <paramref name="state"/> in place and persists nothing. <see cref="StudentProfile.ContextNotes"/> is refreshed only when the episode has personal facts.
    /// PL: Modyfikuje <paramref name="state"/> w miejscu i niczego nie utrwala. <see cref="StudentProfile.ContextNotes"/> jest odświeżane tylko wtedy, gdy epizod ma fakty osobiste.
    /// </remarks>
    /// <param name="state">
    /// EN: Learner's memory, modified in place.
    /// PL: Pamięć ucznia, modyfikowana w miejscu.
    /// </param>
    /// <param name="episode">
    /// EN: Compressed record of the closed session.
    /// PL: Skompresowany zapis zamkniętej sesji.
    /// </param>
    /// <param name="limits">
    /// EN: Limits of the merged lists, of which only <see cref="SemanticWriteLimits.PersonalFacts"/> is used; <c>null</c> means <see cref="SemanticWriteLimits.Default"/>.
    /// PL: Limity scalanych list, z których używany jest tylko <see cref="SemanticWriteLimits.PersonalFacts"/>; <c>null</c> oznacza <see cref="SemanticWriteLimits.Default"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> or <paramref name="episode"/> is <c>null</c>.
    /// PL: <paramref name="state"/> lub <paramref name="episode"/> jest <c>null</c>.
    /// </exception>
    public static void Apply(AgentState state, Episode episode, SemanticWriteLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(episode);
        limits ??= SemanticWriteLimits.Default;

        state.Episodic.Add(episode);

        var mistakesAsText = episode.Mistakes
            .Take(RecentMistakesLimit)
            .Select(m => $"{m.UserPhrase} -> {m.Correction} ({m.Rule})");
        state.Progress.SetRecentMistakes(mistakesAsText);

        if (episode.NextStep is { Length: > 0 })
        {
            state.Progress.SetNextFocus(episode.NextStep);
            state.Progress.SetHomeworkStatus(HomeworkStatus.Assigned);
        }

        if (episode.PersonalFacts.Count > 0)
        {
            state.Profile.SetPersonalContext(state.Profile.PersonalContext with
            {
                PersonalFacts = TextMerge.MergeUnique(
                    state.Profile.PersonalContext.PersonalFacts,
                    episode.PersonalFacts,
                    limits.PersonalFacts),
            });

            SemanticWriteApplier.RefreshContextSummary(state.Profile);
        }
    }
}
