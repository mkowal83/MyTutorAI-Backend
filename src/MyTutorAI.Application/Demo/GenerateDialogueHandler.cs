using Microsoft.Extensions.Options;
using MyTutorAI.Application.Contracts;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Application.Options;

namespace MyTutorAI.Application.Demo;

/// <summary>
/// EN: Use case generating a demo practice dialogue in English on a given topic through the LLM. Not tied to any student's memory and persists nothing.
/// PL: Przypadek użycia generujący przez LLM demonstracyjny dialog ćwiczeniowy po angielsku na zadany temat. Niezwiązany z pamięcią żadnego ucznia i niczego nie zapisuje.
/// </summary>
public sealed class GenerateDialogueHandler
{
    private readonly IPromptRenderer _promptRenderer;
    private readonly IStructuredOutputClient _llm;
    private readonly bool _allowDialogue;

    /// <summary>
    /// EN: Creates the handler; the dialogue feature flag is read once here, so later option changes do not affect this instance.
    /// PL: Tworzy handler; flaga funkcji dialogu jest odczytywana tu jednorazowo, więc późniejsze zmiany opcji nie wpływają na tę instancję.
    /// </summary>
    /// <param name="promptRenderer">
    /// EN: Renders the system prompt from the <see cref="PromptTemplates.DialogueGeneration"/> template.
    /// PL: Renderuje prompt systemowy z szablonu <see cref="PromptTemplates.DialogueGeneration"/>.
    /// </param>
    /// <param name="llm">
    /// EN: Structured output client used to obtain the dialogue.
    /// PL: Klient structured output używany do uzyskania dialogu.
    /// </param>
    /// <param name="features">
    /// EN: Feature flags; <see cref="FeaturesOptions.AllowDialogue"/> enables the use case.
    /// PL: Flagi funkcji; <see cref="FeaturesOptions.AllowDialogue"/> włącza przypadek użycia.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GenerateDialogueHandler(
        IPromptRenderer promptRenderer,
        IStructuredOutputClient llm,
        IOptions<FeaturesOptions> features)
    {
        ArgumentNullException.ThrowIfNull(promptRenderer);
        ArgumentNullException.ThrowIfNull(llm);
        ArgumentNullException.ThrowIfNull(features);

        _promptRenderer = promptRenderer;
        _llm = llm;
        _allowDialogue = features.Value.AllowDialogue;
    }

    /// <summary>
    /// EN: Generates a practice dialogue for the trimmed topic of <paramref name="request"/> with a single structured output call using default request options.
    /// PL: Generuje dialog ćwiczeniowy dla przyciętego tematu z <paramref name="request"/> jednym wywołaniem structured output z domyślnymi opcjami żądania.
    /// </summary>
    /// <param name="request">
    /// EN: Validated request with the dialogue topic; must not be null.
    /// PL: Zwalidowane żądanie z tematem dialogu; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the LLM call; cancellation surfaces as <see cref="OperationCanceledException"/>.
    /// PL: Anuluje wywołanie LLM; anulowanie objawia się jako <see cref="OperationCanceledException"/>.
    /// </param>
    /// <returns>
    /// EN: Dialogue returned by the model and parsed against the <see cref="DialogueGenerationResponse"/> schema.
    /// PL: Dialog zwrócony przez model i sparsowany zgodnie ze schematem <see cref="DialogueGenerationResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="FeatureDisabledException">
    /// EN: <see cref="FeaturesOptions.AllowDialogue"/> is false.
    /// PL: <see cref="FeaturesOptions.AllowDialogue"/> ma wartość false.
    /// </exception>
    /// <exception cref="LlmException">
    /// EN: The LLM does not return a schema-conforming reply within the allowed attempts.
    /// PL: LLM nie zwraca odpowiedzi zgodnej ze schematem w dozwolonej liczbie prób.
    /// </exception>
    public async Task<DialogueGenerationResponse> HandleAsync(
        DialogueGenerationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_allowDialogue)
        {
            throw new FeatureDisabledException("dialogue");
        }

        var topic = request.Topic.Trim();

        var prompts = new List<PromptMessage>(capacity: 2)
        {
            new(PromptRole.System, _promptRenderer.Render(PromptTemplates.DialogueGeneration, request)),
            new(PromptRole.User, topic),
        };

        return await _llm.CompleteAsync<DialogueGenerationResponse>(
            prompts, StructuredOutputRequestOptions.Default, cancellationToken);
    }
}
