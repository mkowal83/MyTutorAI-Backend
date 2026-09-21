namespace MyTutorAI.Application.Llm;

/// <summary>
/// EN: Names of the prompt templates known to the application, passed to <see cref="IPromptRenderer.Render{TModel}"/>. Each constant is the template file name without the <c>.scriban</c> extension.
/// PL: Nazwy szablonów promptów znanych aplikacji, przekazywane do <see cref="IPromptRenderer.Render{TModel}"/>. Każda stała to nazwa pliku szablonu bez rozszerzenia <c>.scriban</c>.
/// </summary>
/// <remarks>
/// EN: A constant must match an existing <c>Prompts/{name}.scriban</c> file in Infrastructure; a missing template surfaces at render time as <see cref="InvalidOperationException"/>.
/// PL: Stała musi odpowiadać istniejącemu plikowi <c>Prompts/{name}.scriban</c> w Infrastructure; brak szablonu ujawnia się w czasie renderowania jako <see cref="InvalidOperationException"/>.
/// </remarks>
public static class PromptTemplates
{
    /// <summary>
    /// EN: System prompt of the standalone example-dialogue generator (not tied to a specific student's memory).
    /// PL: Prompt systemowy generatora przykładowego dialogu (niezwiązanego z pamięcią konkretnego ucznia).
    /// </summary>
    public const string DialogueGenerationSystem = "dialogue-generation-system";
}
