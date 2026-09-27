namespace MyTutorAI.Application.Contracts;

/// <summary>
/// EN: Input of the demo example-dialogue generator; body of <c>POST /api/demo/dialogue</c>.
/// PL: Dane wejściowe demonstracyjnego generatora przykładowego dialogu; body <c>POST /api/demo/dialogue</c>.
/// </summary>
/// <param name="Topic">
/// EN: Dialogue topic; must not be empty and is limited to <see cref="Demo.DialogueGenerationRequestValidator.TopicMaxLength"/> characters, and is trimmed before being sent to the LLM.
/// PL: Temat dialogu; nie może być pusty, jest ograniczony do <see cref="Demo.DialogueGenerationRequestValidator.TopicMaxLength"/> znaków i jest przycinany przed wysłaniem do LLM.
/// </param>
public sealed record DialogueGenerationRequest(string Topic);
