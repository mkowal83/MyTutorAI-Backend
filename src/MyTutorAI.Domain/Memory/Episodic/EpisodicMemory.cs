namespace MyTutorAI.Domain.Memory.Episodic;

/// <summary>
/// EN: Episodic memory layer of one learner: compressed records of completed sessions, limited to the most recent <see cref="MaxEpisodes"/> episodes.
/// PL: Warstwa pamięci epizodycznej jednego ucznia: skompresowane zapisy zakończonych sesji, ograniczone do ostatnich <see cref="MaxEpisodes"/> epizodów.
/// </summary>
public sealed class EpisodicMemory
{
    private const int DefaultMaxEpisodes = 10;

    private readonly List<Episode> _episodes = [];

    /// <summary>
    /// EN: Creates an empty episodic memory with the given retention limit.
    /// PL: Tworzy pustą pamięć epizodyczną z podanym limitem przechowywania.
    /// </summary>
    /// <param name="maxEpisodes">
    /// EN: Maximum number of retained episodes; must be positive, defaults to 10.
    /// PL: Maksymalna liczba przechowywanych epizodów; musi być dodatnia, domyślnie 10.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="maxEpisodes"/> is zero or negative.
    /// PL: <paramref name="maxEpisodes"/> jest zerem lub liczbą ujemną.
    /// </exception>
    public EpisodicMemory(int maxEpisodes = DefaultMaxEpisodes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEpisodes);

        MaxEpisodes = maxEpisodes;
    }

    /// <summary>
    /// EN: Retained episodes ordered from oldest to newest; changed only through <see cref="Add"/>.
    /// PL: Przechowywane epizody uporządkowane od najstarszego do najnowszego; zmieniane tylko przez <see cref="Add"/>.
    /// </summary>
    public IReadOnlyList<Episode> Episodes => _episodes;

    /// <summary>
    /// EN: Maximum number of retained episodes.
    /// PL: Maksymalna liczba przechowywanych epizodów.
    /// </summary>
    public int MaxEpisodes { get; }

    /// <summary>
    /// EN: Number of currently retained episodes.
    /// PL: Liczba aktualnie przechowywanych epizodów.
    /// </summary>
    public int Count => _episodes.Count;

    /// <summary>
    /// EN: Appends an episode as the newest one and drops the oldest episodes above <see cref="MaxEpisodes"/>.
    /// PL: Dopisuje epizod jako najnowszy i usuwa najstarsze epizody ponad <see cref="MaxEpisodes"/>.
    /// </summary>
    /// <param name="episode">
    /// EN: Episode to add; must not be null.
    /// PL: Epizod do dodania; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="episode"/> is null.
    /// PL: <paramref name="episode"/> jest null.
    /// </exception>
    public void Add(Episode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);

        _episodes.Add(episode);

        if (_episodes.Count > MaxEpisodes)
        {
            _episodes.RemoveRange(0, _episodes.Count - MaxEpisodes);
        }
    }

    /// <summary>
    /// EN: Returns the newest episodes as a new list ordered from oldest to newest.
    /// PL: Zwraca najnowsze epizody jako nową listę uporządkowaną od najstarszego do najnowszego.
    /// </summary>
    /// <param name="limit">
    /// EN: Maximum number of episodes to return; must be positive, defaults to 2.
    /// PL: Maksymalna liczba zwracanych epizodów; musi być dodatnia, domyślnie 2.
    /// </param>
    /// <returns>
    /// EN: Up to <paramref name="limit"/> newest episodes; empty when no episodes are retained.
    /// PL: Do <paramref name="limit"/> najnowszych epizodów; pusta lista, gdy nie ma żadnych epizodów.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="limit"/> is zero or negative.
    /// PL: <paramref name="limit"/> jest zerem lub liczbą ujemną.
    /// </exception>
    public IReadOnlyList<Episode> Recent(int limit = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var start = Math.Max(0, _episodes.Count - limit);
        return _episodes.GetRange(start, _episodes.Count - start);
    }
}
