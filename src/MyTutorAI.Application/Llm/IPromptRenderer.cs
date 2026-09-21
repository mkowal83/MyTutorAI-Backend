namespace MyTutorAI.Application.Llm;


/// <summary>
/// EN: Port for rendering LLM prompts from Scriban templates. Application handlers depend only on a template name (<see cref="PromptTemplates"/>) and a context model, never on the template engine or on where template files are stored.
/// PL: Port renderowania promptów LLM z szablonów Scriban. Handlery Application zależą wyłącznie od nazwy szablonu (<see cref="PromptTemplates"/>) i modelu kontekstu — nie znają silnika szablonów ani miejsca przechowywania plików.
/// </summary>
/// <remarks>
/// EN: Implemented in Infrastructure and registered as a singleton, so implementations must be thread-safe. The call is synchronous and blocking: rendering is CPU-only, but loading the template text may perform I/O.
/// PL: Implementowany w Infrastructure i rejestrowany jako singleton, więc implementacje muszą być thread-safe. Wywołanie jest synchroniczne i blokujące: renderowanie obciąża tylko CPU, ale odczyt treści szablonu może wykonać I/O.
/// </remarks>
public interface IPromptRenderer
{
    /// <summary>
    /// EN: Renders the template <paramref name="templateName"/> with <paramref name="model"/> as its context. The model is exposed to the template as the global <c>state</c> (e.g. <c>state.profile.native_language</c>), with .NET member names converted from PascalCase to snake_case. A static template may ignore the model. Access to an undefined variable or member is an error, not an empty string.
    /// PL: Renderuje szablon <paramref name="templateName"/> z modelem <paramref name="model"/> jako kontekstem. Model jest dostępny w szablonie jako globalne <c>state</c> (np. <c>state.profile.native_language</c>), a nazwy składowych .NET są konwertowane z PascalCase na snake_case. Szablon statyczny może ignorować model. Odwołanie do niezdefiniowanej zmiennej lub składowej jest błędem, a nie pustym tekstem.
    /// </summary>
    /// <typeparam name="TModel">
    /// EN: Any reference type used as the template context (avoids casting on the caller side).
    /// PL: Dowolny typ referencyjny użyty jako kontekst szablonu (eliminuje rzutowanie po stronie wołającego).
    /// </typeparam>
    /// <param name="templateName">
    /// EN: Template name, one of the <see cref="PromptTemplates"/> constants; must not be null, empty or whitespace.
    /// PL: Nazwa szablonu — jedna ze stałych <see cref="PromptTemplates"/>; nie może być null, pusta ani złożona z białych znaków.
    /// </param>
    /// <param name="model">
    /// EN: Template context; must not be null.
    /// PL: Kontekst szablonu; nie może być null.
    /// </param>
    /// <returns>
    /// EN: The rendered prompt text.
    /// PL: Wyrenderowany tekst promptu.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: The template does not exist, contains Scriban syntax errors, or fails at render time (e.g. an undefined variable). This is a configuration defect, not a user error; by default the global exception handler maps it to HTTP 500.
    /// PL: Szablon nie istnieje, zawiera błędy składni Scriban albo zawodzi w czasie renderowania (np. niezdefiniowana zmienna). To defekt konfiguracji, nie błąd użytkownika; domyślnie globalny exception handler mapuje go na HTTP 500.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="templateName"/> or <paramref name="model"/> is null.
    /// PL: <paramref name="templateName"/> lub <paramref name="model"/> jest null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="templateName"/> is empty or whitespace.
    /// PL: <paramref name="templateName"/> jest pusty lub złożony z białych znaków.
    /// </exception>
    string Render<TModel>(string templateName, TModel model) where TModel : class;
}

