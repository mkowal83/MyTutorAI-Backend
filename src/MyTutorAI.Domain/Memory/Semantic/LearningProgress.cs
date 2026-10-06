namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Learning progress layer of the learner's semantic memory: recent mistakes, homework status and the focus of the next session. A new instance is an empty progress with no mistakes, no homework and no focus.
/// PL: Warstwa postępu nauki w pamięci semantycznej ucznia: ostatnie błędy, status zadania domowego i fokus następnej sesji. Nowa instancja to pusty postęp bez błędów, zadania i fokusu.
/// </summary>
public sealed class LearningProgress
{
    private readonly List<string> _recentMistakes = [];

    /// <summary>
    /// EN: The learner's recent mistakes as human-readable text, trimmed and without blank entries; changed only via <see cref="SetRecentMistakes"/>.
    /// PL: Ostatnie błędy ucznia jako czytelny tekst, przycięte i bez pustych wpisów; zmieniane wyłącznie przez <see cref="SetRecentMistakes"/>.
    /// </summary>
    public IReadOnlyList<string> RecentMistakes => _recentMistakes;

    /// <summary>
    /// EN: Homework status; defaults to <see cref="HomeworkStatus.None"/> and is changed only via <see cref="SetHomeworkStatus"/>.
    /// PL: Status zadania domowego; domyślnie <see cref="HomeworkStatus.None"/>, zmieniany wyłącznie przez <see cref="SetHomeworkStatus"/>.
    /// </summary>
    public HomeworkStatus HomeworkStatus { get; private set; } = HomeworkStatus.None;

    /// <summary>
    /// EN: Next area to practice, carried over between sessions; trimmed, or null when not set. Changed only via <see cref="SetNextFocus"/>.
    /// PL: Następny obszar do ćwiczenia, przenoszony między sesjami; przycięty albo null, gdy nie ustawiono. Zmieniany wyłącznie przez <see cref="SetNextFocus"/>.
    /// </summary>
    public string? NextFocus { get; private set; }

    /// <summary>
    /// EN: Replaces the whole list of recent mistakes, skipping null or whitespace entries and trimming the rest.
    /// PL: Zastępuje całą listę ostatnich błędów, pomijając wpisy null lub z samych białych znaków i przycinając pozostałe.
    /// </summary>
    /// <param name="recentMistakes">
    /// EN: New mistakes in the order to keep; must not be null, may be empty to clear the list.
    /// PL: Nowe błędy w kolejności do zachowania; nie może być null, może być puste, aby wyczyścić listę.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="recentMistakes"/> is null.
    /// PL: <paramref name="recentMistakes"/> jest null.
    /// </exception>
    public void SetRecentMistakes(IEnumerable<string> recentMistakes)
    {
        ArgumentNullException.ThrowIfNull(recentMistakes);

        _recentMistakes.Clear();
        _recentMistakes.AddRange(
            recentMistakes
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim()));
    }

    /// <summary>
    /// EN: Sets the homework status.
    /// PL: Ustawia status zadania domowego.
    /// </summary>
    /// <param name="homeworkStatus">
    /// EN: New status; must be a value defined in <see cref="Semantic.HomeworkStatus"/>.
    /// PL: Nowy status; musi być wartością zdefiniowaną w <see cref="Semantic.HomeworkStatus"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="homeworkStatus"/> is not a defined enum value.
    /// PL: <paramref name="homeworkStatus"/> nie jest zdefiniowaną wartością enuma.
    /// </exception>
    public void SetHomeworkStatus(HomeworkStatus homeworkStatus)
    {
        if (!Enum.IsDefined(homeworkStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(homeworkStatus), homeworkStatus, "Unknown homework status.");
        }

        HomeworkStatus = homeworkStatus;
    }

    /// <summary>
    /// EN: Sets the focus of the next session; null, empty or whitespace clears it.
    /// PL: Ustawia fokus następnej sesji; null, pusty tekst lub same białe znaki go czyszczą.
    /// </summary>
    /// <param name="nextFocus">
    /// EN: Area to practice; stored trimmed, or as null when blank.
    /// PL: Obszar do ćwiczenia; zapisywany po przycięciu albo jako null, gdy jest pusty.
    /// </param>
    public void SetNextFocus(string? nextFocus) =>
        NextFocus = string.IsNullOrWhiteSpace(nextFocus) ? null : nextFocus.Trim();
}