using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Llm.Contracts;
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
/// EN: Business process of closing a session: storage ports and the LLM client are mocked, domain rules (episode mapping, episode application, phase policy) run for real.
/// PL: Proces biznesowy zamknięcia sesji: porty magazynów i klient LLM są zamockowane, reguły domenowe (mapowanie epizodu, nanoszenie epizodu, polityka faz) działają naprawdę.
/// </summary>
public sealed class CloseSessionHandlerTests
{
    private const string Topic = "Job interview practice";
    private const string Opener = "Hi Anna, ready to practise job interview answers?";
    private const string Summary = "Anna practised introducing herself for a bank interview.";
    private const string PersonalFact = "Has an interview at a bank on Friday";
    private const string NextStep = "Practise answers about strengths and weaknesses";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SessionId = Guid.NewGuid();
    private static readonly DateTimeOffset StartedAt = new(2026, 10, 21, 9, 30, 0, TimeSpan.Zero);
    private static readonly MistakeLogResponse Mistake = new("I am working here since 2020", "I have been working here since 2020", "Present perfect continuous");

    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<IAgentStateStore> _agentStateStore = new();
    private readonly Mock<IWorkingMemoryStore> _workingMemory = new();
    private readonly Mock<IStructuredOutputClient> _llm = new();
    /// <summary>
    /// EN: Names of storage write calls in the order they happen; lets tests assert the sequence of the persistence step.
    /// PL: Nazwy wywołań zapisu do magazynów w kolejności wykonania; pozwala testom sprawdzić kolejność kroku utrwalania.
    /// </summary>
    private readonly List<string> _storageCalls = [];
    private readonly CloseSessionHandler _handler;

    private EpisodeResponse _llmEpisode = new(Summary, [PersonalFact], [Mistake], NextStep);
    private IReadOnlyList<PromptMessage>? _sentPrompts;
    private AgentState? _savedState;
    private Session? _savedSession;

    /// <summary>
    /// EN: Builds the handler with a prompt renderer returning the bracketed template name, an LLM client returning the episode set by the test, and records agent state saves, Working memory clears and session saves.
    /// PL: Buduje handler z rendererem promptów zwracającym nazwę szablonu w nawiasach, klientem LLM zwracającym epizod ustawiony przez test oraz rejestruje zapisy stanu agenta, czyszczenia pamięci Working i zapisy sesji.
    /// </summary>
    public CloseSessionHandlerTests()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(UserId);

        var promptRenderer = new Mock<IPromptRenderer>();
        promptRenderer
            .Setup(x => x.Render(It.IsAny<string>(), It.IsAny<AgentState>()))
            .Returns<string, AgentState>((templateName, _) => $"[{templateName}]");

        _llm
            .Setup(x => x.CompleteAsync<EpisodeResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PromptMessage>, StructuredOutputRequestOptions, CancellationToken>(
                (prompts, _, _) => _sentPrompts = prompts)
            .ReturnsAsync(() => _llmEpisode);

        // Storage writes record their order so the test can assert the sequence of the persistence step.
        _agentStateStore
            .Setup(x => x.SaveAsync(It.IsAny<AgentState>(), It.IsAny<CancellationToken>()))
            .Callback<AgentState, CancellationToken>((state, _) =>
            {
                _savedState = state;
                _storageCalls.Add($"{nameof(IAgentStateStore)}.{nameof(IAgentStateStore.SaveAsync)}");
            })
            .Returns(Task.CompletedTask);

        _workingMemory
            .Setup(x => x.ClearAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback(() => _storageCalls.Add(
                $"{nameof(IWorkingMemoryStore)}.{nameof(IWorkingMemoryStore.ClearAsync)}"))
            .Returns(Task.CompletedTask);

        _sessions
            .Setup(x => x.SaveAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((session, _) =>
            {
                _savedSession = session;
                _storageCalls.Add($"{nameof(ISessionRepository)}.{nameof(ISessionRepository.SaveAsync)}");
            })
            .Returns(Task.CompletedTask);

        _handler = new CloseSessionHandler(
            currentUser.Object, _agentStateStore.Object, _sessions.Object, _workingMemory.Object,
            promptRenderer.Object, _llm.Object);
    }

    /// <summary>
    /// EN: The compression prompt and the session transcript are sent to the LLM; the returned episode is saved in episodic memory together with recent mistakes, next focus, assigned homework and personal facts, Working memory is cleared, the next session opens with homework check, and the session is closed after the memory is saved.
    /// PL: Do LLM trafia prompt kompresji i transkrypt sesji; zwrócony epizod jest zapisywany w pamięci epizodycznej razem z ostatnimi błędami, fokusem następnej sesji, przydzielonym zadaniem i faktami osobistymi, pamięć Working jest czyszczona, następna sesja zaczyna się od sprawdzenia zadania, a sesja jest zamykana po zapisie pamięci.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmReturnsEpisode_SavesEpisodeProgressThenClosesSession()
    {
        ArrangeSession();

        var response = await _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        response.NextOpenPhase.Should().Be(SessionPhase.HomeworkCheck);
        response.Episode.Should().BeEquivalentTo(new
        {
            Summary,
            PersonalFacts = new[] { PersonalFact },
            Mistakes = new[] { new { Mistake.UserPhrase, Mistake.Correction, Mistake.Rule } },
            NextStep,
        });

        _sentPrompts.Should().NotBeNull();
        _sentPrompts!.Select(x => (x.Role, x.Text)).Should().Equal(
            (PromptRole.System, $"[{PromptTemplates.EpisodeCompressionSystem}]"),
            (PromptRole.User, $"assistant: {Opener}\nuser: Learner answer 1\nassistant: Tutor follow-up 1"));

        _savedState.Should().NotBeNull();
        _savedState!.Episodic.Episodes.Should().ContainSingle().Which.Summary.Should().Be(Summary);
        _savedState.Progress.RecentMistakes.Should().Equal(
            $"{Mistake.UserPhrase} -> {Mistake.Correction} ({Mistake.Rule})");
        _savedState.Progress.NextFocus.Should().Be(NextStep);
        _savedState.Progress.HomeworkStatus.Should().Be(HomeworkStatus.Assigned);
        _savedState.Profile.PersonalContext.PersonalFacts.Should().Contain(PersonalFact);
        _savedState.Working.Count.Should().Be(0);
        _savedState.Workflow.Phase.Should().Be(SessionPhase.HomeworkCheck);

        _savedSession.Should().NotBeNull();
        _savedSession!.Status.Should().Be(SessionStatus.Closed);

        _workingMemory.Verify(x => x.ClearAsync(UserId, SessionId, It.IsAny<CancellationToken>()), Times.Once);
        _storageCalls.Should().Equal(
            $"{nameof(IAgentStateStore)}.{nameof(IAgentStateStore.SaveAsync)}",
            $"{nameof(IWorkingMemoryStore)}.{nameof(IWorkingMemoryStore.ClearAsync)}",
            $"{nameof(ISessionRepository)}.{nameof(ISessionRepository.SaveAsync)}");
    }

    /// <summary>
    /// EN: An episode without mistakes and without a next step leaves no backlog: earlier recent mistakes are replaced by the empty list and the next session opens with practice.
    /// PL: Epizod bez błędów i bez następnego kroku nie zostawia zaległości: wcześniejsze ostatnie błędy są zastępowane pustą listą, a następna sesja zaczyna się od ćwiczenia.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EpisodeWithoutBacklog_NextSessionOpensWithPractice()
    {
        ArrangeSession(progress: p => p.SetRecentMistakes(["He go to work -> He goes to work (Third person -s)"]));
        _llmEpisode = new EpisodeResponse(Summary, [], []);

        var response = await _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        response.NextOpenPhase.Should().Be(SessionPhase.Practice);
        _savedState.Should().NotBeNull();
        _savedState!.Progress.RecentMistakes.Should().BeEmpty();
        _savedState.Progress.HomeworkStatus.Should().Be(HomeworkStatus.None);
        _savedState.Workflow.Phase.Should().Be(SessionPhase.Practice);
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

        var act = () => _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<SessionNotFoundException>();
        AssertNothingLoadedOrPersisted();
    }

    /// <summary>
    /// EN: An already closed session ends with <see cref="ConflictException"/> before the memory is loaded and the LLM is called; nothing is persisted.
    /// PL: Już zamknięta sesja kończy się <see cref="ConflictException"/> przed wczytaniem pamięci i wywołaniem LLM; nic nie jest utrwalane.
    /// </summary>
    [Fact]
    public async Task HandleAsync_SessionClosed_ThrowsConflictAndTouchesNothing()
    {
        var session = ArrangeSession();
        session.Close();

        var act = () => _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConflictException>();
        AssertNothingLoadedOrPersisted();
    }

    /// <summary>
    /// EN: A session with empty Working memory ends with <see cref="EmptySessionException"/> before the LLM is called; nothing is persisted.
    /// PL: Sesja z pustą pamięcią Working kończy się <see cref="EmptySessionException"/> przed wywołaniem LLM; nic nie jest utrwalane.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EmptyWorkingMemory_ThrowsEmptySessionAndPersistsNothing()
    {
        ArrangeSession(exchanges: 0, withOpener: false);

        var act = () => _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<EmptySessionException>();
        VerifyLlmNeverCalled();
        _storageCalls.Should().BeEmpty();
    }

    /// <summary>
    /// EN: An LLM failure propagates as <see cref="LlmException"/>, nothing is persisted and the session stays active, so closing can be retried.
    /// PL: Awaria LLM propaguje się jako <see cref="LlmException"/>, nic nie jest utrwalane, a sesja pozostaje aktywna, więc zamknięcie można powtórzyć.
    /// </summary>
    [Fact]
    public async Task HandleAsync_LlmFails_SessionStaysOpenAndNothingPersisted()
    {
        var session = ArrangeSession();
        _llm
            .Setup(x => x.CompleteAsync<EpisodeResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LlmException("Repair attempts exhausted."));

        var act = () => _handler.HandleAsync(SessionId, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<LlmException>();
        session.Status.Should().Be(SessionStatus.Active);
        _storageCalls.Should().BeEmpty();
    }

    /// <summary>
    /// EN: Sets up an active session of the current user in the practice phase and its agent state: B1 learner profile without semantic data, empty episodic memory, and Working memory with the opener followed by the given number of learner/tutor exchanges.
    /// PL: Przygotowuje aktywną sesję bieżącego użytkownika w fazie ćwiczenia i jej stan agenta: profil ucznia B1 bez danych semantycznych, pustą pamięć epizodyczną oraz pamięć Working z powitaniem, po którym następuje podana liczba wymian uczeń/tutor.
    /// </summary>
    /// <param name="exchanges">
    /// EN: Number of learner/tutor exchanges in Working memory after the opener.
    /// PL: Liczba wymian uczeń/tutor w pamięci Working po powitaniu.
    /// </param>
    /// <param name="withOpener">
    /// EN: Whether Working memory starts with the opener; <c>false</c> with no exchanges gives an empty session.
    /// PL: Czy pamięć Working zaczyna się od powitania; <c>false</c> bez wymian daje pustą sesję.
    /// </param>
    /// <param name="progress">
    /// EN: Optional setup of the learner's progress before closing.
    /// PL: Opcjonalne przygotowanie postępu ucznia przed zamknięciem.
    /// </param>
    /// <returns>
    /// EN: The session returned by the mocked repository.
    /// PL: Sesja zwracana przez zamockowane repozytorium.
    /// </returns>
    private Session ArrangeSession(
        int exchanges = 1, bool withOpener = true, Action<LearningProgress>? progress = null)
    {
        var session = new Session(SessionId, UserId, Topic, SessionPhase.Practice, StartedAt);

        var working = new WorkingMemory();
        if (withOpener)
        {
            working.Append(ChatRole.Assistant, Opener);
        }

        for (var i = 1; i <= exchanges; i++)
        {
            working.Append(ChatRole.User, $"Learner answer {i}");
            working.Append(ChatRole.Assistant, $"Tutor follow-up {i}");
        }

        var learningProgress = new LearningProgress();
        progress?.Invoke(learningProgress);

        var state = new AgentState(
            CoreMemory.Default,
            new StudentProfile(UserId, "Anna", "B1", "Pass a job interview"),
            learningProgress,
            new WorkflowMemory(Topic, SessionPhase.Practice),
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
        VerifyLlmNeverCalled();
        _storageCalls.Should().BeEmpty();
    }

    private void VerifyLlmNeverCalled() =>
        _llm.Verify(
            x => x.CompleteAsync<EpisodeResponse>(
                It.IsAny<IReadOnlyList<PromptMessage>>(), It.IsAny<StructuredOutputRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
