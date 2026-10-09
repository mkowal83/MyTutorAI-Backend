namespace MyTutorAI.Domain.Memory.Working;

/// <summary>
/// EN: Working memory layer: the full, size-unbounded turn history of the currently open session. Not thread-safe.
/// PL: Warstwa pamięci Working: pełna, nieograniczona rozmiarem historia tur bieżącej, otwartej sesji. Nie jest thread-safe.
/// </summary>
public sealed class WorkingMemory
{
    private const int DefaultContextWindow = 6;

    private readonly List<ChatTurn> _history = [];

    /// <summary>
    /// EN: All turns of the session in insertion order; read-only view, changed only through <see cref="Append"/> and <see cref="Clear"/>.
    /// PL: Wszystkie tury sesji w kolejności dodania; widok tylko do odczytu, zmieniany wyłącznie przez <see cref="Append"/> i <see cref="Clear"/>.
    /// </summary>
    public IReadOnlyList<ChatTurn> History => _history;

    /// <summary>
    /// EN: Number of turns in the history.
    /// PL: Liczba tur w historii.
    /// </summary>
    public int Count => _history.Count;

    /// <summary>
    /// EN: Appends a turn to the end of the history.
    /// PL: Dopisuje turę na koniec historii.
    /// </summary>
    /// <param name="role">
    /// EN: Author of the turn; must be a defined <see cref="ChatRole"/> value.
    /// PL: Autor tury; musi być zdefiniowaną wartością <see cref="ChatRole"/>.
    /// </param>
    /// <param name="content">
    /// EN: Message content; must not be null, empty or whitespace, and is stored trimmed.
    /// PL: Treść wiadomości; nie może być null, pusta ani składać się z samych białych znaków, zapisywana jest po przycięciu.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="role"/> is not a defined <see cref="ChatRole"/> value.
    /// PL: <paramref name="role"/> nie jest zdefiniowaną wartością <see cref="ChatRole"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="content"/> is null, empty or whitespace.
    /// PL: <paramref name="content"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public void Append(ChatRole role, string content) =>
        _history.Add(new ChatTurn(role, content));

    /// <summary>
    /// EN: Returns the most recent turns, used as the conversation context; the history itself is not changed.
    /// PL: Zwraca najnowsze tury, używane jako kontekst rozmowy; sama historia nie jest zmieniana.
    /// </summary>
    /// <param name="limit">
    /// EN: Maximum number of turns to return; must be positive, defaults to <c>6</c>.
    /// PL: Maksymalna liczba zwracanych tur; musi być dodatnia, domyślnie <c>6</c>.
    /// </param>
    /// <returns>
    /// EN: A new list with up to <paramref name="limit"/> last turns in chronological order; empty when the history is empty.
    /// PL: Nowa lista z co najwyżej <paramref name="limit"/> ostatnimi turami w kolejności chronologicznej; pusta, gdy historia jest pusta.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="limit"/> is zero or negative.
    /// PL: <paramref name="limit"/> jest zerem lub liczbą ujemną.
    /// </exception>
    public IReadOnlyList<ChatTurn> GetContextWindow(int limit = DefaultContextWindow)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var start = Math.Max(0, _history.Count - limit);
        return _history.GetRange(start, _history.Count - start);
    }

    /// <summary>
    /// EN: Removes all turns from the history.
    /// PL: Usuwa wszystkie tury z historii.
    /// </summary>
    public void Clear() => _history.Clear();
}
