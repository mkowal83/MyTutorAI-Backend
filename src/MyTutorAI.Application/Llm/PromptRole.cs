namespace MyTutorAI.Application.Llm;

/// <summary>
/// EN: Sender role of a <see cref="PromptMessage"/> sent to the LLM through <see cref="IStructuredOutputClient"/>.
/// PL: Rola nadawcy <see cref="PromptMessage"/> wysyłanej do LLM przez <see cref="IStructuredOutputClient"/>.
/// </summary>
/// <remarks>
/// EN: Deliberately a separate type from <see cref="MyTutorAI.Domain.Memory.Working.ChatRole"/> (different value order: there <c>User=0</c>/<c>Assistant=1</c>/<c>System=2</c>) — Application knows the LLM transport, Domain does not. Callers map <c>Domain.ChatRole</c> to <see cref="PromptRole"/> by hand when building the <see cref="PromptMessage"/> list from <c>WorkingMemory.GetContextWindow</c>.
/// PL: Świadomie osobny typ od <see cref="MyTutorAI.Domain.Memory.Working.ChatRole"/> (inna kolejność wartości: tam <c>User=0</c>/<c>Assistant=1</c>/<c>System=2</c>) — Application zna transport do LLM, Domain o tym nic nie wie. Wywołujący mapują <c>Domain.ChatRole</c> na <see cref="PromptRole"/> ręcznie przy budowaniu listy <see cref="PromptMessage"/> z <c>WorkingMemory.GetContextWindow</c>.
/// </remarks>
public enum PromptRole
{
    /// <summary>
    /// EN: System-level instructions (e.g. the rendered <c>tutor-system</c> prompt) — sent first, sets the model's behavior for the conversation.
    /// PL: Instrukcje systemowe (np. wyrenderowany prompt <c>tutor-system</c>) — wysyłane jako pierwsze, ustawiają zachowanie modelu na czas konwersacji.
    /// </summary>
    System = 0,

    /// <summary>
    /// EN: A message from the end user (student) or, for handlers that feed transcripts to the LLM, user-authored content such as a compression transcript.
    /// PL: Wiadomość od użytkownika końcowego (ucznia) lub, w handlerach przekazujących transkrypty do LLM, treść autorstwa użytkownika, np. transkrypt do kompresji.
    /// </summary>
    User = 1,

    /// <summary>
    /// EN: A message previously produced by the model, replayed as conversation history.
    /// PL: Wiadomość wcześniej wygenerowana przez model, odtwarzana jako historia konwersacji.
    /// </summary>
    Assistant = 2,
}