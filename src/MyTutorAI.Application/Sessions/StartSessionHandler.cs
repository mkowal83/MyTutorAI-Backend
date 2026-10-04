using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Session;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case that opens a new session for the current user and loads the user's Core memory and learner profile. The session is not persisted yet, the opening phase is always <see cref="SessionPhase.HomeworkCheck"/> and the opener is a fixed greeting.
/// PL: Przypadek użycia otwierający nową sesję bieżącego użytkownika i wczytujący jego pamięć Core oraz profil ucznia. Sesja nie jest jeszcze utrwalana, faza otwarcia to zawsze <see cref="SessionPhase.HomeworkCheck"/>, a powitanie jest stałym tekstem.
/// </summary>
public sealed class StartSessionHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly ICoreMemoryRepository _core;
    private readonly IStudentProfileRepository _profiles;

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
    /// <param name="clock">
    /// EN: Source of the session start time. Must not be null.
    /// PL: Źródło czasu rozpoczęcia sesji. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public StartSessionHandler(ICurrentUser currentUser, ICoreMemoryRepository core, IStudentProfileRepository profiles, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profiles);        
        ArgumentNullException.ThrowIfNull(clock);

        _currentUser = currentUser;
        _core = core;
        _profiles = profiles;
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
    /// EN: Cancels loading the Core memory and the learner profile from the database.
    /// PL: Anuluje wczytywanie pamięci Core i profilu ucznia z bazy danych.
    /// </param>
    /// <returns>
    /// EN: New session identifier, opening phase, trimmed topic, opener and UTC start time.
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

        // 2. Assembling AgentState with memory layers
        var state = new AgentState(core, profile);

        // 3. Opening phase based on the remainder of the previous session.
        var openPhase = SessionPhase.HomeworkCheck;

        // 5. Creation and persistence of the session record.
        var now = _clock.UtcNow;
        var session = new Session(Guid.NewGuid(), userId, topic, openPhase, now);

        // 7. Opening text -> Assistant turn (locally for graph consistency + authoritatively in the store).
        var opener = "Welcome to your new session!";

        return new StartSessionResponse(session.Id, openPhase, topic, opener, now);
    }
}
