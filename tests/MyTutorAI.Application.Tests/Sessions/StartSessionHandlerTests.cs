using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Sessions;
using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Episodic;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Working;
using Moq;

namespace MyTutorAI.Application.Tests.Sessions;

/// <summary>
/// EN: Business process of opening a session: storage ports are mocked, domain rules (opening phase, greeting) run for real.
/// PL: Proces biznesowy otwarcia sesji: porty magazynów są zamockowane, reguły domenowe (faza otwarcia, powitanie) działają naprawdę.
/// </summary>
public sealed class StartSessionHandlerTests
{
    private const string LearnerName = "Anna";
    private const string Topic = "Job interview practice";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);

    private readonly StudentProfile _profile = new(UserId, LearnerName, "B1", "Pass a job interview");
    private readonly LearningProgress _progress = new();
    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<IWorkingMemoryStore> _workingMemory = new();
    /// <summary>
    /// EN: Names of storage write calls in the order they happen; lets tests assert the sequence of the opening process.
    /// PL: Nazwy wywołań zapisu do magazynów w kolejności wykonania; pozwala testom sprawdzić kolejność procesu otwarcia.
    /// </summary>
    private readonly List<string> _storageCalls = [];
    private readonly StartSessionHandler _handler;

    private Session? _savedSession;

    /// <summary>
    /// EN: Builds the handler with mocks returning the default Core memory, a B1 learner profile without semantic data, empty progress and empty episodic memory, and records session and Working memory writes.
    /// PL: Buduje handler z mockami zwracającymi domyślną pamięć Core, profil ucznia B1 bez danych semantycznych, pusty postęp i pustą pamięć epizodyczną oraz rejestruje zapisy sesji i pamięci Working.
    /// </summary>
    public StartSessionHandlerTests()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(UserId);

        var clock = new Mock<IClock>();
        clock.Setup(x => x.UtcNow).Returns(Now);

        var core = new Mock<ICoreMemoryRepository>();
        core.Setup(x => x.GetAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(CoreMemory.Default);

        var profiles = new Mock<IStudentProfileRepository>();
        profiles.Setup(x => x.GetAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(_profile);

        var progress = new Mock<IProgressRepository>();
        progress.Setup(x => x.GetAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(_progress);

        var episodic = new Mock<IEpisodicMemoryRepository>();
        episodic.Setup(x => x.GetAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new EpisodicMemory());

        // Storage writes record their order so the test can assert the sequence of the opening process.
        _sessions
            .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((session, _) =>
            {
                _savedSession = session;
                _storageCalls.Add(nameof(ISessionRepository.AddAsync));
            })
            .Returns(Task.CompletedTask);

        _workingMemory
            .Setup(x => x.ClearAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback(() => _storageCalls.Add(nameof(IWorkingMemoryStore.ClearAsync)))
            .Returns(Task.CompletedTask);

        _workingMemory
            .Setup(x => x.AppendAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ChatRole>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => _storageCalls.Add(nameof(IWorkingMemoryStore.AppendAsync)))
            .Returns(Task.CompletedTask);

        _handler = new StartSessionHandler(
            currentUser.Object, core.Object, profiles.Object, progress.Object, episodic.Object,
            _sessions.Object, _workingMemory.Object, clock.Object);
    }

    /// <summary>
    /// EN: Without homework or recent mistakes the session opens in <see cref="SessionPhase.Practice"/> with the trimmed topic; the session is saved first, then the Working memory is cleared and receives the opener as the Assistant turn.
    /// PL: Bez zadania domowego i ostatnich błędów sesja otwiera się w <see cref="SessionPhase.Practice"/> z przyciętym tematem; najpierw zapisywana jest sesja, potem pamięć Working jest czyszczona i dostaje powitanie jako turę Assistant.
    /// </summary>
    [Fact]
    public async Task HandleAsync_NoBacklog_OpensPracticeSessionAndPersistsOpener()
    {
        var response = await _handler.HandleAsync(
            new StartSessionRequest($"  {Topic}  "), TestContext.Current.CancellationToken);

        response.Should().BeEquivalentTo(new
        {
            Phase = SessionPhase.Practice,
            Topic,
            StartedAt = Now,
        });
        response.Opener.Should().Contain(LearnerName).And.Contain(Topic);

        _savedSession.Should().BeEquivalentTo(new
        {
            Id = response.SessionId,
            UserId,
            Topic,
            Phase = SessionPhase.Practice,
            Status = SessionStatus.Active,
            StartedAt = Now,
        });

        _workingMemory.Verify(
            x => x.ClearAsync(UserId, response.SessionId, It.IsAny<CancellationToken>()),
            Times.Once);
        _workingMemory.Verify(
            x => x.AppendAsync(
                UserId, response.SessionId, ChatRole.Assistant, response.Opener, It.IsAny<CancellationToken>()),
            Times.Once);
        _storageCalls.Should().Equal(
            nameof(ISessionRepository.AddAsync),
            nameof(IWorkingMemoryStore.ClearAsync),
            nameof(IWorkingMemoryStore.AppendAsync));
    }

    /// <summary>
    /// EN: Assigned homework or recent mistakes make the session open in <see cref="SessionPhase.HomeworkCheck"/>, both in the response and in the saved session.
    /// PL: Przypisane zadanie domowe lub ostatnie błędy sprawiają, że sesja otwiera się w <see cref="SessionPhase.HomeworkCheck"/>, zarówno w odpowiedzi, jak i w zapisanej sesji.
    /// </summary>
    /// <param name="homeworkAssigned">
    /// EN: Sets the homework status to <see cref="HomeworkStatus.Assigned"/>.
    /// PL: Ustawia status zadania domowego na <see cref="HomeworkStatus.Assigned"/>.
    /// </param>
    /// <param name="hasRecentMistakes">
    /// EN: Adds one recent mistake to the learning progress.
    /// PL: Dodaje jeden ostatni błąd do postępu nauki.
    /// </param>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task HandleAsync_LearnerHasBacklog_OpensWithHomeworkCheck(
        bool homeworkAssigned, bool hasRecentMistakes)
    {
        if (homeworkAssigned)
        {
            _progress.SetHomeworkStatus(HomeworkStatus.Assigned);
        }

        if (hasRecentMistakes)
        {
            _progress.SetRecentMistakes(["I have went -> I have gone (present perfect)"]);
        }

        var response = await _handler.HandleAsync(
            new StartSessionRequest(Topic), TestContext.Current.CancellationToken);

        response.Phase.Should().Be(SessionPhase.HomeworkCheck);
        _savedSession.Should().NotBeNull();
        _savedSession!.Phase.Should().Be(SessionPhase.HomeworkCheck);
    }

    /// <summary>
    /// EN: In the homework check phase with a next-session focus, the opener refers to that focus instead of the requested topic.
    /// PL: W fazie sprawdzenia zadania domowego z fokusem następnej sesji powitanie nawiązuje do tego fokusu zamiast do żądanego tematu.
    /// </summary>
    [Fact]
    public async Task HandleAsync_HomeworkAssignedWithNextFocus_OpenerRefersToBacklog()
    {
        const string nextFocus = "Present perfect in job interview answers";
        _progress.SetHomeworkStatus(HomeworkStatus.Assigned);
        _progress.SetNextFocus(nextFocus);

        var response = await _handler.HandleAsync(
            new StartSessionRequest(Topic), TestContext.Current.CancellationToken);

        response.Phase.Should().Be(SessionPhase.HomeworkCheck);
        response.Opener.Should().Contain(nextFocus).And.NotContain(Topic);
    }

    /// <summary>
    /// EN: In the practice phase the opener uses the latest relationship hook and the first high-stakes situation from the profile instead of the requested topic.
    /// PL: W fazie ćwiczenia powitanie używa najnowszego haczyka relacyjnego i pierwszej sytuacji wysokiej stawki z profilu zamiast żądanego tematu.
    /// </summary>
    [Fact]
    public async Task HandleAsync_PersonalHookAndHighStakesSituation_OpenerUsesThemInsteadOfTopic()
    {
        const string latestHook = "How was your trip to Gdansk?";
        const string highStakes = "Quarterly review with the US team";
        _profile.SetPersonalContext(new PersonalContext(
            RelationshipHooks: ["How is your dog?", latestHook]));
        _profile.SetWorkContext(new WorkContext(
            HighStakesSituations: [highStakes, "Client demo"]));

        var response = await _handler.HandleAsync(
            new StartSessionRequest(Topic), TestContext.Current.CancellationToken);

        response.Phase.Should().Be(SessionPhase.Practice);
        response.Opener.Should()
            .Contain(latestHook)
            .And.Contain(highStakes)
            .And.NotContain(Topic);
    }
}
