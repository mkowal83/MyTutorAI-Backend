namespace MyTutorAI.Domain.Memory.Core;

/// <summary>
/// EN: Deterministic safety net for the no-homework-solving guardrail: detects, by fixed English and Polish phrases, a learner message asking the tutor to produce a ready-made answer. Used after the LLM reply only when the model itself did not report the guardrail; it never blocks the LLM call.
/// PL: Deterministyczna siatka bezpieczeństwa dla guardraila zakazu rozwiązywania zadań: wykrywa po stałych frazach angielskich i polskich wiadomość ucznia proszącą tutora o gotową odpowiedź. Używana po odpowiedzi LLM tylko wtedy, gdy model sam nie zgłosił guardraila; nigdy nie blokuje wywołania LLM.
/// </summary>
public static class GuardrailDetector
{
    private static readonly string[] RequestPatterns =
    [
        "write the whole", "just copy", "do it for me",
        "napisz za mnie", "gotowca", "skopiować", "copy it",
    ];

    /// <summary>
    /// EN: Reason returned by <see cref="Detect"/> when a ready-made-answer request is detected.
    /// PL: Powód zwracany przez <see cref="Detect"/>, gdy wykryto prośbę o gotową odpowiedź.
    /// </summary>
    public const string HomeworkSolvingRequestReason =
        "NO_HOMEWORK_SOLVING: A case involving a ready-made assignment was detected.";

    /// <summary>
    /// EN: Checks whether the learner's message contains one of the known ready-made-answer request phrases; matching is case-insensitive substring search. Pure and thread-safe.
    /// PL: Sprawdza, czy wiadomość ucznia zawiera jedną ze znanych fraz proszących o gotową odpowiedź; dopasowanie to wyszukiwanie podciągu bez rozróżniania wielkości liter. Czysta i thread-safe.
    /// </summary>
    /// <param name="userMessage">
    /// EN: Learner's message; must not be null, empty or whitespace.
    /// PL: Wiadomość ucznia; nie może być null, pusta ani składać się z samych białych znaków.
    /// </param>
    /// <returns>
    /// EN: <c>(true, <see cref="HomeworkSolvingRequestReason"/>)</c> on a match, otherwise <c>(false, null)</c>.
    /// PL: <c>(true, <see cref="HomeworkSolvingRequestReason"/>)</c> przy dopasowaniu, w przeciwnym razie <c>(false, null)</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="userMessage"/> is null, empty or whitespace.
    /// PL: <paramref name="userMessage"/> jest null, pusty lub składa się z samych białych znaków.
    /// </exception>
    public static (bool Used, string? Reason) Detect(string userMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var lowered = userMessage.ToLowerInvariant();
        var matched = RequestPatterns.Any(pattern => lowered.Contains(pattern, StringComparison.Ordinal));

        return matched ? (true, HomeworkSolvingRequestReason) : (false, null);
    }
}
