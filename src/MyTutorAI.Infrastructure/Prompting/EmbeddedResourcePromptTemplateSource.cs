using System.Reflection;

namespace MyTutorAI.Infrastructure.Prompting;

/// <summary>
/// EN: <see cref="IPromptTemplateSource"/> that reads templates from <c>*.scriban</c> resources embedded in an assembly, so prompts ship inside the binary.
/// PL: <see cref="IPromptTemplateSource"/> czytający szablony z zasobów <c>*.scriban</c> osadzonych w assembly, dzięki czemu prompty są dostarczane wewnątrz binarki.
/// </summary>
/// <remarks>
/// EN: Resources are addressed by flat name <c>{templateName}.scriban</c>; the <c>LogicalName</c> set in <c>MyTutorAI.Infrastructure.csproj</c> makes the name independent of <c>RootNamespace</c> and directory layout. Stateless; safe to share as a singleton.
/// PL: Zasoby są adresowane płaską nazwą <c>{templateName}.scriban</c>; <c>LogicalName</c> ustawione w <c>MyTutorAI.Infrastructure.csproj</c> uniezależnia nazwę od <c>RootNamespace</c> i układu katalogów. Bezstanowy; można go współdzielić jako singleton.
/// </remarks>
public sealed class EmbeddedResourcePromptTemplateSource : IPromptTemplateSource
{
    private const string TemplateExtension = ".scriban";

    private readonly Assembly _assembly;

    /// <summary>
    /// EN: Creates a source that reads from the <c>MyTutorAI.Infrastructure</c> assembly, which embeds the prompt templates.
    /// PL: Tworzy źródło czytające z assembly <c>MyTutorAI.Infrastructure</c>, w którym osadzone są szablony promptów.
    /// </summary>
    public EmbeddedResourcePromptTemplateSource()
        : this(typeof(EmbeddedResourcePromptTemplateSource).Assembly)
    {
    }

    /// <summary>
    /// EN: Creates a source that reads embedded <c>*.scriban</c> resources from the given assembly.
    /// PL: Tworzy źródło czytające osadzone zasoby <c>*.scriban</c> z podanego assembly.
    /// </summary>
    /// <param name="assembly">
    /// EN: Assembly holding the embedded templates. Must not be null.
    /// PL: Assembly z osadzonymi szablonami. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="assembly"/> is null.
    /// PL: <paramref name="assembly"/> ma wartość null.
    /// </exception>
    public EmbeddedResourcePromptTemplateSource(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        _assembly = assembly;
    }

    /// <summary>
    /// EN: Returns the text of the embedded resource <c>{templateName}.scriban</c>.
    /// PL: Zwraca treść osadzonego zasobu <c>{templateName}.scriban</c>.
    /// </summary>
    /// <param name="templateName">
    /// EN: Template name without the <c>.scriban</c> extension. Must not be null, empty or whitespace.
    /// PL: Nazwa szablonu bez rozszerzenia <c>.scriban</c>. Nie może być null, pusta ani składać się z samych białych znaków.
    /// </param>
    /// <returns>
    /// EN: Raw template text, read as UTF-8 (a BOM is detected).
    /// PL: Surowa treść szablonu, odczytana jako UTF-8 (BOM jest wykrywany).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="templateName"/> is empty or whitespace (<see cref="ArgumentNullException"/> when null).
    /// PL: <paramref name="templateName"/> jest pusty lub składa się z białych znaków (<see cref="ArgumentNullException"/> dla null).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: No embedded resource with that name exists; the message lists the resources available in the assembly.
    /// PL: Zasób osadzony o tej nazwie nie istnieje; komunikat wymienia zasoby dostępne w assembly.
    /// </exception>
    public string Load(string templateName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        var resourceName = templateName + TemplateExtension;

        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            var available = string.Join(", ", _assembly.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Prompt template '{resourceName}' was not found in embedded resources of assembly " +
                $"'{_assembly.GetName().Name}'. Available resources: {available}.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
