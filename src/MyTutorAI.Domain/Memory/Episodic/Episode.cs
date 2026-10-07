namespace MyTutorAI.Domain.Memory.Episodic;

/// <summary>
/// EN: Compressed record of one completed session: a summary, facts about the learner, recorded mistakes and the next practice step.
/// PL: Skompresowany zapis jednej zakończonej sesji: podsumowanie, fakty o uczniu, zapisane błędy i następny krok ćwiczeń.
/// </summary>
public sealed record Episode
{
    private readonly List<string> _personalFacts = [];
    private readonly List<MistakeLog> _mistakes = [];

    /// <summary>
    /// EN: Creates an episode; text values are trimmed and empty entries are dropped.
    /// PL: Tworzy epizod; wartości tekstowe są przycinane, a puste wpisy odrzucane.
    /// </summary>
    /// <param name="summary">
    /// EN: One-sentence summary of the session; must not be null, empty or whitespace.
    /// PL: Jednozdaniowe podsumowanie sesji; nie może być null, puste ani składać się z samych białych znaków.
    /// </param>
    /// <param name="personalFacts">
    /// EN: Durable facts about the learner; null means none, and null, empty or whitespace items are skipped.
    /// PL: Trwałe fakty o uczniu; null oznacza brak, a elementy null, puste lub z samych białych znaków są pomijane.
    /// </param>
    /// <param name="mistakes">
    /// EN: Mistakes recorded in the session; null means none, and null items are skipped.
    /// PL: Błędy zapisane w sesji; null oznacza brak, a elementy null są pomijane.
    /// </param>
    /// <param name="nextStep">
    /// EN: What to practise next; null, empty or whitespace is stored as <see cref="string.Empty"/>.
    /// PL: Co ćwiczyć dalej; null, pusta wartość lub same białe znaki są zapisywane jako <see cref="string.Empty"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="summary"/> is null, empty or whitespace.
    /// PL: <paramref name="summary"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public Episode(
        string summary,
        IReadOnlyList<string>? personalFacts = null,
        IReadOnlyList<MistakeLog>? mistakes = null,
        string? nextStep = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        Summary = summary.Trim();
        NextStep = string.IsNullOrWhiteSpace(nextStep) ? string.Empty : nextStep.Trim();

        if (personalFacts is not null)
        {
            _personalFacts.AddRange(
                personalFacts
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .Select(f => f.Trim()));
        }

        if (mistakes is not null)
        {
            _mistakes.AddRange(mistakes.OfType<MistakeLog>());
        }
    }

    /// <summary>
    /// EN: One-sentence summary of the session (trimmed, non-empty).
    /// PL: Jednozdaniowe podsumowanie sesji (przycięte, niepuste).
    /// </summary>
    public string Summary { get; }

    /// <summary>
    /// EN: Facts about the learner extracted from the conversation (trimmed, non-empty items; may be empty).
    /// PL: Fakty o uczniu wyciągnięte z rozmowy (przycięte, niepuste elementy; lista może być pusta).
    /// </summary>
    public IReadOnlyList<string> PersonalFacts => _personalFacts;

    /// <summary>
    /// EN: Mistakes recorded during the session (may be empty).
    /// PL: Błędy zapisane w trakcie sesji (lista może być pusta).
    /// </summary>
    public IReadOnlyList<MistakeLog> Mistakes => _mistakes;

    /// <summary>
    /// EN: What to practise next (trimmed); <see cref="string.Empty"/> when not given.
    /// PL: Co ćwiczyć dalej (przycięte); <see cref="string.Empty"/>, gdy nie podano.
    /// </summary>
    public string NextStep { get; }
}
