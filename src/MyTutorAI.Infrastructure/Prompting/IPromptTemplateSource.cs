namespace MyTutorAI.Infrastructure.Prompting;

/// <summary>
/// EN: Infrastructure port that supplies the raw text of a prompt template by name to <see cref="ScribanPromptRenderer"/>. It isolates reading the template (I/O) from parsing and rendering it.
/// PL: Port Infrastructure, który dostarcza surowy tekst szablonu promptu po nazwie dla <see cref="ScribanPromptRenderer"/>. Oddziela odczyt szablonu (I/O) od jego parsowania i renderowania.
/// </summary>
/// <remarks>
/// <para>
/// EN: Implementations may be called concurrently, because <see cref="ScribanPromptRenderer"/> loads a template through a concurrent cache and the factory can run in parallel for the same name. Existing implementations read embedded resources (<see cref="EmbeddedResourcePromptTemplateSource"/>) or files from disk with an embedded-resource fallback (<see cref="PhysicalFileProviderPromptTemplateSource"/>).
/// PL: Implementacje mogą być wywoływane współbieżnie, bo <see cref="ScribanPromptRenderer"/> ładuje szablon przez współbieżny cache, a jego fabryka może działać równolegle dla tej samej nazwy. Istniejące implementacje czytają zasoby osadzone (<see cref="EmbeddedResourcePromptTemplateSource"/>) albo pliki z dysku z fallbackiem do zasobów osadzonych (<see cref="PhysicalFileProviderPromptTemplateSource"/>).
/// </para>
/// <para>
/// EN: The returned text is not validated; Scriban syntax errors are reported by the renderer, which calls <see cref="Load"/> once per template name for the lifetime of its instance.
/// PL: Zwrócony tekst nie jest walidowany; błędy składni Scriban zgłasza renderer, który wywołuje <see cref="Load"/> raz na nazwę szablonu przez cały czas życia swojej instancji.
/// </para>
/// </remarks>
public interface IPromptTemplateSource
{
    /// <summary>
    /// EN: Returns the raw, unparsed text of the named prompt template.
    /// PL: Zwraca surowy, niesparsowany tekst szablonu promptu o podanej nazwie.
    /// </summary>
    /// <param name="templateName">
    /// EN: Template name without the <c>.scriban</c> extension, one of the <see cref="MyTutorAI.Application.Llm.PromptTemplates"/> constants. Must not be empty or whitespace.
    /// PL: Nazwa szablonu bez rozszerzenia <c>.scriban</c>, jedna ze stałych <see cref="MyTutorAI.Application.Llm.PromptTemplates"/>. Nie może być pusta ani składać się z białych znaków.
    /// </param>
    /// <returns>
    /// EN: The template text exactly as stored in the source.
    /// PL: Tekst szablonu dokładnie taki, jak jest zapisany w źródle.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="templateName"/> is null, empty or whitespace.
    /// PL: <paramref name="templateName"/> jest null, pusta lub składa się z białych znaków.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: No template with this name exists in the source.
    /// PL: W źródle nie istnieje szablon o tej nazwie.
    /// </exception>
    string Load(string templateName);
}
