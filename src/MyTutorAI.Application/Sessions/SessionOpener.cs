using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Sessions;

/// <summary>
/// EN: Builds the opener of a new session, i.e. the first Assistant turn, from the learner's memory; called by <see cref="StartSessionHandler"/> after the opening phase is set. Pure and free of I/O.
/// PL: Buduje powitanie nowej sesji, czyli pierwszą turę Assistant, z pamięci ucznia; wywoływany przez <see cref="StartSessionHandler"/> po ustaleniu fazy otwarcia. Czysta funkcja, bez I/O.
/// </summary>
public static class SessionOpener
{
    /// <summary>
    /// EN: Returns the English greeting that opens the session and addresses the learner by <c>Profile.Name</c>.
    /// PL: Zwraca angielskie powitanie otwierające sesję, zwracające się do ucznia przez <c>Profile.Name</c>.
    /// </summary>
    /// <param name="state">
    /// EN: Learner's memory with the phase and topic already set in <c>Workflow</c>; read only, never modified.
    /// PL: Pamięć ucznia z fazą i tematem już ustawionymi w <c>Workflow</c>; tylko odczyt, nigdy nie jest modyfikowana.
    /// </param>
    /// <returns>
    /// EN: Non-empty opener text; the same state always gives the same text.
    /// PL: Niepusty tekst powitania; ten sam stan zawsze daje ten sam tekst.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> is <c>null</c>.
    /// PL: <paramref name="state"/> jest <c>null</c>.
    /// </exception>
    /// <remarks>
    /// EN: In the <see cref="SessionPhase.HomeworkCheck"/> phase with a non-empty <c>Progress.NextFocus</c>, the opener refers back to that focus. Otherwise it names a topic, which is the first <c>Profile.WorkContext.HighStakesSituations</c> entry or, when there is none, <c>Workflow.Topic</c>, preceded by the last <c>Profile.PersonalContext.RelationshipHooks</c> entry when one exists.
    /// PL: W fazie <see cref="SessionPhase.HomeworkCheck"/> z niepustym <c>Progress.NextFocus</c> powitanie nawiązuje do tego fokusu. W przeciwnym razie podaje temat, którym jest pierwszy wpis <c>Profile.WorkContext.HighStakesSituations</c>, a gdy go brak — <c>Workflow.Topic</c>, poprzedzony ostatnim wpisem <c>Profile.PersonalContext.RelationshipHooks</c>, jeśli taki istnieje.
    /// </remarks>
    public static string BuildOpener(AgentState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var name = state.Profile.Name;

        if (state.Workflow.Phase == SessionPhase.HomeworkCheck
            && state.Progress.NextFocus is { Length: > 0 } nextFocus)
        {
            return $"Hi {name}! Let's pick up where we left off: {nextFocus}. " +
                   "We'll start with a quick warm-up and then move on smoothly.";
        }

        var hooks = state.Profile.PersonalContext.RelationshipHooks;
        var hook = hooks.Count > 0 ? hooks[^1] : string.Empty;

        var highStakes = state.Profile.WorkContext.HighStakesSituations;
        var topic = highStakes.Count > 0 ? highStakes[0] : state.Workflow.Topic;

        return hook.Length > 0
            ? $"Hi {name}! {hook} Today we'll focus on: {topic}. Start in English and I'll guide you."
            : $"Hi {name}! Today we'll focus on: {topic}. Start in English and I'll guide you.";
    }
}
