using System.Collections.Concurrent;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;

namespace MyTutorAI.Api.Tests.Infrastructure;

/// <summary>
/// EN: Test double of the LLM port returning fixed structured replies per contract type and counting the calls; replaces the HTTP client in the API host.
/// PL: Dubler portu LLM zwracający stałe ustrukturyzowane odpowiedzi per typ kontraktu i liczący wywołania; zastępuje klienta HTTP w hoście API.
/// </summary>
public sealed class FakeStructuredOutputClient : IStructuredOutputClient
{
    public const string Reply = "Good start! How would you describe your current role in one sentence?";
    public const string Summary = "The learner practised introducing themselves for a job interview.";
    public const string PersonalFact = "Has a job interview at a bank on Friday";
    public const string NextStep = "Practise answers about strengths and weaknesses";

    public static readonly LiveTutorReplyResponse TutorReply = new(
        Reply,
        new SemanticWriteCandidate([], [], [], [], [], [], [], [], [], []),
        CoreGuardrailUsed: false,
        CoreGuardrailReason: null);

    public static readonly EpisodeResponse Episode = new(
        Summary,
        [PersonalFact],
        [new MistakeLogResponse("I am working here since 2020", "I have been working here since 2020", "Present perfect continuous")],
        NextStep);

    private readonly ConcurrentDictionary<Type, int> _calls = new();

    /// <summary>
    /// EN: Number of completed calls for the contract type <typeparamref name="T"/>.
    /// PL: Liczba wykonanych wywołań dla typu kontraktu <typeparamref name="T"/>.
    /// </summary>
    public int CallCount<T>() => _calls.GetValueOrDefault(typeof(T));

    public Task<T> CompleteAsync<T>(
        IReadOnlyList<PromptMessage> prompts,
        StructuredOutputRequestOptions options,
        CancellationToken cancellationToken)
        where T : class
    {
        object response = typeof(T) switch
        {
            var t when t == typeof(LiveTutorReplyResponse) => TutorReply,
            var t when t == typeof(EpisodeResponse) => Episode,
            _ => throw new NotSupportedException($"No fake LLM response for {typeof(T).Name}."),
        };

        _calls.AddOrUpdate(typeof(T), 1, (_, count) => count + 1);
        return Task.FromResult((T)response);
    }
}
