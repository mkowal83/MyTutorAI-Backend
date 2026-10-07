namespace MyTutorAI.Domain.Memory.Episodic;

/// <summary>
/// EN: A single language mistake made by the learner in a session, with its correction and the rule behind it; part of an <see cref="Episode"/>.
/// PL: Pojedynczy błąd językowy ucznia popełniony w sesji, wraz z poprawką i regułą, której dotyczy; część <see cref="Episode"/>.
/// </summary>
public sealed record MistakeLog
{
    /// <summary>
    /// EN: Creates a mistake entry; all values are trimmed.
    /// PL: Tworzy wpis błędu; wszystkie wartości są przycinane.
    /// </summary>
    /// <param name="userPhrase">
    /// EN: The learner's phrase containing the mistake; must not be null, empty or whitespace.
    /// PL: Fraza ucznia zawierająca błąd; nie może być null, pusta ani składać się z samych białych znaków.
    /// </param>
    /// <param name="correction">
    /// EN: The correct version of the phrase; must not be null, empty or whitespace.
    /// PL: Poprawna wersja frazy; nie może być null, pusta ani składać się z samych białych znaków.
    /// </param>
    /// <param name="rule">
    /// EN: Short name of the language rule behind the correction; must not be null, empty or whitespace.
    /// PL: Krótka nazwa reguły językowej stojącej za poprawką; nie może być null, pusta ani składać się z samych białych znaków.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="userPhrase"/>, <paramref name="correction"/> or <paramref name="rule"/> is null, empty or whitespace.
    /// PL: <paramref name="userPhrase"/>, <paramref name="correction"/> lub <paramref name="rule"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public MistakeLog(string userPhrase, string correction, string rule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userPhrase);
        ArgumentException.ThrowIfNullOrWhiteSpace(correction);
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);

        UserPhrase = userPhrase.Trim();
        Correction = correction.Trim();
        Rule = rule.Trim();
    }

    /// <summary>
    /// EN: The learner's phrase containing the mistake (trimmed, non-empty).
    /// PL: Fraza ucznia zawierająca błąd (przycięta, niepusta).
    /// </summary>
    public string UserPhrase { get; }

    /// <summary>
    /// EN: The correct version of the phrase (trimmed, non-empty).
    /// PL: Poprawna wersja frazy (przycięta, niepusta).
    /// </summary>
    public string Correction { get; }

    /// <summary>
    /// EN: Name of the language rule behind the correction (trimmed, non-empty).
    /// PL: Nazwa reguły językowej stojącej za poprawką (przycięta, niepusta).
    /// </summary>
    public string Rule { get; }
}
