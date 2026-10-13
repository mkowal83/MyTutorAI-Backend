using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: Structured LLM reply for one learner message in an open session: the tutor's reply, semantic profile candidates and the self-reported guardrail status.
/// PL: Ustrukturyzowana odpowiedź LLM na jedną wiadomość ucznia w otwartej sesji: odpowiedź tutora, kandydaci do profilu semantycznego i samodzielnie zgłoszony status guardraila.
/// </summary>
/// <remarks>
/// EN: Part of the structured output contract: <c>JsonPropertyName</c> and <c>Description</c> attributes go into the JSON schema imposed on the model, so changing them changes the schema.
/// PL: Część kontraktu structured output: atrybuty <c>JsonPropertyName</c> i <c>Description</c> trafiają do schematu JSON narzucanego modelowi, więc ich zmiana zmienia schemat.
/// </remarks>
/// <param name="Reply">
/// EN: Tutor's reply shown to the learner.
/// PL: Odpowiedź tutora pokazywana uczniowi.
/// </param>
/// <param name="SemanticWrite">
/// EN: Candidates for new semantic profile entries extracted from this turn.
/// PL: Kandydaci do nowych wpisów profilu semantycznego wyciągnięci z tej tury.
/// </param>
/// <param name="CoreGuardrailUsed">
/// EN: Whether the model reports that the no-homework-solving guardrail applied in this turn.
/// PL: Czy model zgłasza, że w tej turze zadziałał guardrail zakazu rozwiązywania zadań.
/// </param>
/// <param name="CoreGuardrailReason">
/// EN: Short reason of the guardrail; expected only when <paramref name="CoreGuardrailUsed"/> is <c>true</c>, otherwise may be <c>null</c>.
/// PL: Krótki powód guardraila; oczekiwany tylko, gdy <paramref name="CoreGuardrailUsed"/> ma wartość <c>true</c>, w przeciwnym razie może być <c>null</c>.
/// </param>
public sealed record LiveTutorReplyResponse(
    [property: JsonPropertyName("reply")]
    [property: Description("The tutor's reply to the learner")]
    string Reply,
    [property: JsonPropertyName("semantic_write")]
    [property: Description("Candidates for new entries in the learner's semantic memory extracted from this turn")]
    SemanticWriteCandidate SemanticWrite,
    [property: JsonPropertyName("core_guardrail_used")]
    [property: Description("Whether the NO_HOMEWORK_SOLVING guardrail was triggered in this turn")]
    bool CoreGuardrailUsed,
    [property: JsonPropertyName("core_guardrail_reason")]
    [property: Description("A short reason the guardrail was triggered, when CoreGuardrailUsed is true")]
    string? CoreGuardrailReason);