using Microsoft.Extensions.Options;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Application.Llm.Mapping;
using MyTutorAI.Application.Options;
using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case of <c>POST /api/sessions/{sessionId}/messages</c>: generates the tutor's reply to a learner message in the current user's open session through LLM structured output and updates the learner's memory. Registered as scoped.
/// PL: Przypadek użycia <c>POST /api/sessions/{sessionId}/messages</c>: generuje przez structured output LLM odpowiedź tutora na wiadomość ucznia w otwartej sesji bieżącego użytkownika i aktualizuje pamięć ucznia. Rejestrowany jako scoped.
/// </summary>
/// <remarks>
/// EN: Nothing is persisted before the LLM reply arrives, so an LLM failure leaves the memory and the session unchanged. After the reply, both turns are appended to Working memory, the merged semantic profile and other SQL layers are saved, and the session phase is saved only when it advanced. No lock is held during the LLM call.
/// PL: Nic nie jest utrwalane przed nadejściem odpowiedzi LLM, więc awaria LLM pozostawia pamięć i sesję bez zmian. Po odpowiedzi obie tury trafiają do pamięci Working, scalony profil semantyczny i pozostałe warstwy SQL są zapisywane, a faza sesji jest zapisywana tylko wtedy, gdy przeszła dalej. Podczas wywołania LLM nie jest trzymana żadna blokada.
/// </remarks>
public sealed class SendMessageHandler
{
    private const string SessionClosedMessage =
        "Cannot send a message to a closed session.";

    private readonly ICurrentUser _currentUser;
    private readonly IAgentStateStore _agentStateStore;
    private readonly ISessionRepository _sessions;
    private readonly IWorkingMemoryStore _workingMemory;
    private readonly IPromptRenderer _promptRenderer;
    private readonly IStructuredOutputClient _llm;
    private readonly int _contextWindow;

    /// <summary>
    /// EN: Creates the handler.
    /// PL: Tworzy handler.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Source of the authenticated user's identifier.
    /// PL: Źródło identyfikatora uwierzytelnionego użytkownika.
    /// </param>
    /// <param name="agentStateStore">
    /// EN: Loads and saves the learner's memory state.
    /// PL: Wczytuje i zapisuje stan pamięci ucznia.
    /// </param>
    /// <param name="sessions">
    /// EN: Reads the session and saves its phase.
    /// PL: Odczytuje sesję i zapisuje jej fazę.
    /// </param>
    /// <param name="workingMemory">
    /// EN: Redis store of the session's turns.
    /// PL: Magazyn Redis tur sesji.
    /// </param>
    /// <param name="promptRenderer">
    /// EN: Renders the tutor system prompt templates.
    /// PL: Renderuje szablony promptu systemowego tutora.
    /// </param>
    /// <param name="llm">
    /// EN: Structured output LLM client.
    /// PL: Klient LLM ze structured output.
    /// </param>
    /// <param name="memoryLimits">
    /// EN: Supplies the size of the context window sent to the LLM.
    /// PL: Dostarcza rozmiar okna kontekstu wysyłanego do LLM.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any dependency is null.
    /// PL: Dowolna zależność jest null.
    /// </exception>
    public SendMessageHandler(
        ICurrentUser currentUser,
        IAgentStateStore agentStateStore,
        ISessionRepository sessions,
        IWorkingMemoryStore workingMemory,
        IPromptRenderer promptRenderer,
        IStructuredOutputClient llm,
        IOptions<MemoryLimitsOptions> memoryLimits)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(agentStateStore);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(workingMemory);
        ArgumentNullException.ThrowIfNull(promptRenderer);
        ArgumentNullException.ThrowIfNull(llm);
        ArgumentNullException.ThrowIfNull(memoryLimits);

        _currentUser = currentUser;
        _agentStateStore = agentStateStore;
        _sessions = sessions;
        _workingMemory = workingMemory;
        _promptRenderer = promptRenderer;
        _llm = llm;
        _contextWindow = memoryLimits.Value.ContextWindow;
    }

    /// <summary>
    /// EN: Handles one learner message: sends the system prompt and the latest Working memory turns to the LLM, applies the deterministic guardrail check when the LLM did not report the guardrail, merges the semantic write into the profile, advances the phase when its threshold is reached, and persists the result.
    /// PL: Obsługuje jedną wiadomość ucznia: wysyła do LLM prompt systemowy i najnowsze tury pamięci Working, stosuje deterministyczne sprawdzenie guardraila, gdy LLM go nie zgłosił, scala zapis semantyczny z profilem, przesuwa fazę po osiągnięciu progu i utrwala wynik.
    /// </summary>
    /// <param name="sessionId">
    /// EN: Session of the current user.
    /// PL: Sesja bieżącego użytkownika.
    /// </param>
    /// <param name="request">
    /// EN: Validated request with the learner's message; must not be null.
    /// PL: Zwalidowane żądanie z wiadomością ucznia; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the storage operations and the LLM call; cancellation after the LLM reply may leave the exchange partially persisted.
    /// PL: Anuluje operacje na magazynach i wywołanie LLM; anulowanie po odpowiedzi LLM może zostawić wymianę częściowo utrwaloną.
    /// </param>
    /// <returns>
    /// EN: Tutor's reply with the guardrail status, the semantic write, the session phase and the Working memory size.
    /// PL: Odpowiedź tutora ze statusem guardraila, zapisem semantycznym, fazą sesji i rozmiarem pamięci Working.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="SessionNotFoundException">
    /// EN: The session does not exist or belongs to another user (HTTP 404).
    /// PL: Sesja nie istnieje lub należy do innego użytkownika (HTTP 404).
    /// </exception>
    /// <exception cref="ConflictException">
    /// EN: The session is not active (HTTP 409).
    /// PL: Sesja nie jest aktywna (HTTP 409).
    /// </exception>
    /// <exception cref="LlmException">
    /// EN: The LLM call failed or its reply did not match the schema after the repair attempts (HTTP 502).
    /// PL: Wywołanie LLM nie powiodło się lub jego odpowiedź nie pasowała do schematu po próbach naprawczych (HTTP 502).
    /// </exception>
    public async Task<SendMessageResponse> HandleAsync(
        Guid sessionId,
        SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = _currentUser.UserId;
        var message = request.Message.Trim();

        // a) Session entity — ownership check (per-user isolation) + Active status.
        var session = await _sessions.GetAsync(userId, sessionId, cancellationToken)
            ?? throw new SessionNotFoundException(userId, sessionId);

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(SessionClosedMessage);
        }

        // a) Full AgentState rehydration (the facade throws SessionNotFoundException for another user's session).
        var state = await _agentStateStore.LoadAsync(userId, sessionId, cancellationToken);

        // b) Append the student's turn to the conversation window (in memory; persisted in step j).
        state.Working.Append(ChatRole.User, message);

        // c) System prompt: persona/context + structured output field instructions.
        var systemPrompt = string.Concat(
            _promptRenderer.Render(PromptTemplates.TutorSystem, state),
            "\n\n",
            _promptRenderer.Render(PromptTemplates.TutorLiveOutputInstructions, state));

        // d) prompts = [System] + last N turns of the context window.
        var prompts = new List<PromptMessage>(capacity: _contextWindow + 1)
        {
            new(PromptRole.System, systemPrompt),
        };

        foreach (var turn in state.Working.GetContextWindow(_contextWindow))
        {
            prompts.Add(new PromptMessage(ToPromptRole(turn.Role), turn.Content));
        }

        // e) Structured output — LlmException propagates (→ HTTP 502, KA3.5.3).
        var reply = await _llm.CompleteAsync<LiveTutorReplyResponse>(
            prompts, StructuredOutputRequestOptions.Default, cancellationToken);

        // f) Deterministic guardrail backstop — override only when the LLM did not flag it.
        var coreGuardrailUsed = reply.CoreGuardrailUsed;
        var coreGuardrailReason = reply.CoreGuardrailReason;

        if (!coreGuardrailUsed)
        {
            var (detected, reason) = GuardrailDetector.Detect(message);
            if (detected)
            {
                coreGuardrailUsed = true;
                coreGuardrailReason = reason;
            }
        }

        // g) Merge the semantic write into the profile (mutates state.Profile in place).
        var semanticWrite = SemanticWriteCandidateMapper.ToDomain(reply.SemanticWrite);
        SemanticWriteApplier.Apply(state, semanticWrite);

        // h) Append the coach's turn to the conversation window.
        state.Working.Append(ChatRole.Assistant, reply.Reply);

        // i) Auto-advance the phase based on the number of student turns.
        var autoPhaseNote = PhasePolicy.TryAdvancePhase(state);

        // j) Persist: both working memory turns, the profile, and the phase (if it changed).
        await _workingMemory.AppendAsync(userId, sessionId, ChatRole.User, message, cancellationToken);
        await _workingMemory.AppendAsync(
            userId, sessionId, ChatRole.Assistant, reply.Reply, cancellationToken);
        await _agentStateStore.SaveAsync(state, cancellationToken);

        if (session.Phase != state.Workflow.Phase)
        {
            session.AdvancePhase(state.Workflow.Phase);
            await _sessions.SaveAsync(session, cancellationToken);
        }

        return new SendMessageResponse(
            reply.Reply,
            semanticWrite,
            coreGuardrailUsed,
            coreGuardrailReason,
            state.Workflow.Phase,
            autoPhaseNote,
            state.Working.Count);
    }

    private static PromptRole ToPromptRole(ChatRole role) => role switch
    {
        ChatRole.User => PromptRole.User,
        ChatRole.Assistant => PromptRole.Assistant,
        ChatRole.System => PromptRole.System,
        _ => throw new ArgumentOutOfRangeException(
            nameof(role), role, "Unknown working memory turn role."),
    };
}