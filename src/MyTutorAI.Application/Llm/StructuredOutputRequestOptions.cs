namespace MyTutorAI.Application.Llm;

/// <summary>
/// EN: Per-call overrides for <see cref="IStructuredOutputClient.CompleteAsync{T}"/> — model, temperature, and retry count, layered on top of the <c>LlmOptions</c> defaults.
/// PL: Nadpisania per-wywołanie dla <see cref="IStructuredOutputClient.CompleteAsync{T}"/> — model, temperatura i liczba prób, nakładane na wartości domyślne z <c>LlmOptions</c>.
/// </summary>
/// <remarks>
/// EN: A <c>null</c> field means "don't override, use the configured default" — <c>StructuredOutputClient</c> resolves each field as <c>options.X ?? LlmOptions.X</c>. <c>TimeoutSeconds</c> is deliberately absent: it is HTTP resilience configuration on the client (Polly), not a per-call parameter.
/// PL: Pole <c>null</c> oznacza „nie nadpisuj, weź wartość domyślną z konfiguracji" — <c>StructuredOutputClient</c> rozwiązuje każde pole jako <c>options.X ?? LlmOptions.X</c>. <c>TimeoutSeconds</c> celowo pominięty — to konfiguracja odporności HTTP po stronie klienta (Polly), nie parametr pojedynczego wywołania.
/// </remarks>
/// <param name="Model">
/// EN: LLM model name; <c>null</c> uses the configured default.
/// PL: Nazwa modelu LLM; <c>null</c> oznacza użycie domyślnej wartości z konfiguracji.
/// </param>
/// <param name="Temperature">
/// EN: Generation temperature; <c>null</c> uses the configured default.
/// PL: Temperatura generowania; <c>null</c> oznacza użycie domyślnej wartości z konfiguracji.
/// </param>
/// <param name="MaxRetries">
/// EN: Maximum attempts of the schema-repair retry loop; <c>null</c> uses the configured default.
/// PL: Maksymalna liczba prób pętli naprawczej dopasowania do schematu; <c>null</c> oznacza użycie domyślnej wartości z konfiguracji.
/// </param>
public sealed record StructuredOutputRequestOptions(
    string? Model = null,
    float? Temperature = null,
    int? MaxRetries = null)
{
    /// <summary>
    /// EN: No overrides — every value is taken from <c>LlmOptions</c> configuration. Used by all current callers.
    /// PL: Brak nadpisań — wszystkie wartości brane z konfiguracji <c>LlmOptions</c>. Używane przez wszystkich obecnych wywołujących.
    /// </summary>
    public static StructuredOutputRequestOptions Default { get; } = new();
}
