using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Domain.Memory.Workflow;

/// <summary>
/// EN: Session phase rules: picks the opening phase of a new session and moves an ongoing session forward automatically once a user-turn threshold is reached. Pure domain logic, free of I/O.
/// PL: Reguły faz sesji: wybiera fazę otwarcia nowej sesji i automatycznie przesuwa trwającą sesję dalej po osiągnięciu progu tur użytkownika. Czysta logika domenowa, bez I/O.
/// </summary>
public static class PhasePolicy
{
    /// <summary>
    /// EN: Number of user turns at which <see cref="SessionPhase.HomeworkCheck"/> moves to <see cref="SessionPhase.Practice"/>.
    /// PL: Liczba tur użytkownika, przy której <see cref="SessionPhase.HomeworkCheck"/> przechodzi w <see cref="SessionPhase.Practice"/>.
    /// </summary>
    public const int HomeworkCheckToPracticeUserTurns = 1;

    /// <summary>
    /// EN: Number of user turns at which <see cref="SessionPhase.Practice"/> moves to <see cref="SessionPhase.WrapUp"/>.
    /// PL: Liczba tur użytkownika, przy której <see cref="SessionPhase.Practice"/> przechodzi w <see cref="SessionPhase.WrapUp"/>.
    /// </summary>
    public const int PracticeToWrapUpUserTurns = 4;

    /// <summary>
    /// EN: Message returned by <see cref="TryAdvancePhase"/> on the <c>HomeworkCheck → Practice</c> transition.
    /// PL: Komunikat zwracany przez <see cref="TryAdvancePhase"/> przy przejściu <c>HomeworkCheck → Practice</c>.
    /// </summary>
    public const string HomeworkCheckAdvancedMessage =
        "The session automatically moved from the HomeworkCheck phase to Practice after a short check-in.";

    /// <summary>
    /// EN: Message returned by <see cref="TryAdvancePhase"/> on the <c>Practice → WrapUp</c> transition.
    /// PL: Komunikat zwracany przez <see cref="TryAdvancePhase"/> przy przejściu <c>Practice → WrapUp</c>.
    /// </summary>
    public const string PracticeAdvancedMessage =
        "The session automatically moved from the Practice phase to WrapUp — time to summarise.";

    /// <summary>
    /// EN: Determines the opening phase of a new session from the learner's backlog: <see cref="SessionPhase.HomeworkCheck"/> when <see cref="LearningProgress.HomeworkStatus"/> is <see cref="HomeworkStatus.Assigned"/> or <see cref="LearningProgress.RecentMistakes"/> is not empty, otherwise <see cref="SessionPhase.Practice"/>.
    /// PL: Ustala fazę otwarcia nowej sesji na podstawie zaległości ucznia: <see cref="SessionPhase.HomeworkCheck"/>, gdy <see cref="LearningProgress.HomeworkStatus"/> to <see cref="HomeworkStatus.Assigned"/> lub <see cref="LearningProgress.RecentMistakes"/> nie jest puste, w przeciwnym razie <see cref="SessionPhase.Practice"/>.
    /// </summary>
    /// <param name="state">
    /// EN: Learner's memory; read only, never modified.
    /// PL: Pamięć ucznia; tylko odczyt, nigdy nie jest modyfikowana.
    /// </param>
    /// <returns>
    /// EN: <see cref="SessionPhase.HomeworkCheck"/> or <see cref="SessionPhase.Practice"/>; never <see cref="SessionPhase.WrapUp"/>.
    /// PL: <see cref="SessionPhase.HomeworkCheck"/> lub <see cref="SessionPhase.Practice"/>; nigdy <see cref="SessionPhase.WrapUp"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> is <c>null</c>.
    /// PL: <paramref name="state"/> jest <c>null</c>.
    /// </exception>
    public static SessionPhase DetermineOpenPhase(AgentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state.Progress.HomeworkStatus == HomeworkStatus.Assigned
            || state.Progress.RecentMistakes.Count > 0
            ? SessionPhase.HomeworkCheck
            : SessionPhase.Practice;
    }

    /// <summary>
    /// EN: Moves the phase of an ongoing session one step forward when the number of user turns reaches the threshold for the current phase.
    /// PL: Przesuwa fazę trwającej sesji o jeden krok do przodu, gdy liczba tur użytkownika osiągnie próg dla bieżącej fazy.
    /// </summary>
    /// <param name="state">
    /// EN: Learner's memory; its <see cref="AgentState.Workflow"/> is modified in place when a transition occurs.
    /// PL: Pamięć ucznia; jej <see cref="AgentState.Workflow"/> jest modyfikowane w miejscu, gdy nastąpi przejście.
    /// </param>
    /// <returns>
    /// EN: <see cref="HomeworkCheckAdvancedMessage"/> or <see cref="PracticeAdvancedMessage"/> for the transition made, or <c>null</c> when no threshold has been reached.
    /// PL: <see cref="HomeworkCheckAdvancedMessage"/> lub <see cref="PracticeAdvancedMessage"/> dla wykonanego przejścia albo <c>null</c>, gdy żaden próg nie został osiągnięty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> is <c>null</c>.
    /// PL: <paramref name="state"/> jest <c>null</c>.
    /// </exception>
    /// <remarks>
    /// EN: All <see cref="ChatRole.User"/> turns in <see cref="WorkingMemory.History"/> are counted, not only those since the last phase change. Thresholds: <see cref="HomeworkCheckToPracticeUserTurns"/> for <c>HomeworkCheck → Practice</c> and <see cref="PracticeToWrapUpUserTurns"/> for <c>Practice → WrapUp</c>. At most one transition happens per call, and none is defined from <see cref="SessionPhase.WrapUp"/>.
    /// PL: Liczone są wszystkie tury <see cref="ChatRole.User"/> w <see cref="WorkingMemory.History"/>, nie tylko te od ostatniej zmiany fazy. Progi: <see cref="HomeworkCheckToPracticeUserTurns"/> dla <c>HomeworkCheck → Practice</c> i <see cref="PracticeToWrapUpUserTurns"/> dla <c>Practice → WrapUp</c>. W jednym wywołaniu następuje co najwyżej jedno przejście, a z <see cref="SessionPhase.WrapUp"/> nie zdefiniowano żadnego.
    /// </remarks>
    public static string? TryAdvancePhase(AgentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var userTurns = state.Working.History.Count(turn => turn.Role == ChatRole.User);

        if (state.Workflow.Phase == SessionPhase.HomeworkCheck
            && userTurns >= HomeworkCheckToPracticeUserTurns)
        {
            state.Workflow.AdvancePhase(SessionPhase.Practice);
            return HomeworkCheckAdvancedMessage;
        }

        if (state.Workflow.Phase == SessionPhase.Practice
            && userTurns >= PracticeToWrapUpUserTurns)
        {
            state.Workflow.AdvancePhase(SessionPhase.WrapUp);
            return PracticeAdvancedMessage;
        }

        return null;
    }
}