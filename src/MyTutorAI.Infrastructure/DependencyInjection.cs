using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MyTutorAI.Application.Llm;
using MyTutorAI.Infrastructure.Llm;
using MyTutorAI.Infrastructure.Prompting;

namespace MyTutorAI.Infrastructure;

/// <summary>
/// EN: Dependency injection registration of Infrastructure services, called from the Api composition root.
/// PL: Rejestracja usług Infrastructure w DI, wywoływana z korzenia kompozycji w Api.
/// </summary>
public static class DependencyInjection
{
    private const string PromptsDirectoryName = "Prompts";

    /// <summary>
    /// EN: Registers <see cref="IPromptRenderer"/> (Scriban) and its template source as singletons. With <paramref name="useDiskTemplates"/> templates are read from the <c>Prompts</c> directory next to the assembly, falling back to the embedded resources when a file or the directory is missing; otherwise only the embedded resources are used.
    /// PL: Rejestruje <see cref="IPromptRenderer"/> (Scriban) i jego źródło szablonów jako singletony. Przy <paramref name="useDiskTemplates"/> szablony są czytane z katalogu <c>Prompts</c> obok assembly, z fallbackiem do zasobów osadzonych, gdy brakuje pliku lub katalogu; w przeciwnym razie używane są tylko zasoby osadzone.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <param name="useDiskTemplates">
    /// EN: True in Development to allow editing templates without rebuilding (a restart is still needed because parsed templates are cached).
    /// PL: True w Development, aby edytować szablony bez przebudowy (restart nadal jest potrzebny, bo sparsowane szablony są cache'owane).
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddPromptRendering(this IServiceCollection services, bool useDiskTemplates)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPromptTemplateSource>(_ =>
        {
            var embedded = new EmbeddedResourcePromptTemplateSource();
            var promptsDirectory = Path.Combine(AppContext.BaseDirectory, PromptsDirectoryName);

            return useDiskTemplates && Directory.Exists(promptsDirectory)
                ? new PhysicalFileProviderPromptTemplateSource(new PhysicalFileProvider(promptsDirectory), embedded)
                : embedded;
        });
        services.AddSingleton<IPromptRenderer, ScribanPromptRenderer>();

        return services;
    }

    /// <summary>
    /// EN: Registers <see cref="IStructuredOutputClient"/> as a typed <see cref="HttpClient"/> client. <c>LlmOptions</c> must be bound by the caller; the HTTP resilience pipeline is not configured yet.
    /// PL: Rejestruje <see cref="IStructuredOutputClient"/> jako typowanego klienta <see cref="HttpClient"/>. <c>LlmOptions</c> musi podpiąć wywołujący; pipeline odporności HTTP nie jest jeszcze skonfigurowany.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddLlmClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IStructuredOutputClient, StructuredOutputClient>();

        return services;
    }
}
