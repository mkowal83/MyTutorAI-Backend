using MyTutorAI.Domain.Tools;

namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Merges a semantic write extracted from a conversation turn into the learner's semantic profile, without overwriting existing entries and without duplicates.
/// PL: Scala zapis semantyczny wyciągnięty z tury rozmowy z profilem semantycznym ucznia, bez nadpisywania istniejących wpisów i bez duplikatów.
/// </summary>
public static class SemanticWriteApplier
{

    /// <summary>
    /// EN: Appends each list of <paramref name="write"/> to the matching list of <see cref="AgentState.Profile"/> via <see cref="TextMerge.MergeUnique"/>, trimming each list to its limit (newest entries kept), and then refreshes <see cref="StudentProfile.ContextNotes"/>. Mutates the profile in place; nothing is persisted.
    /// PL: Dołącza każdą listę z <paramref name="write"/> do odpowiadającej listy w <see cref="AgentState.Profile"/> przez <see cref="TextMerge.MergeUnique"/>, przycinając każdą listę do jej limitu (zostają najnowsze wpisy), a następnie odświeża <see cref="StudentProfile.ContextNotes"/>. Modyfikuje profil w miejscu; niczego nie utrwala.
    /// </summary>
    /// <param name="state">
    /// EN: Agent state whose profile is updated; must not be null.
    /// PL: Stan agenta, którego profil jest aktualizowany; nie może być null.
    /// </param>
    /// <param name="write">
    /// EN: Candidate entries to merge; an empty list leaves the matching profile list unchanged apart from trimming to the limit.
    /// PL: Wpisy kandydujące do scalenia; pusta lista zostawia odpowiadającą listę profilu bez zmian poza przycięciem do limitu.
    /// </param>
    /// <param name="limits">
    /// EN: Per-list size limits; <c>null</c> uses <see cref="SemanticWriteLimits.Default"/>.
    /// PL: Limity rozmiaru per lista; <c>null</c> oznacza <see cref="SemanticWriteLimits.Default"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> or <paramref name="write"/> is null.
    /// PL: <paramref name="state"/> lub <paramref name="write"/> jest null.
    /// </exception>
    public static void Apply(AgentState state, SemanticWriteInput write, SemanticWriteLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(write);
        limits ??= SemanticWriteLimits.Default;

        var profile = state.Profile;

        profile.SetPersonalContext(profile.PersonalContext with
        {
            PersonalFacts = TextMerge.MergeUnique(
                profile.PersonalContext.PersonalFacts, write.PersonalFacts, limits.PersonalFacts),
            RelationshipHooks = TextMerge.MergeUnique(
                profile.PersonalContext.RelationshipHooks, write.RelationshipHooks, limits.RelationshipHooks),
            UpcomingEvents = TextMerge.MergeUnique(
                profile.PersonalContext.UpcomingEvents, write.UpcomingEvents, limits.UpcomingEvents),
        });

        profile.SetWorkContext(profile.WorkContext with
        {
            HighStakesSituations = TextMerge.MergeUnique(
                profile.WorkContext.HighStakesSituations, write.WorkSituations, limits.WorkSituations),
        });

        profile.SetPreferences(TextMerge.MergeUnique(
            profile.Preferences, write.Preferences, limits.Preferences));

        profile.SetSemanticSignals(profile.SemanticSignals with
        {
            ConfidenceBlockers = TextMerge.MergeUnique(
                profile.SemanticSignals.ConfidenceBlockers, write.ConfidenceBlockers, limits.ConfidenceBlockers),
            MotivationDrivers = TextMerge.MergeUnique(
                profile.SemanticSignals.MotivationDrivers, write.MotivationDrivers, limits.MotivationDrivers),
            VocabularyTargets = TextMerge.MergeUnique(
                profile.SemanticSignals.VocabularyTargets, write.VocabularyTargets, limits.VocabularyTargets),
        });

        profile.SetPerformanceProfile(profile.PerformanceProfile with
        {
            ToneTargets = TextMerge.MergeUnique(
                profile.PerformanceProfile.ToneTargets, write.ToneTargets, limits.ToneTargets),
            RecurringWeaknesses = TextMerge.MergeUnique(
                profile.PerformanceProfile.RecurringWeaknesses, write.RecurringWeaknesses, limits.RecurringWeaknesses),
        });

        RefreshContextSummary(profile);
    }

    /// <summary>
    /// EN: Rebuilds <see cref="StudentProfile.ContextNotes"/> from the newest entry of selected profile lists (relationship hooks, upcoming events, confidence blockers, high-stakes work situations, vocabulary targets, tone targets), joined with <c> | </c>; at most five entries are used. Leaves the notes unchanged when all those lists are empty.
    /// PL: Odbudowuje <see cref="StudentProfile.ContextNotes"/> z najnowszego wpisu wybranych list profilu (haczyki relacyjne, nadchodzące wydarzenia, blokady pewności siebie, sytuacje zawodowe wysokiej stawki, cele słownictwa, docelowy ton), połączonych przez <c> | </c>; użytych jest najwyżej pięć wpisów. Nie zmienia notatek, gdy wszystkie te listy są puste.
    /// </summary>
    /// <param name="profile">
    /// EN: Profile whose context notes are refreshed.
    /// PL: Profil, którego notatki kontekstowe są odświeżane.
    /// </param>
    internal static void RefreshContextSummary(StudentProfile profile)
    {
        var bits = new List<string>(6);
        AppendLast(bits, profile.PersonalContext.RelationshipHooks);
        AppendLast(bits, profile.PersonalContext.UpcomingEvents);
        AppendLast(bits, profile.SemanticSignals.ConfidenceBlockers);
        AppendLast(bits, profile.WorkContext.HighStakesSituations);
        AppendLast(bits, profile.SemanticSignals.VocabularyTargets);
        AppendLast(bits, profile.PerformanceProfile.ToneTargets);

        if (bits.Count == 0)
        {
            return;
        }

        var tail = bits.Count <= 5 ? bits : bits.GetRange(bits.Count - 5, 5);
        profile.SetContextNotes(string.Join(" | ", tail));
    }

    private static void AppendLast(List<string> bits, IReadOnlyList<string> source)
    {
        if (source.Count > 0)
        {
            bits.Add(source[^1]);
        }
    }
}
