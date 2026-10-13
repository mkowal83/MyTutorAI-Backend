using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Llm.Mapping;

/// <summary>
/// EN: Maps the LLM contract <see cref="SemanticWriteCandidate"/> to the domain input <see cref="SemanticWriteInput"/>.
/// PL: Mapuje kontrakt LLM <see cref="SemanticWriteCandidate"/> na wejście domenowe <see cref="SemanticWriteInput"/>.
/// </summary>
public static class SemanticWriteCandidateMapper
{
    /// <summary>
    /// EN: Copies every list field one-to-one; <c>null</c> lists become empty lists in the result.
    /// PL: Kopiuje każde pole listy jeden do jednego; listy <c>null</c> stają się w wyniku pustymi listami.
    /// </summary>
    /// <param name="candidate">
    /// EN: LLM candidates; must not be null.
    /// PL: Kandydaci z LLM; nie może być null.
    /// </param>
    /// <returns>
    /// EN: Domain input ready for <see cref="SemanticWriteApplier.Apply"/>.
    /// PL: Wejście domenowe gotowe dla <see cref="SemanticWriteApplier.Apply"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="candidate"/> is null.
    /// PL: <paramref name="candidate"/> jest null.
    /// </exception>
    public static SemanticWriteInput ToDomain(SemanticWriteCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return new SemanticWriteInput(
            PersonalFacts: candidate.PersonalFacts,
            RelationshipHooks: candidate.RelationshipHooks,
            UpcomingEvents: candidate.UpcomingEvents,
            WorkSituations: candidate.WorkSituations,
            Preferences: candidate.Preferences,
            ConfidenceBlockers: candidate.ConfidenceBlockers,
            MotivationDrivers: candidate.MotivationDrivers,
            VocabularyTargets: candidate.VocabularyTargets,
            ToneTargets: candidate.ToneTargets,
            RecurringWeaknesses: candidate.RecurringWeaknesses);
    }
}
