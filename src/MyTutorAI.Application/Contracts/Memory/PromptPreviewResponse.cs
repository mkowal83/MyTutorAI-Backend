namespace MyTutorAI.Application.Contracts.Memory;

/// <summary>
/// EN: Response of <c>GET /api/memory/prompt-preview</c>: the system prompt rendered from the current user's memory, without calling the LLM.
/// PL: Odpowiedź <c>GET /api/memory/prompt-preview</c>: prompt systemowy wyrenderowany z pamięci bieżącego użytkownika, bez wywołania LLM.
/// </summary>
/// <param name="TemplateName">
/// EN: Name of the rendered template; currently always <c>tutor-system</c>.
/// PL: Nazwa wyrenderowanego szablonu; obecnie zawsze <c>tutor-system</c>.
/// </param>
/// <param name="RenderedPrompt">
/// EN: Rendered prompt, truncated to 1800 characters with a truncation marker appended when longer.
/// PL: Wyrenderowany prompt, przycięty do 1800 znaków z doklejonym znacznikiem ucięcia, gdy jest dłuższy.
/// </param>
public sealed record PromptPreviewResponse(string TemplateName, string RenderedPrompt);

