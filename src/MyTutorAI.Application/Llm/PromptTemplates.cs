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
    /// EN: Tutor system prompt for a live session, rendered from the <see cref="Domain.Memory.AgentState"/> (coach identity, learner profile, progress, recent episodes, current phase); used by <see cref="Sessions.SendMessageHandler"/>.
    /// PL: Prompt systemowy tutora dla trwającej sesji, renderowany z <see cref="Domain.Memory.AgentState"/> (tożsamość coacha, profil ucznia, postęp, ostatnie epizody, bieżąca faza); używany przez <see cref="Sessions.SendMessageHandler"/>.
    /// </summary>
    public const string TutorSystem = "tutor-system";

    /// <summary>
    /// EN: Instructions for the structured output fields of a live tutor reply (<c>semantic_write</c>, Core guardrail), appended to <see cref="TutorSystem"/> by <see cref="Sessions.SendMessageHandler"/>.
    /// PL: Instrukcje pól structured output odpowiedzi tutora w trwającej sesji (<c>semantic_write</c>, guardrail Core), dołączane do <see cref="TutorSystem"/> przez <see cref="Sessions.SendMessageHandler"/>.
    /// </summary>
    public const string TutorLiveOutputInstructions = "tutor-live-output-instructions";

    /// <summary>
    /// EN: System prompt that compresses a closed session into an episode (mistakes, personal facts).
    /// PL: Prompt systemowy kompresujący zamkniętą sesję do epizodu (błędy, fakty osobiste).
    /// </summary>
    public const string EpisodeCompressionSystem = "episode-compression-system";

    /// <summary>
    /// EN: Prompt for an LLM-generated session greeting; an alternative to the deterministic opener built by <see cref="Sessions.SessionOpener"/> without an LLM call.
    /// PL: Prompt dla powitania sesji generowanego przez LLM; alternatywa dla deterministycznego otwarcia budowanego przez <see cref="Sessions.SessionOpener"/> bez wywołania LLM.
    /// </summary>
    public const string SessionOpener = "session-opener";

    /// <summary>
    /// EN: System prompt of the standalone example-dialogue generator (not tied to a specific student's memory).
    /// PL: Prompt systemowy generatora przykładowego dialogu (niezwiązanego z pamięcią konkretnego ucznia).
    /// </summary>
    public const string DialogueGeneration = "dialogue-generation-system";
}
