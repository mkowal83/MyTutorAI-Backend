namespace MyTutorAI.Application.Llm;

/// <summary>
/// EN: Port for communicating with the LLM through structured output — the JSON schema for the requested response type is injected into the system prompt and enforced through a repair loop on non-conforming replies. The sole channel through which handlers talk to the language model.
/// PL: Port komunikacji z LLM przez structured output — schemat JSON żądanego typu odpowiedzi jest wstrzykiwany w system prompt i wymuszany przez pętlę naprawczą przy niezgodnych odpowiedziach. Jedyny kanał, przez który handlery rozmawiają z modelem językowym.
/// </summary>
/// <remarks>
/// EN: Belongs to the Application layer; Domain has no knowledge of LLM communication. Implemented in Infrastructure and registered in DI as a typed <c>HttpClient</c>; test doubles substitute this interface directly.
/// PL: Należy do warstwy Application; Domain nic nie wie o komunikacji z LLM. Implementowany w Infrastructure i rejestrowany w DI jako typed <c>HttpClient</c>; testowe dublery podstawiają ten interfejs bezpośrednio.
/// </remarks>
public interface IStructuredOutputClient
{
    /// <summary>
    /// EN: Sends <paramref name="prompts"/> to the model and returns the reply parsed as <typeparamref name="T"/>. The implementation generates a JSON schema for <typeparamref name="T"/> and injects it as an additional leading system message not present in <paramref name="prompts"/>; on a schema mismatch it retries by appending the invalid reply and an error message to the conversation, up to the effective retry count.
    /// PL: Wysyła <paramref name="prompts"/> do modelu i zwraca odpowiedź sparsowaną jako <typeparamref name="T"/>. Implementacja generuje schemat JSON dla <typeparamref name="T"/> i wstrzykuje go jako dodatkową, wiodącą wiadomość systemową spoza <paramref name="prompts"/>; przy niezgodności ze schematem ponawia próbę, dopisując do konwersacji błędną odpowiedź i komunikat błędu, maksymalnie do efektywnej liczby prób.
    /// </summary>
    /// <typeparam name="T">
    /// EN: Structured output contract type; must be a reference type whose properties deserialize from the model's JSON reply using snake_case names.
    /// PL: Typ kontraktu structured output; musi być typem referencyjnym, którego właściwości deserializują się z odpowiedzi JSON modelu przy nazwach w formacie snake_case.
    /// </typeparam>
    /// <param name="prompts">
    /// EN: Ordered conversation messages, oldest first; excludes the schema system prompt, which the implementation adds automatically.
    /// PL: Uporządkowane wiadomości konwersacji, od najstarszej; nie zawiera systemowego promptu ze schematem — ten dokłada automatycznie implementacja.
    /// </param>
    /// <param name="options">
    /// EN: Per-call overrides for model, temperature and retry count; a <c>null</c> field falls back to the configured default. Use <see cref="StructuredOutputRequestOptions.Default"/> when no override is needed.
    /// PL: Nadpisania modelu, temperatury i liczby prób dla tego wywołania; pole <c>null</c> oznacza wartość domyślną z konfiguracji. Użyj <see cref="StructuredOutputRequestOptions.Default"/>, gdy nadpisania nie są potrzebne.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call, including any retry attempt already in progress.
    /// PL: Anuluje wywołanie, także w trakcie trwającej już próby ponowienia.
    /// </param>
    /// <returns>
    /// EN: The model's reply deserialized into <typeparamref name="T"/>.
    /// PL: Odpowiedź modelu zdeserializowana do <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="MyTutorAI.Application.Exceptions.LlmException">
    /// EN: Thrown when no reply matching the schema is obtained within the effective retry count.
    /// PL: Zgłaszany, gdy w ramach efektywnej liczby prób nie uda się uzyskać odpowiedzi zgodnej ze schematem.
    /// </exception>
    Task<T> CompleteAsync<T>(
        IReadOnlyList<PromptMessage> prompts,
        StructuredOutputRequestOptions options,
        CancellationToken cancellationToken)
        where T : class;
}
