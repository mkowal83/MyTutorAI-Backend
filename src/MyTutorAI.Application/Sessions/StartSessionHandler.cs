using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case that opens a new session for the current user from the user's Core memory, learner profile, learning progress and episodic memory. The opening phase comes from <see cref="PhasePolicy.DetermineOpenPhase"/> and the opener from <see cref="SessionOpener.BuildOpener"/>; the session is stored through <see cref="ISessionRepository"/>, and the opener is written as the first Assistant turn to the Working memory through <see cref="IWorkingMemoryStore"/>.
/// PL: Przypadek użycia otwierający nową sesję bieżącego użytkownika na podstawie jego pamięci Core, profilu ucznia, postępu nauki i pamięci epizodycznej. Faza otwarcia pochodzi z <see cref="PhasePolicy.DetermineOpenPhase"/>, a powitanie z <see cref="SessionOpener.BuildOpener"/>; sesja jest zapisywana przez <see cref="ISessionRepository"/>, a powitanie trafia jako pierwsza tura Assistant do pamięci Working przez <see cref="IWorkingMemoryStore"/>.
/// </summary>
/// <remarks>
/// EN: The session is saved in SQL Server before the Working memory is cleared and written; when Redis fails, the session stays saved in SQL Server without the opener turn in the Working memory.
/// PL: Sesja jest zapisywana w SQL Server przed wyczyszczeniem i zapisem pamięci Working; gdy zawiedzie Redis, sesja pozostaje zapisana w SQL Server bez tury powitania w pamięci Working.
/// </remarks>
public sealed class StartSessionHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly ICoreMemoryRepository _core;
    private readonly IStudentProfileRepository _profiles;
    private readonly IProgressRepository _progress;
    private readonly IEpisodicMemoryRepository _episodic;
    private readonly ISessionRepository _sessions;
    private readonly IWorkingMemoryStore _workingMemory;
    private readonly IClock _clock;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="core">
    /// EN: Store of the user's Core memory. Must not be null.
    /// PL: Magazyn pamięci Core użytkownika. Nie może być null.
    /// </param>
    /// <param name="profiles">
    /// EN: Store of learner profiles. Must not be null.
    /// PL: Magazyn profili uczniów. Nie może być null.
    /// </param>
    /// <param name="progress">
    /// EN: Store of learners' learning progress. Must not be null.
    /// PL: Magazyn postępu nauki uczniów. Nie może być null.
    /// </param>
    /// <param name="episodic">
    /// EN: Store of learners' episodic memory. Must not be null.
    /// PL: Magazyn pamięci epizodycznej uczniów. Nie może być null.
    /// </param>
    /// <param name="sessions">
    /// EN: Store in which the new session is saved. Must not be null.
    /// PL: Magazyn, w którym zapisywana jest nowa sesja. Nie może być null.
    /// </param>
    /// <param name="workingMemory">
    /// EN: Store of the session's Working memory (Redis), which receives the opener turn. Must not be null.
    /// PL: Magazyn pamięci Working sesji (Redis), do którego trafia tura powitania. Nie może być null.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the session start time. Must not be null.
    /// PL: Źródło czasu rozpoczęcia sesji. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public StartSessionHandler(ICurrentUser currentUser, 
                               ICoreMemoryRepository core, 
                               IStudentProfileRepository profiles,
                               IProgressRepository progress,
                               IEpisodicMemoryRepository episodic,
                               ISessionRepository sessions,
                               IWorkingMemoryStore workingMemory,   
                               IClock clock)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(episodic);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(workingMemory);
        ArgumentNullException.ThrowIfNull(clock);

        _currentUser = currentUser;
        _core = core;
        _profiles = profiles;
        _progress = progress;
        _episodic = episodic;
        _sessions = sessions;
        _workingMemory = workingMemory;
        _clock = clock;
    }

    /// <summary>
    /// EN: Opens a new session with the requested topic for the current user and returns its opening state.
    /// PL: Otwiera nową sesję z żądanym tematem dla bieżącego użytkownika i zwraca jej stan otwarcia.
    /// </summary>
    /// <param name="request">
    /// EN: Session opening data. Must not be null.
    /// PL: Dane otwarcia sesji. Nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels loading the Core memory, the learner profile, the learning progress and the episodic memory, saving the session in the database, and clearing and writing the Working memory.
    /// PL: Anuluje wczytywanie pamięci Core, profilu ucznia, postępu nauki i pamięci epizodycznej, zapis sesji w bazie danych oraz wyczyszczenie i zapis pamięci Working.
    /// </param>
    /// <returns>
    /// EN: Stored session identifier, opening phase, trimmed topic, opener and UTC start time.
    /// PL: Identyfikator nowej sesji, faza otwarcia, przycięty temat, powitanie i czas rozpoczęcia w UTC.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: The current request has no authenticated user id.
    /// PL: Bieżące żądanie nie ma identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: The topic is empty or whitespace.
    /// PL: Temat jest pusty lub zawiera same białe znaki.
    /// </exception>
    public async Task<StartSessionResponse> HandleAsync(StartSessionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = _currentUser.UserId;
        var topic = request.Topic.Trim();

        // 1. Rehydration of the student's four durable memory layers
        var core = await _core.GetAsync(userId, cancellationToken);
        var profile = await _profiles.GetAsync(userId, cancellationToken);
        var progress = await _progress.GetAsync(userId, cancellationToken);
        var episodic = await _episodic.GetAsync(userId, cancellationToken);

        // 2. Assembling AgentState with memory layers
        var state = new AgentState(core, profile, progress, new WorkflowMemory(topic), new WorkingMemory(), episodic);

        // 3. Opening phase based on the remainder of the previous session.
        var openPhase = PhasePolicy.DetermineOpenPhase(state);

        // 4. Retarget Workflow na właściwy temat + fazę (odpowiednik start_session).
        state.Workflow.Retarget(topic, openPhase);

        // 5. Creation of the session record.
        var now = _clock.UtcNow;
        var session = new Session(Guid.NewGuid(), userId, topic, openPhase, now);
        await _sessions.AddAsync(session, cancellationToken);

        // 6. Reset the working memory for the (userId, sessionId)
        await _workingMemory.ClearAsync(userId, session.Id, cancellationToken);

        // 7. Opening text -> Assistant turn (locally for graph consistency + authoritatively in the store).
        var opener = SessionOpener.BuildOpener(state);
        state.Working.Append(ChatRole.Assistant, opener);
        await _workingMemory.AppendAsync(userId, session.Id, ChatRole.Assistant, opener, cancellationToken);


        return new StartSessionResponse(session.Id, openPhase, topic, opener, now);
    }
}
