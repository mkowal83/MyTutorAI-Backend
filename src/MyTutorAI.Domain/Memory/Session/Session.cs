namespace MyTutorAI.Domain.Memory.Session;

/// <summary>
/// EN: Learning session of a single user with a fixed topic, the current conversation phase and a lifecycle status.
/// PL: Sesja nauki jednego użytkownika ze stałym tematem, bieżącą fazą rozmowy i statusem cyklu życia.
/// </summary>
public sealed class Session
{
    /// <summary>
    /// EN: Creates a new session in the <see cref="SessionStatus.Active"/> status; the topic is stored trimmed.
    /// PL: Tworzy nową sesję w statusie <see cref="SessionStatus.Active"/>; temat jest zapisywany po przycięciu.
    /// </summary>
    /// <param name="id">
    /// EN: Session identifier; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator sesji; nie może być <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="userId">
    /// EN: Identifier of the owning user; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator użytkownika-właściciela; nie może być <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="topic">
    /// EN: Session topic; must not be null, empty or whitespace.
    /// PL: Temat sesji; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="phase">
    /// EN: Opening phase; must be a defined <see cref="SessionPhase"/> value.
    /// PL: Faza otwarcia; musi być zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </param>
    /// <param name="startedAt">
    /// EN: Moment the session was started.
    /// PL: Moment rozpoczęcia sesji.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="topic"/> is null.
    /// PL: <paramref name="topic"/> jest null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="id"/> or <paramref name="userId"/> is empty, or <paramref name="topic"/> is empty or whitespace.
    /// PL: <paramref name="id"/> lub <paramref name="userId"/> jest pusty albo <paramref name="topic"/> jest pusty lub zawiera same białe znaki.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="phase"/> is not a defined <see cref="SessionPhase"/> value.
    /// PL: <paramref name="phase"/> nie jest zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </exception>
    public Session(Guid id, Guid userId, string topic, SessionPhase phase, DateTimeOffset startedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        EnsureKnownPhase(phase);

        Id = id;
        UserId = userId;
        Topic = topic.Trim();
        Phase = phase;
        Status = SessionStatus.Active;
        StartedAt = startedAt;
    }

    /// <summary>
    /// EN: Session identifier, never <see cref="Guid.Empty"/>.
    /// PL: Identyfikator sesji, nigdy <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// EN: Identifier of the user who owns the session, never <see cref="Guid.Empty"/>.
    /// PL: Identyfikator użytkownika, do którego należy sesja, nigdy <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// EN: Trimmed session topic, set at creation and immutable afterwards.
    /// PL: Przycięty temat sesji, ustalany przy utworzeniu i potem niezmienny.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// EN: Current conversation phase; changed only through <see cref="AdvancePhase"/>.
    /// PL: Bieżąca faza rozmowy; zmieniana wyłącznie przez <see cref="AdvancePhase"/>.
    /// </summary>
    public SessionPhase Phase { get; private set; }

    /// <summary>
    /// EN: Lifecycle status; <see cref="SessionStatus.Active"/> after creation, <see cref="SessionStatus.Closed"/> after <see cref="Close"/>.
    /// PL: Status cyklu życia; <see cref="SessionStatus.Active"/> po utworzeniu, <see cref="SessionStatus.Closed"/> po <see cref="Close"/>.
    /// </summary>
    public SessionStatus Status { get; private set; }

    /// <summary>
    /// EN: Moment the session was started, set at creation.
    /// PL: Moment rozpoczęcia sesji, ustalany przy utworzeniu.
    /// </summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>
    /// EN: Sets the current conversation phase to the given value.
    /// PL: Ustawia bieżącą fazę rozmowy na podaną wartość.
    /// </summary>
    /// <param name="phase">
    /// EN: New phase; must be a defined <see cref="SessionPhase"/> value.
    /// PL: Nowa faza; musi być zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="phase"/> is not a defined <see cref="SessionPhase"/> value.
    /// PL: <paramref name="phase"/> nie jest zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </exception>
    public void AdvancePhase(SessionPhase phase)
    {
        EnsureKnownPhase(phase);
        Phase = phase;
    }

    /// <summary>
    /// EN: Sets <see cref="Status"/> to <see cref="SessionStatus.Closed"/>; calling it on an already closed session changes nothing.
    /// PL: Ustawia <see cref="Status"/> na <see cref="SessionStatus.Closed"/>; wywołanie na już zamkniętej sesji niczego nie zmienia.
    /// </summary>
    public void Close() => Status = SessionStatus.Closed;

    private static void EnsureKnownPhase(SessionPhase phase)
    {
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown session phase.");
        }
    }
}
