namespace MyTutorAI.Domain.Tools;

/// <summary>
/// EN: Merges text lists of the semantic profile without overwriting and without duplicates, keeping each list bounded.
/// PL: Scala listy tekstów profilu semantycznego bez nadpisywania i bez duplikatów, utrzymując ograniczony rozmiar każdej listy.
/// </summary>
public static class TextMerge
{
    /// <summary>
    /// EN: Default maximum number of items kept by <see cref="MergeUnique"/>.
    /// PL: Domyślna maksymalna liczba elementów zachowywanych przez <see cref="MergeUnique"/>.
    /// </summary>
    public const int DefaultMergeLimit = 6;

    /// <summary>
    /// EN: Appends <paramref name="incoming"/> to <paramref name="existing"/>, trimming items, skipping blank ones and dropping exact (case-sensitive) duplicates while keeping first-seen order; when the result exceeds <paramref name="limit"/>, only the newest items are kept. Pure; inputs are not modified.
    /// PL: Dołącza <paramref name="incoming"/> do <paramref name="existing"/>, przycinając elementy, pomijając puste i odrzucając dokładne duplikaty (z rozróżnieniem wielkości liter) z zachowaniem kolejności pierwszego wystąpienia; gdy wynik przekracza <paramref name="limit"/>, zostają tylko najnowsze elementy. Czysta; wejścia nie są modyfikowane.
    /// </summary>
    /// <param name="existing">
    /// EN: Items already stored; <c>null</c> is treated as empty.
    /// PL: Elementy już zapisane; <c>null</c> traktowany jest jak pusta lista.
    /// </param>
    /// <param name="incoming">
    /// EN: New candidate items; <c>null</c> is treated as empty.
    /// PL: Nowi kandydaci; <c>null</c> traktowany jest jak pusta lista.
    /// </param>
    /// <param name="limit">
    /// EN: Maximum number of items in the result; must be positive.
    /// PL: Maksymalna liczba elementów w wyniku; musi być dodatnia.
    /// </param>
    /// <returns>
    /// EN: New list with at most <paramref name="limit"/> unique, trimmed items, oldest first.
    /// PL: Nowa lista z co najwyżej <paramref name="limit"/> unikalnymi, przyciętymi elementami, od najstarszego.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="limit"/> is zero or negative.
    /// PL: <paramref name="limit"/> jest zerem lub liczbą ujemną.
    /// </exception>
    public static IReadOnlyList<string> MergeUnique(
        IEnumerable<string>? existing,
        IEnumerable<string>? incoming,
        int limit = DefaultMergeLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var merged = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in (existing ?? []).Concat(incoming ?? []))
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            var clean = item.Trim();
            if (seen.Add(clean))
            {
                merged.Add(clean);
            }
        }

        return merged.Count <= limit
            ? merged
            : merged.GetRange(merged.Count - limit, limit);
    }
}
