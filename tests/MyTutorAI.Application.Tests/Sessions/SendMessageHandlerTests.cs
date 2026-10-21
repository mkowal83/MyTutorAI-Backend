using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;
using MyTutorAI.Application.Options;
using MyTutorAI.Application.Sessions;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Episodic;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;
using Moq;

namespace MyTutorAI.Application.Tests.Sessions;

/// <summary>
/// EN: Business process of a message exchange in an open session: storage ports and the LLM client are mocked, domain rules (guardrail detector, semantic merge, phase policy) run for real.
/// PL: Proces biznesowy wymiany wiadomości w otwartej sesji: porty magazynów i klient LLM są zamockowane, reguły domenowe (detektor guardraila, scalanie semantyczne, polityka faz) działają naprawdę.
/// </summary>
public sealed class SendMessageHandlerTests
{
    private const string Topic = "Job interview practice";
    private const string Opener = "Hi Anna, ready to practise job interview answers?";
    private const string Message = "I am nervous about the interview tomorrow.";
    private const string TutorReply = "That's normal. Tell me how you would introduce yourself.";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SessionId = Guid.NewGuid();
    private static readonly DateTimeOffset StartedAt = new(2026, 10, 21, 9, 30, 0, TimeSpan.Zero);

    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<IAgentStateStore> _agentStateStore = new();
    private readonly Mock<IWorkingMemoryStore> _workingMemory = new();
    private readonly Mock<IStructuredOutputClient> _llm = new();
    /// <summary>
    /// EN: Names of storage write calls in the order they happen (Working memory appends carry the turn role); lets tests assert the sequence of the persistence step.
    /// PL: Nazwy wywołań zapisu do magazynów w kolejności wykonania (dopisania do pamięci Working niosą rolę tury); pozwala testom sprawdzić kolejność kroku utrwalania.
    /// </summary>
    private readonly List<string> _storageCalls = [];
    private readonly SendMessageHandler _handler;

    private LiveTutorReplyResponse _llmReply = Reply();
    private IReadOnlyList<PromptMessage>? _sentPrompts;
    private AgentState? _savedState;
    private Session? _savedSession;

    /// <summary>
    /// EN: Builds the handler with a prompt renderer returning the bracketed template name, an LLM client returning the reply set by the test, and records Working memory, agent state and session writes.
    /// PL: Buduje handler z rendererem promptów zwracającym nazwę szablonu w nawiasach, klientem LLM zwracającym odpowiedź ustawioną przez test oraz rejestruje zapisy pamięci Working, stanu agenta i sesji.
    /// </summary>
    public SendMessageHandlerTests()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(UserId);

        var promptRenderer = new Mock<IPromptRenderer>();
        promptRenderer
            .Setup(x => x.Render(It.IsAny<string>(), It.IsAny<AgentState>()))
            .Returns<string, AgentState>((templateName, _) => $"[{templateName}]");

        _llm
            .Setup(x => x.CompleteAsync<LiveTutorReplyResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PromptMessage>, StructuredOutputRequestOptions, CancellationToken>(
                (prompts, _, _) => _sentPrompts = prompts)
            .ReturnsAsync(() => _llmReply);

        // Storage writes record their order so the test can assert the sequence of the persistence step.
        _workingMemory
            .Setup(x => x.AppendAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ChatRole>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, ChatRole, string, CancellationToken>((_, _, role, _, _) =>
                _storageCalls.Add($"{nameof(IWorkingMemoryStore.AppendAsync)}:{role}"))
            .Returns(Task.CompletedTask);

        _agentStateStore
            .Setup(x => x.SaveAsync(It.IsAny<AgentState>(), It.IsAny<CancellationToken>()))
            .Callback<AgentState, CancellationToken>((state, _) =>
            {
                _savedState = state;
                _storageCalls.Add($"{nameof(IAgentStateStore)}.{nameof(IAgentStateStore.SaveAsync)}");
            })
            .Returns(Task.CompletedTask);

        _sessions
            .Setup(x => x.SaveAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((session, _) =>
            {
                _savedSession = session;
                _storageCalls.Add($"{nameof(ISessionRepository)}.{nameof(ISessionRepository.SaveAsync)}");
            })
            .Returns(Task.CompletedTask);

        _handler = new SendMessageHandler(
            currentUser.Object, _agentStateStore.Object, _sessions.Object, _workingMemory.Object,
            promptRenderer.Object, _llm.Object, Microsoft.Extensions.Options.Options.Create(new MemoryLimitsOptions()));
    }

    /// <summary>
    /// EN: Without reaching a phase threshold the tutor's reply is returned, the system prompt and the Working memory turns are sent to the LLM, both turns are appended to Working memory before the merged profile is saved, and the session itself is not saved.
    /// PL: Bez osiągnięcia progu fazy zwracana jest odpowiedź tutora, do LLM trafia prompt systemowy i tury pamięci Working, obie tury są dopisywane do pamięci Working przed zapisem scalonego profilu, a sama sesja nie jest zapisywana.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmReplies_PersistsBothTurnsThenProfile()
    {
        const string hook = "Interview at a bank tomorrow";
        ArrangeSession(SessionPhase.Practice);
        _llmReply = Reply(semanticWrite: EmptyCandidate() with { RelationshipHooks = [hook] });

        var response = await _handler.HandleAsync(
            SessionId, new SendMessageRequest($"  {Message}  "), TestContext.Current.CancellationToken);

        response.Should().BeEquivalentTo(new
        {
            Reply = TutorReply,
            CoreGuardrailUsed = false,
            CoreGuardrailReason = (string?)null,
            Phase = SessionPhase.Practice,
            AutoPhaseNote = (string?)null,
            WorkingMemoryCount = 3,
        });
        response.SemanticWrite.RelationshipHooks.Should().Equal(hook);

        _sentPrompts.Should().NotBeNull();
        _sentPrompts!.Select(x => (x.Role, x.Text)).Should().Equal(
            (PromptRole.System,
                $"[{PromptTemplates.TutorSystem}]\n\n[{PromptTemplates.TutorLiveOutputInstructions}]"),
            (PromptRole.Assistant, Opener),
            (PromptRole.User, Message));

        _workingMemory.Verify(
            x => x.AppendAsync(UserId, SessionId, ChatRole.User, Message, It.IsAny<CancellationToken>()),
            Times.Once);
        _workingMemory.Verify(
            x => x.AppendAsync(UserId, SessionId, ChatRole.Assistant, TutorReply, It.IsAny<CancellationToken>()),
            Times.Once);
        _storageCalls.Should().Equal(
            $"{nameof(IWorkingMemoryStore.AppendAsync)}:{ChatRole.User}",
            $"{nameof(IWorkingMemoryStore.AppendAsync)}:{ChatRole.Assistant}",
            $"{nameof(IAgentStateStore)}.{nameof(IAgentStateStore.SaveAsync)}");

        _savedState.Should().NotBeNull();
        _savedState!.Profile.PersonalContext.RelationshipHooks.Should().Contain(hook);
    }

    /// <summary>
    /// EN: When the LLM does not report the guardrail but the learner asks for a ready-made answer, the deterministic detector flags the guardrail with its own reason.
    /// PL: Gdy LLM nie zgłasza guardraila, a uczeń prosi o gotową odpowiedź, deterministyczny detektor oznacza guardrail własnym powodem.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmMissedGuardrail_DetectorFlagsHomeworkRequest()
    {
        ArrangeSession(SessionPhase.Practice);

        var response = await _handler.HandleAsync(
            SessionId, new SendMessageRequest("Please write my cover letter, just do it for me."),
            TestContext.Current.CancellationToken);

        response.CoreGuardrailUsed.Should().BeTrue();
        response.CoreGuardrailReason.Should().Be(GuardrailDetector.HomeworkSolvingRequestReason);
    }

    /// <summary>
    /// EN: When the LLM reports the guardrail itself, its reason is kept and not replaced by the detector's reason.
    /// PL: Gdy LLM sam zgłasza guardrail, jego powód zostaje zachowany i nie jest zastępowany powodem detektora.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmReportedGuardrail_KeepsLlmReason()
    {
        const string llmReason = "Learner asked for a finished essay.";
        ArrangeSession(SessionPhase.Practice);
        _llmReply = Reply(guardrailUsed: true, guardrailReason: llmReason);

        var response = await _handler.HandleAsync(
            SessionId, new SendMessageRequest("Just do it for me, please."), TestContext.Current.CancellationToken);

        response.CoreGuardrailUsed.Should().BeTrue();
        response.CoreGuardrailReason.Should().Be(llmReason);
    }

    /// <summary>
    /// EN: When the learner's turn reaches the threshold of the current phase, the phase moves one step forward with the matching note, and the session is saved with the new phase after the profile.
    /// PL: Gdy tura ucznia osiąga próg bieżącej fazy, faza przesuwa się o jeden krok z odpowiednią notatką, a sesja jest zapisywana z nową fazą po profilu.
    /// </summary>
    /// <param name="phase">
    /// EN: Phase of the session before the message.
    /// PL: Faza sesji przed wiadomością.
    /// </param>
    /// <param name="priorUserTurns">
    /// EN: Number of learner turns already in Working memory.
    /// PL: Liczba tur ucznia już obecnych w pamięci Working.
    /// </param>
    /// <param name="expectedPhase">
    /// EN: Phase expected after the message.
    /// PL: Faza oczekiwana po wiadomości.
    /// </param>
    /// <param name="expectedNote">
    /// EN: Automatic phase change note expected in the response.
    /// PL: Notatka o automatycznej zmianie fazy oczekiwana w odpowiedzi.
    /// </param>
    [Theory]
    [InlineData(SessionPhase.HomeworkCheck, 0, SessionPhase.Practice, PhasePolicy.HomeworkCheckAdvancedMessage)]
    [InlineData(SessionPhase.Practice, 3, SessionPhase.WrapUp, PhasePolicy.PracticeAdvancedMessage)]
    public async Task HandleAsync_ThresholdReached_AdvancesPhaseAndSavesSession(
        SessionPhase phase, int priorUserTurns, SessionPhase expectedPhase, string expectedNote)
    {
        ArrangeSession(phase, priorUserTurns);

        var response = await _handler.HandleAsync(
            SessionId, new SendMessageRequest(Message), TestContext.Current.CancellationToken);

        response.Phase.Should().Be(expectedPhase);
        response.AutoPhaseNote.Should().Be(expectedNote);

        _savedSession.Should().NotBeNull();
        _savedSession!.Phase.Should().Be(expectedPhase);
        _storageCalls.Should().EndWith(
        [
            $"{nameof(IAgentStateStore)}.{nameof(IAgentStateStore.SaveAsync)}",
            $"{nameof(ISessionRepository)}.{nameof(ISessionRepository.SaveAsync)}",
        ]);
    }

    /// <summary>
    /// EN: A missing session (or one owned by another user) ends with <see cref="SessionNotFoundException"/> before the memory is loaded and the LLM is called; nothing is persisted.
    /// PL: Brakująca sesja (lub należąca do innego użytkownika) kończy się <see cref="SessionNotFoundException"/> przed wczytaniem pamięci i wywołaniem LLM; nic nie jest utrwalane.
    /// </summary>
    [Fact]
    public async Task HandleAsync_SessionNotFound_ThrowsAndTouchesNothing()
    {
        _sessions
            .Setup(x => x.GetAsync(UserId, SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var act = () => _handler.HandleAsync(
            SessionId, new SendMessageRequest(Message), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<SessionNotFoundException>();
        AssertNothingLoadedOrPersisted();
    }

    /// <summary>
    /// EN: A closed session ends with <see cref="ConflictException"/> before the memory is loaded and the LLM is called; nothing is persisted.
    /// PL: Zamknięta sesja kończy się <see cref="ConflictException"/> przed wczytaniem pamięci i wywołaniem LLM; nic nie jest utrwalane.
    /// </summary>
    [Fact]
    public async Task HandleAsync_SessionClosed_ThrowsConflictAndTouchesNothing()
    {
        var session = ArrangeSession(SessionPhase.Practice);
        session.Close();

        var act = () => _handler.HandleAsync(
            SessionId, new SendMessageRequest(Message), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConflictException>();
        AssertNothingLoadedOrPersisted();
    }

    /// <summary>
    /// EN: An LLM failure propagates as <see cref="LlmException"/> and leaves Working memory, the profile and the session unchanged in storage.
    /// PL: Awaria LLM propaguje się jako <see cref="LlmException"/> i pozostawia pamięć Working, profil i sesję w magazynach bez zmian.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmFails_PersistsNothing()
    {
        ArrangeSession(SessionPhase.HomeworkCheck);
        _llm
            .Setup(x => x.CompleteAsync<LiveTutorReplyResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LlmException("Repair attempts exhausted."));

        var act = () => _handler.HandleAsync(
            SessionId, new SendMessageRequest(Message), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<LlmException>();
        _storageCalls.Should().BeEmpty();
    }

    /// <summary>
    /// EN: Sets up an active session of the current user and its agent state: B1 learner profile without semantic data, empty progress and episodic memory, and Working memory with the opener followed by the given number of learner/tutor exchanges.
    /// PL: Przygotowuje aktywną sesję bieżącego użytkownika i jej stan agenta: profil ucznia B1 bez danych semantycznych, pusty postęp i pamięć epizodyczną oraz pamięć Working z powitaniem, po którym następuje podana liczba wymian uczeń/tutor.
    /// </summary>
    /// <param name="phase">
    /// EN: Current phase of the session and its Workflow memory.
    /// PL: Bieżąca faza sesji i jej pamięci Workflow.
    /// </param>
    /// <param name="priorUserTurns">
    /// EN: Number of learner/tutor exchanges already in Working memory after the opener.
    /// PL: Liczba wymian uczeń/tutor już obecnych w pamięci Working po powitaniu.
    /// </param>
    /// <returns>
    /// EN: The session returned by the mocked repository.
    /// PL: Sesja zwracana przez zamockowane repozytorium.
    /// </returns>
    private Session ArrangeSession(SessionPhase phase, int priorUserTurns = 0)
    {
        var session = new Session(SessionId, UserId, Topic, phase, StartedAt);

        var working = new WorkingMemory();
        working.Append(ChatRole.Assistant, Opener);
        for (var i = 1; i <= priorUserTurns; i++)
        {
            working.Append(ChatRole.User, $"Learner answer {i}");
            working.Append(ChatRole.Assistant, $"Tutor follow-up {i}");
        }

        var state = new AgentState(
            CoreMemory.Default,
            new StudentProfile(UserId, "Anna", "B1", "Pass a job interview"),
            new LearningProgress(),
            new WorkflowMemory(Topic, phase),
            working,
            new EpisodicMemory());

        _sessions
            .Setup(x => x.GetAsync(UserId, SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _agentStateStore
            .Setup(x => x.LoadAsync(UserId, SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        return session;
    }

    private void AssertNothingLoadedOrPersisted()
    {
        _agentStateStore.Verify(
            x => x.LoadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _llm.Verify(
            x => x.CompleteAsync<LiveTutorReplyResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _storageCalls.Should().BeEmpty();
    }

    private static LiveTutorReplyResponse Reply(
        SemanticWriteCandidate? semanticWrite = null,
        bool guardrailUsed = false,
        string? guardrailReason = null) =>
        new(TutorReply, semanticWrite ?? EmptyCandidate(), guardrailUsed, guardrailReason);

    private static SemanticWriteCandidate EmptyCandidate() =>
        new([], [], [], [], [], [], [], [], [], []);
}
