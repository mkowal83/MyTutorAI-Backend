using Microsoft.Extensions.FileProviders;
namespace MyTutorAI.Infrastructure.Prompting;

/// <summary>
/// EN: Decorator over <see cref="IPromptTemplateSource"/> that prefers a template file on disk and falls back to another source when the file is absent. Lets template text be edited without rebuilding.
/// PL: Dekorator <see cref="IPromptTemplateSource"/>, który preferuje plik szablonu z dysku, a gdy go brak, korzysta z innego źródła. Pozwala edytować treść szablonu bez przebudowy.
/// </summary>
/// <remarks>
/// EN: Templates are looked up by flat name <c>{templateName}.scriban</c> at the root of the file provider (no subdirectories). Nothing is cached here; every call reads the file again. Registered as a singleton in DI; stateless.
/// PL: Szablony są wyszukiwane po płaskiej nazwie <c>{templateName}.scriban</c> w korzeniu file providera (bez podkatalogów). Nic nie jest tu cache'owane; każde wywołanie czyta plik ponownie. Rejestrowany jako singleton w DI; bezstanowy.
/// </remarks>
public sealed class PhysicalFileProviderPromptTemplateSource : IPromptTemplateSource
{
    private const string TemplateExtension = ".scriban";

    private readonly IFileProvider _fileProvider;
    private readonly IPromptTemplateSource _fallback;

    /// <summary>
    /// EN: Creates the decorator over a disk root and a fallback source.
    /// PL: Tworzy dekorator nad korzeniem na dysku i źródłem zapasowym.
    /// </summary>
    /// <param name="fileProvider">
    /// EN: Provider whose root is the directory holding the <c>*.scriban</c> files, typically a <see cref="PhysicalFileProvider"/> on <c>Prompts/</c> next to the assembly. Must not be null.
    /// PL: Provider, którego korzeń jest katalogiem z plikami <c>*.scriban</c>, zwykle <see cref="PhysicalFileProvider"/> na <c>Prompts/</c> obok assembly. Nie może być null.
    /// </param>
    /// <param name="fallback">
    /// EN: Source used when the file does not exist on disk or the path is a directory. Must not be null.
    /// PL: Źródło używane, gdy plik nie istnieje na dysku albo ścieżka wskazuje katalog. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="fileProvider"/> or <paramref name="fallback"/> is null.
    /// PL: <paramref name="fileProvider"/> lub <paramref name="fallback"/> ma wartość null.
    /// </exception>
    public PhysicalFileProviderPromptTemplateSource(IFileProvider fileProvider, IPromptTemplateSource fallback)
    {
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fallback);
        _fileProvider = fileProvider;
        _fallback = fallback;
    }

    /// <summary>
    /// EN: Returns the template text from <c>{templateName}.scriban</c> on disk when that file exists; otherwise delegates to the fallback source.
    /// PL: Zwraca treść szablonu z pliku <c>{templateName}.scriban</c> na dysku, gdy ten plik istnieje; w przeciwnym razie deleguje do źródła zapasowego.
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
    /// EN: The file is absent on disk and the fallback source has no such template.
    /// PL: Pliku brak na dysku, a źródło zapasowe nie zawiera takiego szablonu.
    /// </exception>
    public string Load(string templateName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        var fileInfo = _fileProvider.GetFileInfo(templateName + TemplateExtension);
        if (!fileInfo.Exists || fileInfo.IsDirectory)
        {
            return _fallback.Load(templateName);
        }

        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
