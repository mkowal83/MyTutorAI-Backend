using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Domain.Memory.Workflow;

/// <summary>
/// EN: Workflow memory layer: the conversation phase and topic of the current session, exposed through <see cref="AgentState.Workflow"/>.
/// PL: Warstwa pamięci Workflow: faza rozmowy i temat bieżącej sesji, udostępniane przez <see cref="AgentState.Workflow"/>.
/// </summary>
public sealed class WorkflowMemory
{
    /// <summary>
    /// EN: Creates the workflow layer with a required session topic and a starting phase.
    /// PL: Tworzy warstwę Workflow z wymaganym tematem sesji i fazą początkową.
    /// </summary>
    /// <param name="topic">
    /// EN: Topic of the current session; must not be null, empty or whitespace, and is stored trimmed.
    /// PL: Temat bieżącej sesji; nie może być null, pusty ani składać się z samych białych znaków, zapisywany jest po przycięciu.
    /// </param>
    /// <param name="phase">
    /// EN: Starting phase; defaults to <see cref="SessionPhase.HomeworkCheck"/> and must be a defined <see cref="SessionPhase"/> value.
    /// PL: Faza początkowa; domyślnie <see cref="SessionPhase.HomeworkCheck"/>, musi być zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="topic"/> is null, empty or whitespace.
    /// PL: <paramref name="topic"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="phase"/> is not a defined <see cref="SessionPhase"/> value.
    /// PL: <paramref name="phase"/> nie jest zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </exception>
    public WorkflowMemory(string topic, SessionPhase phase = SessionPhase.HomeworkCheck)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        EnsureKnownPhase(phase);

        Topic = topic.Trim();
        Phase = phase;
    }

    /// <summary>
    /// EN: Phase of the current session; changed through <see cref="Retarget"/> (new session goal) or <see cref="AdvancePhase"/> (phase transition only).
    /// PL: Faza bieżącej sesji; zmieniana przez <see cref="Retarget"/> (nowy cel sesji) lub <see cref="AdvancePhase"/> (tylko przejście fazy).
    /// </summary>
    public SessionPhase Phase { get; private set; }

    /// <summary>
    /// EN: Trimmed topic of the current session, used for role-play instructions in the <see cref="SessionPhase.Practice"/> phase; changed through <see cref="Retarget"/>.
    /// PL: Przycięty temat bieżącej sesji, używany w instrukcjach role-play w fazie <see cref="SessionPhase.Practice"/>; zmieniany przez <see cref="Retarget"/>.
    /// </summary>
    public string Topic { get; private set; }

    /// <summary>
    /// EN: Replaces both the session topic and the phase.
    /// PL: Zastępuje jednocześnie temat sesji i fazę.
    /// </summary>
    /// <param name="topic">
    /// EN: New session topic; must not be null, empty or whitespace, and is stored trimmed.
    /// PL: Nowy temat sesji; nie może być null, pusty ani składać się z samych białych znaków, zapisywany jest po przycięciu.
    /// </param>
    /// <param name="phase">
    /// EN: Phase the session opens in; must be a defined <see cref="SessionPhase"/> value.
    /// PL: Faza, w której sesja się otwiera; musi być zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="topic"/> is null, empty or whitespace; the state is left unchanged.
    /// PL: <paramref name="topic"/> jest null, pusty lub zawiera same białe znaki; stan pozostaje bez zmian.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="phase"/> is not a defined <see cref="SessionPhase"/> value; the state is left unchanged.
    /// PL: <paramref name="phase"/> nie jest zdefiniowaną wartością <see cref="SessionPhase"/>; stan pozostaje bez zmian.
    /// </exception>
    public void Retarget(string topic, SessionPhase phase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        EnsureKnownPhase(phase);

        Topic = topic.Trim();
        Phase = phase;
    }

    /// <summary>
    /// EN: Changes only the session phase and keeps the topic, for the <c>HomeworkCheck → Practice → WrapUp</c> transitions and for switching the phase when the session is closed.
    /// PL: Zmienia tylko fazę sesji i zachowuje temat, na potrzeby przejść <c>HomeworkCheck → Practice → WrapUp</c> oraz zmiany fazy przy zamknięciu sesji.
    /// </summary>
    /// <param name="phase">
    /// EN: New session phase; must be a defined <see cref="SessionPhase"/> value.
    /// PL: Nowa faza sesji; musi być zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="phase"/> is not a defined <see cref="SessionPhase"/> value.
    /// PL: <paramref name="phase"/> nie jest zdefiniowaną wartością <see cref="SessionPhase"/>.
    /// </exception>
    /// <remarks>
    /// EN: The forward-only phase order is not enforced here; any defined phase is accepted, including an earlier one.
    /// PL: Jednokierunkowa kolejność faz nie jest tu wymuszana; przyjmowana jest każda zdefiniowana faza, także wcześniejsza.
    /// </remarks>
    public void AdvancePhase(SessionPhase phase)
    {
        EnsureKnownPhase(phase);
        Phase = phase;
    }

    private static void EnsureKnownPhase(SessionPhase phase)
    {
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown session phase.");
        }
    }
}
