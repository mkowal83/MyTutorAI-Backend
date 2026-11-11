using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Application.Llm.Mapping;
using MyTutorAI.Domain.Memory.Episodic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case of closing a learner's session: compresses its conversation into an episode via LLM structured output and saves the result in the durable memory layers. Registered as scoped.
/// PL: Przypadek użycia zamknięcia sesji ucznia: kompresuje jej rozmowę do epizodu przez structured output LLM i zapisuje wynik w trwałych warstwach pamięci. Rejestrowany jako scoped.
/// </summary>
public sealed class CloseSessionHandler
{
    private const string SessionAlreadyClosedMessage = "The session is already closed.";

    private readonly ICurrentUser _currentUser;
    private readonly IAgentStateStore _agentStateStore;
    private readonly ISessionRepository _sessions;
    private readonly IWorkingMemoryStore _workingMemory;
    private readonly IPromptRenderer _promptRenderer;
    private readonly IStructuredOutputClient _llm;

    /// <summary>
    /// EN: Creates the handler.
    /// PL: Tworzy handler.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Authenticated user whose session is closed.
    /// PL: Uwierzytelniony użytkownik, którego sesja jest zamykana.
    /// </param>
    /// <param name="agentStateStore">
    /// EN: Loads and saves the learner's memory layers.
    /// PL: Wczytuje i zapisuje warstwy pamięci ucznia.
    /// </param>
    /// <param name="sessions">
    /// EN: Reads the session with an ownership check and saves its status.
    /// PL: Odczytuje sesję ze sprawdzeniem właściciela i zapisuje jej status.
    /// </param>
    /// <param name="workingMemory">
    /// EN: Working memory store of the session, cleared after the episode is saved.
    /// PL: Magazyn pamięci Working sesji, czyszczony po zapisaniu epizodu.
    /// </param>
    /// <param name="promptRenderer">
    /// EN: Renders the episode compression system prompt from its template.
    /// PL: Renderuje prompt systemowy kompresji epizodu z szablonu.
    /// </param>
    /// <param name="llm">
    /// EN: Structured output client that returns the <see cref="EpisodeResponse"/>.
    /// PL: Klient structured output zwracający <see cref="EpisodeResponse"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any dependency is null.
    /// PL: Dowolna zależność jest null.
    /// </exception>
    public CloseSessionHandler(
        ICurrentUser currentUser,
        IAgentStateStore agentStateStore,
        ISessionRepository sessions,
        IWorkingMemoryStore workingMemory,
        IPromptRenderer promptRenderer,
        IStructuredOutputClient llm)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(agentStateStore);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(workingMemory);
        ArgumentNullException.ThrowIfNull(promptRenderer);
        ArgumentNullException.ThrowIfNull(llm);

        _currentUser = currentUser;
        _agentStateStore = agentStateStore;
        _sessions = sessions;
        _workingMemory = workingMemory;
        _promptRenderer = promptRenderer;
        _llm = llm;
    }

    /// <summary>
    /// EN: Closes the current user's session: compresses its whole conversation into an episode, updates progress, personal facts and the opening phase of the next session, clears the Working memory and marks the session closed.
    /// PL: Zamyka sesję bieżącego użytkownika: kompresuje całą jej rozmowę do epizodu, aktualizuje postęp, fakty osobiste i fazę otwarcia następnej sesji, czyści pamięć Working i oznacza sesję jako zamkniętą.
    /// </summary>
    /// <remarks>
    /// EN: Nothing is saved before the LLM call succeeds, so a failed compression leaves the session open and the call can be retried.
    /// PL: Nic nie jest zapisywane przed udanym wywołaniem LLM, więc nieudana kompresja zostawia sesję otwartą i wywołanie można powtórzyć.
    /// </remarks>
    /// <param name="sessionId">
    /// EN: Identifier of the session to close.
    /// PL: Identyfikator sesji do zamknięcia.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database, Redis and LLM calls.
    /// PL: Anuluje wywołania bazy danych, Redis i LLM.
    /// </param>
    /// <returns>
    /// EN: The saved episode and the opening phase of the next session.
    /// PL: Zapisany epizod i faza otwarcia następnej sesji.
    /// </returns>
    /// <exception cref="SessionNotFoundException">
    /// EN: The session does not exist or belongs to another user.
    /// PL: Sesja nie istnieje lub należy do innego użytkownika.
    /// </exception>
    /// <exception cref="ConflictException">
    /// EN: The session is already closed.
    /// PL: Sesja jest już zamknięta.
    /// </exception>
    /// <exception cref="EmptySessionException">
    /// EN: The session's Working memory has no turns.
    /// PL: Pamięć Working sesji nie ma żadnych tur.
    /// </exception>
    /// <exception cref="LlmException">
    /// EN: Episode compression by the LLM failed; the API maps it to 502.
    /// PL: Kompresja epizodu przez LLM nie powiodła się; API mapuje to na 502.
    /// </exception>
    public async Task<CloseSessionResponse> HandleAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // a) Session entity — ownership (per-user isolation) + Active status.
        var session = await _sessions.GetAsync(userId, sessionId, cancellationToken)
            ?? throw new SessionNotFoundException(userId, sessionId);

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(SessionAlreadyClosedMessage);
        }

        // a) Full AgentState rehydration + empty-conversation guard.
        var state = await _agentStateStore.LoadAsync(userId, sessionId, cancellationToken);

        if (state.Working.Count == 0)
        {
            throw new EmptySessionException(sessionId);
        }

        // b) Compression system prompt + transcript of the whole session as the user message.
        var prompts = new List<PromptMessage>(capacity: 2)
        {
            new(
                PromptRole.System,
                _promptRenderer.Render(PromptTemplates.EpisodeCompressionSystem, state)),
            new(PromptRole.User, BuildTranscript(state.Working)),
        };

        // c) Structured output — LlmException propagates (→ HTTP 502); nothing has been
        //    persisted yet, so the session stays open and ready for a retry.
        var response = await _llm.CompleteAsync<EpisodeResponse>(
            prompts, StructuredOutputRequestOptions.Default, cancellationToken);

        // d) Episode → durable memory layers; reset the conversation window; opening phase of the next session.
        var episode = EpisodeResponseMapper.ToDomain(response);
        EpisodeApplier.Apply(state, episode);
        state.Working.Clear();

        var nextOpenPhase = PhasePolicy.DetermineOpenPhase(state);
        state.Workflow.AdvancePhase(nextOpenPhase);
        session.Close();

        // e) Durable persistence: memory layers → clear Working → session status.
        await _agentStateStore.SaveAsync(state, cancellationToken);
        await _workingMemory.ClearAsync(userId, sessionId, cancellationToken);
        await _sessions.SaveAsync(session, cancellationToken);

        return new CloseSessionResponse(EpisodeMapper.ToDto(episode), nextOpenPhase);
    }

    private static string BuildTranscript(WorkingMemory working) =>
        string.Join(
            '\n',
            working.History.Select(turn => $"{ToTranscriptRole(turn.Role)}: {turn.Content}"));

    private static string ToTranscriptRole(ChatRole role) => role switch
    {
        ChatRole.User => "user",
        ChatRole.Assistant => "assistant",
        ChatRole.System => "system",
        _ => throw new ArgumentOutOfRangeException(
            nameof(role), role, "Unknown working memory turn role."),
    };
}