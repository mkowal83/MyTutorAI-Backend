namespace MyTutorAI.Infrastructure.Llm;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChatMessage = OpenAI.Chat.ChatMessage; 


/// <summary>
/// EN: Infrastructure adapter of <see cref="IStructuredOutputClient"/> that calls an OpenAI-compatible chat endpoint and returns the reply deserialized into a typed contract. It is the only path through which the tutor's handlers (live reply, episode compression, dialogue demo) talk to the LLM.
/// PL: Adapter Infrastructure dla <see cref="IStructuredOutputClient"/>, który wywołuje endpoint czatu zgodny z OpenAI i zwraca odpowiedź zdeserializowaną do typowanego kontraktu. To jedyna droga, którą handlery tutora (odpowiedź na żywo, kompresja epizodu, demo dialogu) komunikują się z LLM.
/// </summary>
/// <remarks>
/// EN: Registered in DI as a typed HTTP client (transient); transport timeout, transport retry and circuit breaker come from the injected <see cref="HttpClient"/> pipeline, not from this class. The JSON schema and raw model responses are logged only at <see cref="LogLevel.Debug"/>, because conversation content is sensitive.
/// PL: Rejestrowany w DI jako typed HTTP client (transient); timeout transportu, retry transportowy i circuit breaker pochodzą z pipeline'u wstrzykniętego <see cref="HttpClient"/>, a nie z tej klasy. Schemat JSON i surowe odpowiedzi modelu są logowane wyłącznie na poziomie <see cref="LogLevel.Debug"/>, bo treść rozmów jest wrażliwa.
/// </remarks>
public sealed class StructuredOutputClient : IStructuredOutputClient
{
    /// <summary>
    /// EN: System instruction that carries the JSON schema of the target type and forces a bare JSON instance in the reply; it is always the first message of the conversation.
    /// PL: Instrukcja systemowa, która niesie schemat JSON typu docelowego i wymusza w odpowiedzi samą instancję JSON; zawsze jest pierwszą wiadomością konwersacji.
    /// </summary>
    private const string SchemaInstructionTemplate =
        "As a genius expert, your task is to understand the content and provide the parsed " +
        "objects in json that match the following json_schema:\n{0}.\n" +
        "Return ONLY the JSON object, no markdown, no extra text. " +
        "Make sure to return an instance of the JSON, not the schema itself. " +
        "The answer is always in the language of the question asked in a different place.";

    private static readonly CompositeFormat SchemaInstructionFormat =
        CompositeFormat.Parse(SchemaInstructionTemplate);

    private static readonly Regex JsonFencePattern =
        new("```json\\s*(.*?)\\s*```", RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly LlmOptions _options;
    private readonly OpenAIClient _openAiClient;
    private readonly ILogger<StructuredOutputClient> _logger;

    /// <summary>
    /// EN: Creates the client bound to the configured API key and endpoint; an empty <see cref="LlmOptions.BaseUrl"/> selects the SDK's default OpenAI endpoint.
    /// PL: Tworzy klienta powiązanego ze skonfigurowanym kluczem API i endpointem; pusty <see cref="LlmOptions.BaseUrl"/> wybiera domyślny endpoint OpenAI z SDK.
    /// </summary>
    /// <param name="httpClient">
    /// EN: HTTP client supplied by DI whose handler pipeline carries all outgoing requests to the LLM.
    /// PL: HTTP client dostarczony przez DI, którego pipeline handlerów przenosi wszystkie żądania wychodzące do LLM.
    /// </param>
    /// <param name="options">
    /// EN: LLM configuration read once at construction; supplies defaults for model, temperature and repair attempts.
    /// PL: Konfiguracja LLM odczytywana raz w konstruktorze; dostarcza wartości domyślne modelu, temperatury i liczby prób naprawczych.
    /// </param>
    /// <param name="logger">
    /// EN: Logger for Debug-level diagnostics of schemas, raw responses and failed attempts.
    /// PL: Logger do diagnostyki na poziomie Debug: schematów, surowych odpowiedzi i nieudanych prób.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Thrown when <paramref name="httpClient"/>, <paramref name="options"/> or <paramref name="logger"/> is <c>null</c>.
    /// PL: Zgłaszany, gdy <paramref name="httpClient"/>, <paramref name="options"/> lub <paramref name="logger"/> jest <c>null</c>.
    /// </exception>
    /// <exception cref="UriFormatException">
    /// EN: Thrown when a non-empty <see cref="LlmOptions.BaseUrl"/> is not a valid absolute URI.
    /// PL: Zgłaszany, gdy niepusty <see cref="LlmOptions.BaseUrl"/> nie jest poprawnym bezwzględnym URI.
    /// </exception>
    public StructuredOutputClient(HttpClient httpClient, IOptions<LlmOptions> options, ILogger<StructuredOutputClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;

        var credential = new ApiKeyCredential(_options.ApiKey);
        var clientOptions = new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient),
        };
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            clientOptions.Endpoint = new Uri(_options.BaseUrl);
        }

        _openAiClient = new OpenAIClient(credential, clientOptions);
    }

    /// <summary>
    /// EN: Sends the conversation to the model with the JSON schema of <typeparamref name="T"/> prepended as a system message and returns the reply parsed as <typeparamref name="T"/>. Any failed attempt (transport error, missing JSON, invalid JSON) appends the bad reply and a correction request to the conversation and retries.
    /// PL: Wysyła konwersację do modelu ze schematem JSON typu <typeparamref name="T"/> dołożonym na początku jako wiadomość systemowa i zwraca odpowiedź sparsowaną jako <typeparamref name="T"/>. Każda nieudana próba (błąd transportu, brak JSON, niepoprawny JSON) dopisuje do konwersacji błędną odpowiedź i prośbę o poprawkę, po czym ponawia wywołanie.
    /// </summary>
    /// <typeparam name="T">
    /// EN: Structured output contract; its property names are mapped to snake_case in both the schema and deserialization.
    /// PL: Kontrakt structured output; nazwy jego właściwości są mapowane na snake_case zarówno w schemacie, jak i przy deserializacji.
    /// </typeparam>
    /// <param name="prompts">
    /// EN: Conversation messages in order, without the schema instruction, which this method adds itself.
    /// PL: Wiadomości konwersacji w kolejności, bez instrukcji ze schematem, którą ta metoda dokłada sama.
    /// </param>
    /// <param name="options">
    /// EN: Per-call overrides of model, temperature and attempt count; each <c>null</c> field falls back to <see cref="LlmOptions"/>, and the attempt count is at least 1.
    /// PL: Nadpisania modelu, temperatury i liczby prób dla tego wywołania; każde pole <c>null</c> przyjmuje wartość z <see cref="LlmOptions"/>, a liczba prób wynosi co najmniej 1.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call; cancellation is propagated immediately and does not trigger a repair attempt.
    /// PL: Anuluje wywołanie; anulowanie jest propagowane natychmiast i nie uruchamia próby naprawczej.
    /// </param>
    /// <returns>
    /// EN: The first model reply that deserializes into a non-null <typeparamref name="T"/>.
    /// PL: Pierwsza odpowiedź modelu, która deserializuje się do niepustego <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: Thrown when <paramref name="prompts"/> or <paramref name="options"/> is <c>null</c>.
    /// PL: Zgłaszany, gdy <paramref name="prompts"/> lub <paramref name="options"/> jest <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: Thrown when a message in <paramref name="prompts"/> has an unknown <see cref="PromptRole"/>.
    /// PL: Zgłaszany, gdy wiadomość w <paramref name="prompts"/> ma nieznaną <see cref="PromptRole"/>.
    /// </exception>
    /// <exception cref="LlmException">
    /// EN: Thrown when all attempts fail; the last failure is the inner exception, and the API maps it to HTTP 502.
    /// PL: Zgłaszany, gdy wszystkie próby zawiodą; ostatni błąd jest wyjątkiem wewnętrznym, a API mapuje go na HTTP 502.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// EN: Thrown when <paramref name="cancellationToken"/> is cancelled.
    /// PL: Zgłaszany, gdy <paramref name="cancellationToken"/> zostanie anulowany.
    /// </exception>
    public async Task<T> CompleteAsync<T>(
        IReadOnlyList<PromptMessage> prompts,
        StructuredOutputRequestOptions options,
        CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(prompts);
        ArgumentNullException.ThrowIfNull(options);

        var schema = AIJsonUtilities.CreateJsonSchema(typeof(T), serializerOptions: SerializerOptions);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Structured output schema for {Type}: {Schema}", typeof(T).Name, schema);
        }

        var conversation = new List<ChatMessage>
        {
            new SystemChatMessage(string.Format(CultureInfo.InvariantCulture, SchemaInstructionFormat, schema)),
        };
        conversation.AddRange(prompts.Select(ToChatMessage));

        var model = options.Model ?? _options.Model;
        var chatClient = _openAiClient.GetChatClient(model);
        var temperature = options.Temperature ?? _options.Temperature;
        var chatOptions = new ChatCompletionOptions { Temperature = temperature };
        var maxRetries = Math.Max(1, options.MaxRetries ?? _options.MaxRetries);
        Exception? lastError = null;
        var lastRaw = string.Empty;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await chatClient.CompleteChatAsync(conversation, chatOptions, cancellationToken)
                    .ConfigureAwait(false);
                lastRaw = response.Value.Content[0].Text;
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Raw LLM response (attempt {Attempt}/{MaxRetries}): {Raw}", attempt, maxRetries, lastRaw);
                }

                var json = ExtractJsonFromMarkdown(lastRaw);
                var result = JsonSerializer.Deserialize<T>(json, SerializerOptions)
                    ?? throw new JsonException("Deserialization returned null.");
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastError = ex;
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(ex, "Structured output attempt {Attempt}/{MaxRetries} failed for {Type}",
                        attempt, maxRetries, typeof(T).Name);
                }

                conversation.Add(new AssistantChatMessage(lastRaw));
                conversation.Add(new UserChatMessage(
                    $"Your previous response was invalid: {ex.Message}. Return ONLY corrected JSON matching the schema."));
            }
        }

        throw new LlmException(
            $"Failed to obtain a valid structured response for {typeof(T).Name} after {maxRetries} attempts.",
            lastError ?? new InvalidOperationException("No attempt was made."));
    }

    /// <summary>
    /// EN: Returns the content of the first <c>```json</c> fenced block, or the trimmed text when it already starts with <c>{</c> or <c>[</c>.
    /// PL: Zwraca zawartość pierwszego bloku <c>```json</c>, albo przycięty tekst, gdy już zaczyna się od <c>{</c> lub <c>[</c>.
    /// </summary>
    /// <exception cref="JsonException">
    /// EN: Thrown when neither form is found, which counts as a failed attempt in the repair loop.
    /// PL: Zgłaszany, gdy nie znaleziono żadnej z tych postaci, co liczy się jako nieudana próba w pętli naprawczej.
    /// </exception>
    private static string ExtractJsonFromMarkdown(string text)
    {
        var match = JsonFencePattern.Match(text);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        var trimmed = text.Trim();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
        {
            return trimmed;
        }

        throw new JsonException("No JSON block found in the model's response.");
    }

    private static ChatMessage ToChatMessage(PromptMessage message) => message.Role switch
    {
        PromptRole.System => new SystemChatMessage(message.Text),
        PromptRole.User => new UserChatMessage(message.Text),
        PromptRole.Assistant => new AssistantChatMessage(message.Text),
        _ => throw new ArgumentOutOfRangeException(nameof(message), message.Role, "Unknown prompt role."),
    };
}
