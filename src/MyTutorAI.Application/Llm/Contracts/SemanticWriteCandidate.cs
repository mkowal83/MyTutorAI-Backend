using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: Semantic profile candidates returned by the LLM as part of <see cref="LiveTutorReplyResponse"/>; each list may be empty and is mapped to the domain by <see cref="Mapping.SemanticWriteCandidateMapper"/>.
/// PL: Kandydaci do profilu semantycznego zwracani przez LLM w ramach <see cref="LiveTutorReplyResponse"/>; każda lista może być pusta i jest mapowana do domeny przez <see cref="Mapping.SemanticWriteCandidateMapper"/>.
/// </summary>
/// <remarks>
/// EN: Part of the structured output contract: <c>JsonPropertyName</c> and <c>Description</c> attributes go into the JSON schema imposed on the model, so changing them changes the schema.
/// PL: Część kontraktu structured output: atrybuty <c>JsonPropertyName</c> i <c>Description</c> trafiają do schematu JSON narzucanego modelowi, więc ich zmiana zmienia schemat.
/// </remarks>
/// <param name="PersonalFacts">
/// EN: New durable facts about the learner.
/// PL: Nowe trwałe fakty o uczniu.
/// </param>
/// <param name="RelationshipHooks">
/// EN: Small details for naturally opening the next conversation.
/// PL: Drobne szczegóły do naturalnego otwarcia kolejnej rozmowy.
/// </param>
/// <param name="UpcomingEvents">
/// EN: Upcoming high-stakes events mentioned by the learner.
/// PL: Nadchodzące wydarzenia wysokiej stawki wspomniane przez ucznia.
/// </param>
/// <param name="WorkSituations">
/// EN: High-stakes work situations mentioned by the learner.
/// PL: Sytuacje zawodowe wysokiej stawki wspomniane przez ucznia.
/// </param>
/// <param name="Preferences">
/// EN: Preferences for working style or pace.
/// PL: Preferencje dotyczące stylu pracy lub tempa.
/// </param>
/// <param name="ConfidenceBlockers">
/// EN: Stressors and confidence blockers.
/// PL: Stresory i blokady pewności siebie.
/// </param>
/// <param name="MotivationDrivers">
/// EN: Reasons the learner cares about learning.
/// PL: Powody, dla których uczniowi zależy na nauce.
/// </param>
/// <param name="VocabularyTargets">
/// EN: Vocabulary or language areas for further practice.
/// PL: Słownictwo lub obszary językowe do dalszego ćwiczenia.
/// </param>
/// <param name="ToneTargets">
/// EN: How the learner wants to sound in the target language.
/// PL: Jak uczeń chce brzmieć w języku docelowym.
/// </param>
/// <param name="RecurringWeaknesses">
/// EN: Recurring communication or language weaknesses.
/// PL: Powtarzające się słabości komunikacyjne lub językowe.
/// </param>
public sealed record SemanticWriteCandidate(
    [property: JsonPropertyName("personal_facts")]
    [property: Description("New durable facts about the learner extracted from the conversation")]
    IReadOnlyList<string> PersonalFacts,
    [property: JsonPropertyName("relationship_hooks")]
    [property: Description("Small details for naturally opening the next conversation")]
    IReadOnlyList<string> RelationshipHooks,
    [property: JsonPropertyName("upcoming_events")]
    [property: Description("Upcoming high-stakes events mentioned by the learner")]
    IReadOnlyList<string> UpcomingEvents,
    [property: JsonPropertyName("work_situations")]
    [property: Description("High-stakes work situations mentioned by the learner")]
    IReadOnlyList<string> WorkSituations,
    [property: JsonPropertyName("preferences")]
    [property: Description("The learner's preferences for working style or pace")]
    IReadOnlyList<string> Preferences,
    [property: JsonPropertyName("confidence_blockers")]
    [property: Description("The learner's stressors and confidence blockers")]
    IReadOnlyList<string> ConfidenceBlockers,
    [property: JsonPropertyName("motivation_drivers")]
    [property: Description("Reasons the learner cares about learning")]
    IReadOnlyList<string> MotivationDrivers,
    [property: JsonPropertyName("vocabulary_targets")]
    [property: Description("Vocabulary or language areas for further practice")]
    IReadOnlyList<string> VocabularyTargets,
    [property: JsonPropertyName("tone_targets")]
    [property: Description("How the learner wants to sound in the target language")]
    IReadOnlyList<string> ToneTargets,
    [property: JsonPropertyName("recurring_weaknesses")]
    [property: Description("The learner's recurring communication or language weaknesses")]
    IReadOnlyList<string> RecurringWeaknesses);