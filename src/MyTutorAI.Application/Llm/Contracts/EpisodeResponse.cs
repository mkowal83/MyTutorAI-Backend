using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: Structured output schema imposed on the LLM when a closed session is compressed into an episode; JSON names are snake_case and <c>Description</c> attributes describe each field to the model.
/// PL: Schemat structured output narzucany LLM przy kompresji zamykanej sesji do epizodu; nazwy JSON są w snake_case, a atrybuty <c>Description</c> opisują modelowi każde pole.
/// </summary>
/// <param name="Summary">
/// EN: One-sentence summary of the session; an empty value is rejected when mapped to the domain episode.
/// PL: Jednozdaniowe podsumowanie sesji; pusta wartość jest odrzucana przy mapowaniu na epizod domenowy.
/// </param>
/// <param name="PersonalFacts">
/// EN: Durable facts about the learner extracted from the whole session.
/// PL: Trwałe fakty o uczniu wyciągnięte z całej sesji.
/// </param>
/// <param name="Mistakes">
/// EN: Learner mistakes recorded during the session.
/// PL: Błędy ucznia zapisane w trakcie sesji.
/// </param>
/// <param name="NextStep">
/// EN: What to practise in the next session; empty when the model omits it.
/// PL: Co ćwiczyć w następnej sesji; puste, gdy model go pominie.
/// </param>
public sealed record EpisodeResponse(
    [property: JsonPropertyName("summary")]
    [property: Description("One-sentence summary of the session")]
    string Summary,
    [property: JsonPropertyName("personal_facts")]
    [property: Description("Durable facts about the learner extracted from the entire session")]
    IReadOnlyList<string> PersonalFacts,
    [property: JsonPropertyName("mistakes")]
    [property: Description("Learner mistakes recorded during the session")]
    IReadOnlyList<MistakeLogResponse> Mistakes,
    [property: JsonPropertyName("next_step")]
    [property: Description("What to practise next in the following session")]
    string NextStep = "");