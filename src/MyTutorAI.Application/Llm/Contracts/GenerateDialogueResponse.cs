using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: Structured LLM reply of the demo dialogue generator, returned unchanged as the 200 body of <c>POST /api/demo/dialogue</c>.
/// PL: Ustrukturyzowana odpowiedź LLM generatora dialogu demo, zwracana bez zmian jako body 200 <c>POST /api/demo/dialogue</c>.
/// </summary>
/// <remarks>
/// EN: Part of the structured output contract: <c>JsonPropertyName</c> and <c>Description</c> attributes go into the JSON schema imposed on the model, so changing them changes the schema.
/// PL: Część kontraktu structured output: atrybuty <c>JsonPropertyName</c> i <c>Description</c> trafiają do schematu JSON narzucanego modelowi, więc ich zmiana zmienia schemat.
/// </remarks>
/// <param name="Topic">
/// EN: Concise dialogue title produced by the model, not necessarily equal to the requested topic.
/// PL: Zwięzły tytuł dialogu wygenerowany przez model, niekoniecznie równy żądanemu tematowi.
/// </param>
/// <param name="Turns">
/// EN: Dialogue turns in speaking order; the prompt asks for at least two, but the schema does not enforce it.
/// PL: Tury dialogu w kolejności wypowiedzi; prompt wymaga co najmniej dwóch, ale schemat tego nie wymusza.
/// </param>
public sealed record DialogueGenerationResponse(
    [property: JsonPropertyName("topic")]
    [property: Description("Conversation topic")]
    string Topic,
    [property: JsonPropertyName("turns")]
    [property: Description("List of dialogue turns for the topic")]
    IReadOnlyList<DialogueTurn> Turns);
