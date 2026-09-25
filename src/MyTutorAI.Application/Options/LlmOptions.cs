using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Application.Options;

/// <summary>
/// EN: Configuration of the LLM client bound from the <c>Llm</c> configuration section; it defines the OpenAI-compatible endpoint, credentials and default generation settings for every structured-output call.
/// PL: Konfiguracja klienta LLM wiązana z sekcji konfiguracji <c>Llm</c>; określa endpoint zgodny z OpenAI, poświadczenia i domyślne ustawienia generowania dla każdego wywołania structured output.
/// </summary>
/// <remarks>
/// EN: <see cref="Model"/>, <see cref="Temperature"/> and <see cref="MaxRetries"/> are defaults that a caller can override per call through <see cref="Llm.StructuredOutputRequestOptions"/>; a <c>null</c> override falls back to the value here. <see cref="BaseUrl"/>, <see cref="ApiKey"/> and <see cref="TimeoutSeconds"/> cannot be overridden per call. Validation relies only on DataAnnotations enforced at host startup (<c>ValidateDataAnnotations().ValidateOnStart()</c>), so an invalid configuration stops the host with <c>OptionsValidationException</c>; there are no cross-field rules.
/// PL: <see cref="Model"/>, <see cref="Temperature"/> i <see cref="MaxRetries"/> to wartości domyślne, które wywołujący może nadpisać per wywołanie przez <see cref="Llm.StructuredOutputRequestOptions"/>; nadpisanie równe <c>null</c> oznacza użycie wartości stąd. <see cref="BaseUrl"/>, <see cref="ApiKey"/> i <see cref="TimeoutSeconds"/> nie da się nadpisać per wywołanie. Walidacja opiera się wyłącznie na DataAnnotations wymuszanych przy starcie hosta (<c>ValidateDataAnnotations().ValidateOnStart()</c>), więc błędna konfiguracja zatrzymuje host wyjątkiem <c>OptionsValidationException</c>; brak reguł krzyżowych między polami.
/// </remarks>
public sealed class LlmOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to this type (<c>Llm</c>).
    /// PL: Nazwa sekcji konfiguracji wiązanej z tym typem (<c>Llm</c>).
    /// </summary>
    public const string SectionName = "Llm";

    /// <summary>
    /// EN: Base URL of the OpenAI-compatible endpoint; an empty or whitespace value means the default OpenAI endpoint is used.
    /// PL: Bazowy URL endpointu zgodnego z OpenAI; pusta lub biała wartość oznacza użycie domyślnego endpointu OpenAI.
    /// </summary>
    /// <remarks>
    /// EN: A non-empty value must be an absolute URI; it is not validated at startup, so an invalid value fails with <see cref="UriFormatException"/> when the LLM client is created.
    /// PL: Niepusta wartość musi być bezwzględnym URI; nie jest walidowana przy starcie, więc błędna wartość kończy się <see cref="UriFormatException"/> przy tworzeniu klienta LLM.
    /// </remarks>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// EN: API key sent to the LLM endpoint; required, and a secret that must come from user-secrets or environment variables, never from files in the repository.
    /// PL: Klucz API wysyłany do endpointu LLM; wymagany i będący sekretem, który musi pochodzić z user-secrets lub zmiennych środowiskowych, nigdy z plików w repozytorium.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// EN: Default model name used for completions, e.g. <c>gpt-4.1-mini</c>; required, with no built-in default.
    /// PL: Domyślna nazwa modelu używanego do generowania, np. <c>gpt-4.1-mini</c>; wymagana, bez wbudowanej wartości domyślnej.
    /// </summary>
    [Required]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// EN: Maximum number of attempts to obtain a response that deserializes into the requested contract, with corrective feedback sent to the model after each invalid answer; allowed range 0–10, default 3, and 0 is treated as a single attempt.
    /// PL: Maksymalna liczba prób uzyskania odpowiedzi deserializowalnej do żądanego kontraktu, z informacją korygującą wysyłaną do modelu po każdej błędnej odpowiedzi; dozwolony zakres 0–10, domyślnie 3, a 0 jest traktowane jak jedna próba.
    /// </summary>
    /// <remarks>
    /// EN: This limit does not control HTTP transport retries (5xx, timeouts, network errors), which are handled separately by the resilience pipeline.
    /// PL: Ten limit nie steruje ponowieniami transportu HTTP (5xx, timeouty, błędy sieci), które obsługuje osobno pipeline odporności.
    /// </remarks>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// EN: Default sampling temperature sent to the model; allowed range 0–2, default 0.2.
    /// PL: Domyślna temperatura próbkowania wysyłana do modelu; dozwolony zakres 0–2, domyślnie 0.2.
    /// </summary>
    [Range(0f, 2f)]
    public float Temperature { get; set; } = 0.2f;

    /// <summary>
    /// EN: Total time budget in seconds for a single HTTP request to the LLM, including transport retries; each individual transport attempt gets half of this budget. Allowed range 1–600, default 60.
    /// PL: Całkowity budżet czasu w sekundach dla pojedynczego żądania HTTP do LLM, łącznie z ponowieniami transportowymi; każda pojedyncza próba transportowa dostaje połowę tego budżetu. Dozwolony zakres 1–600, domyślnie 60.
    /// </summary>
    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 60;
}
