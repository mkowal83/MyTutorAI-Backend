using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: One learner mistake within the <see cref="EpisodeResponse"/> structured output schema.
/// PL: Jeden błąd ucznia w schemacie structured output <see cref="EpisodeResponse"/>.
/// </summary>
/// <param name="UserPhrase">
/// EN: Phrase used by the learner that contains the mistake.
/// PL: Fraza użyta przez ucznia, która zawiera błąd.
/// </param>
/// <param name="Correction">
/// EN: Corrected version of the phrase.
/// PL: Poprawiona wersja frazy.
/// </param>
/// <param name="Rule">
/// EN: Short name of the language rule behind the correction.
/// PL: Krótka nazwa reguły językowej stojącej za poprawką.
/// </param>
public sealed record MistakeLogResponse(
    [property: JsonPropertyName("user_phrase")]
    [property: Description("Phrase spoken by the learner that contains the mistake")]
    string UserPhrase,
    [property: JsonPropertyName("correction")]
    [property: Description("Corrected version of the phrase")]
    string Correction,
    [property: JsonPropertyName("rule")]
    [property: Description("Short name of the language rule behind the correction")]
    string Rule);