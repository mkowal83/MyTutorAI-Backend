using MyTutorAI.Application.Llm;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using System.Collections.Concurrent;

namespace MyTutorAI.Infrastructure.Prompting;

/// <summary>
/// EN: Infrastructure adapter of <see cref="IPromptRenderer"/> that renders prompt templates by name with the Scriban engine. Each template is loaded through <see cref="IPromptTemplateSource"/> and parsed once, then reused from an in-memory cache.
/// PL: Adapter Infrastructure portu <see cref="IPromptRenderer"/>, który renderuje szablony promptów po nazwie silnikiem Scriban. Każdy szablon jest ładowany przez <see cref="IPromptTemplateSource"/> i parsowany raz, potem używany z cache w pamięci.
/// </summary>
/// <remarks>
/// <para>
/// EN: The model passed to <c>Render</c> is exposed to the template as <c>state</c> (for example <c>state.profile.native_language</c>), regardless of <c>TModel</c>. .NET member names are converted from PascalCase to snake_case by the default Scriban member renamer.
/// PL: Model przekazany do <c>Render</c> jest widoczny w szablonie jako <c>state</c> (np. <c>state.profile.native_language</c>), niezależnie od <c>TModel</c>. Nazwy składowych .NET są konwertowane z PascalCase na snake_case przez domyślny member renamer Scribana.
/// </para>
/// <para>
/// EN: A parsed template is cached per template name for the lifetime of the instance, so later changes to the template source are not visible until the instance is recreated (application restart). A template with syntax errors is never cached; the next call loads and validates it again.
/// PL: Sparsowany szablon jest cache'owany per nazwa szablonu przez cały czas życia instancji, więc późniejsze zmiany w źródle szablonu są widoczne dopiero po odtworzeniu instancji (restart aplikacji). Szablon z błędami składni nigdy nie trafia do cache; kolejne wywołanie ładuje go i waliduje ponownie.
/// </para>
/// <para>
/// EN: Thread-safe: every call renders with its own <see cref="TemplateContext"/>, and the shared cache is a <see cref="ConcurrentDictionary{TKey, TValue}"/>. Registered as a DI singleton.
/// PL: Thread-safe: każde wywołanie renderuje we własnym <see cref="TemplateContext"/>, a współdzielony cache to <see cref="ConcurrentDictionary{TKey, TValue}"/>. Rejestrowany w DI jako singleton.
/// </para>
/// </remarks>
public sealed class ScribanPromptRenderer : IPromptRenderer
{
    private readonly IPromptTemplateSource _templateSource;
    private readonly ConcurrentDictionary<string, Template> _parsedTemplates = new(StringComparer.Ordinal);

    /// <summary>
    /// EN: Creates a renderer that reads raw template text from the given source.
    /// PL: Tworzy renderer, który czyta surową treść szablonów z podanego źródła.
    /// </summary>
    /// <param name="templateSource">
    /// EN: Provider of raw template text by name. Must not be null.
    /// PL: Dostawca surowej treści szablonów po nazwie. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="templateSource"/> is null.
    /// PL: <paramref name="templateSource"/> jest null.
    /// </exception>
    public ScribanPromptRenderer(IPromptTemplateSource templateSource)
    {
        ArgumentNullException.ThrowIfNull(templateSource);
        _templateSource = templateSource;
    }

    /// <summary>
    /// EN: Renders the named prompt template with <paramref name="model"/> available to the template as <c>state</c>. The result is the final prompt text sent to the LLM or shown as a preview.
    /// PL: Renderuje szablon promptu o podanej nazwie z modelem <paramref name="model"/> dostępnym w szablonie jako <c>state</c>. Wynik to końcowy tekst promptu wysyłany do LLM lub pokazywany jako podgląd.
    /// </summary>
    /// <typeparam name="TModel">
    /// EN: Reference type of the template context model; templates that do not read <c>state</c> ignore it.
    /// PL: Typ referencyjny modelu kontekstu szablonu; szablony, które nie czytają <c>state</c>, go ignorują.
    /// </typeparam>
    /// <param name="templateName">
    /// EN: Template name without the <c>.scriban</c> extension (see <c>PromptTemplates</c>). Must not be empty or whitespace.
    /// PL: Nazwa szablonu bez rozszerzenia <c>.scriban</c> (patrz <c>PromptTemplates</c>). Nie może być pusta ani składać się z białych znaków.
    /// </param>
    /// <param name="model">
    /// EN: Context model exposed to the template as <c>state</c>. Must not be null.
    /// PL: Model kontekstu udostępniany szablonowi jako <c>state</c>. Nie może być null.
    /// </param>
    /// <returns>
    /// EN: The rendered prompt text.
    /// PL: Wyrenderowany tekst promptu.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="model"/> is null.
    /// PL: <paramref name="model"/> jest null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="templateName"/> is null, empty or whitespace.
    /// PL: <paramref name="templateName"/> jest null, pusta lub składa się z białych znaków.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: The template does not exist in the source, it has Scriban syntax errors (the message lists them), or rendering fails because the template reads an undefined variable or member (strict variables are on).
    /// PL: Szablon nie istnieje w źródle, ma błędy składni Scriban (komunikat je wymienia) albo renderowanie zawodzi, bo szablon czyta niezdefiniowaną zmienną lub składową (włączone strict variables).
    /// </exception>
    public string Render<TModel>(string templateName, TModel model)
        where TModel : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(model);

        var template = _parsedTemplates.GetOrAdd(templateName, ParseAndValidate);

        var scriptObject = new ScriptObject();
        scriptObject.Import(new { state = model });

        var context = new TemplateContext { StrictVariables = true };
        context.PushGlobal(scriptObject);

        try
        {
            return template.Render(context);
        }
        catch (ScriptRuntimeException exception)
        {
            throw new InvalidOperationException(
                $"Rendering prompt template '{templateName}' failed: {exception.Message}",
                exception);
        }
    }

    private Template ParseAndValidate(string templateName)
    {
        var source = _templateSource.Load(templateName);
        var template = Template.Parse(source, templateName);

        if (template.HasErrors)
        {
            var messages = string.Join("; ", template.Messages.Select(message => message.Message));
            throw new InvalidOperationException(
                $"Prompt template '{templateName}' has Scriban syntax errors: {messages}");
        }

        return template;
    }
}
