using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Use case that opens a new session for the current user. The session is not persisted yet, the opening phase is always <see cref="SessionPhase.HomeworkCheck"/> and the opener is a fixed greeting.
/// PL: Przypadek użycia otwierający nową sesję bieżącego użytkownika. Sesja nie jest jeszcze utrwalana, faza otwarcia to zawsze <see cref="SessionPhase.HomeworkCheck"/>, a powitanie jest stałym tekstem.
/// </summary>
public sealed class StartSessionHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the session start time. Must not be null.
    /// PL: Źródło czasu rozpoczęcia sesji. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="currentUser"/> or <paramref name="clock"/> is null.
    /// PL: <paramref name="currentUser"/> lub <paramref name="clock"/> jest null.
    /// </exception>
    public StartSessionHandler(ICurrentUser currentUser, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(clock);

        _currentUser = currentUser;
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
    /// EN: Cancellation token; currently not observed.
    /// PL: Token anulowania; obecnie nie jest obserwowany.
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
