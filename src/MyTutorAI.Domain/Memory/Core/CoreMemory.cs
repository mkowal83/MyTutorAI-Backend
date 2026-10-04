namespace MyTutorAI.Domain.Memory.Core;

/// <summary>
/// EN: Core memory layer: the coach's fixed identity and the prime directives that govern every session.
/// PL: Warstwa pamięci Core: stała tożsamość coacha i nadrzędne dyrektywy obowiązujące w każdej sesji.
/// </summary>
public sealed record CoreMemory
{
    /// <summary>
    /// EN: Default coach name.
    /// PL: Domyślne imię coacha.
    /// </summary>
    public const string DefaultName = "Coach";

    /// <summary>
    /// EN: Default name of the coach's creator.
    /// PL: Domyślne imię twórcy coacha.
    /// </summary>
    public const string DefaultCreatorName = "Michał";

    /// <summary>
    /// EN: Default coach role.
    /// PL: Domyślna rola coacha.
    /// </summary>
    public const string DefaultRole = "Language Communication Coach";

    /// <summary>
    /// EN: Prime directive <c>NO_HOMEWORK_SOLVING</c>: never hand out ready-made answers, guide instead.
    /// PL: Nadrzędna dyrektywa <c>NO_HOMEWORK_SOLVING</c>: nigdy nie podawaj gotowych odpowiedzi, naprowadzaj.
    /// </summary>
    public const string NoHomeworkSolving =
        "NO_HOMEWORK_SOLVING: never hand out ready-made answers - guide instead";

    /// <summary>
    /// EN: Prime directive <c>COMMUNICATION_FIRST</c>: fluency and perfect sentence structure take priority.
    /// PL: Nadrzędna dyrektywa <c>COMMUNICATION_FIRST</c>: priorytetem są płynność i perfekcyjna budowa zdań.
    /// </summary>
    public const string CommunicationFirst =
        "COMMUNICATION_FIRST: the priority is fluency and perfect sentence structure";

    /// <summary>
    /// EN: Prime directive <c>CONTINUITY</c>: when memory provides context, do not start the session from scratch.
    /// PL: Nadrzędna dyrektywa <c>CONTINUITY</c>: gdy pamięć dostarcza kontekstu, nie zaczynaj sesji od zera.
    /// </summary>
    public const string Continuity =
        "CONTINUITY: if memory provides context, don't start the session from scratch";

    private static readonly string[] CanonicalPrimeDirectives =
        [NoHomeworkSolving, CommunicationFirst, Continuity];

    /// <summary>
    /// EN: Creates the Core layer; with no arguments it produces the canonical coach identity.
    /// PL: Tworzy warstwę Core; bez argumentów daje kanoniczną tożsamość coacha.
    /// </summary>
    /// <param name="name">
    /// EN: Coach name; must not be null, empty or whitespace; stored trimmed.
    /// PL: Imię coacha; nie może być null, puste ani składać się z samych białych znaków; zapisywane po przycięciu.
    /// </param>
    /// <param name="creatorName">
    /// EN: Creator name; must not be null, empty or whitespace; stored trimmed.
    /// PL: Imię twórcy; nie może być null, puste ani składać się z samych białych znaków; zapisywane po przycięciu.
    /// </param>
    /// <param name="role">
    /// EN: Coach role; must not be null, empty or whitespace; stored trimmed.
    /// PL: Rola coacha; nie może być null, pusta ani składać się z samych białych znaków; zapisywana po przycięciu.
    /// </param>
    /// <param name="primeDirectives">
    /// EN: Directives copied with entries trimmed, blank ones dropped and order preserved; <c>null</c> or no remaining entries gives <see cref="NoHomeworkSolving"/>, <see cref="CommunicationFirst"/> and <see cref="Continuity"/>.
    /// PL: Dyrektywy kopiowane z przyciętymi wpisami, bez pustych i z zachowaniem kolejności; <c>null</c> lub brak pozostałych wpisów daje <see cref="NoHomeworkSolving"/>, <see cref="CommunicationFirst"/> i <see cref="Continuity"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="name"/>, <paramref name="creatorName"/> or <paramref name="role"/> is null, empty or whitespace.
    /// PL: <paramref name="name"/>, <paramref name="creatorName"/> lub <paramref name="role"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public CoreMemory(
        string name = DefaultName,
        string creatorName = DefaultCreatorName,
        string role = DefaultRole,
        IReadOnlyList<string>? primeDirectives = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(creatorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        Name = name.Trim();
        CreatorName = creatorName.Trim();
        Role = role.Trim();

        var directives = primeDirectives is null
            ? []
            : primeDirectives
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .ToArray();

        PrimeDirectives = directives.Length == 0
            ? [.. CanonicalPrimeDirectives]
            : directives;
    }

    /// <summary>
    /// EN: Coach name, trimmed and never empty.
    /// PL: Imię coacha, przycięte i nigdy puste.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// EN: Name of the coach's creator, trimmed and never empty.
    /// PL: Imię twórcy coacha, przycięte i nigdy puste.
    /// </summary>
    public string CreatorName { get; }

    /// <summary>
    /// EN: Coach role, trimmed and never empty.
    /// PL: Rola coacha, przycięta i nigdy pusta.
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// EN: The coach's hard operating rules; always at least one, in the given order.
    /// PL: Twarde zasady działania coacha; zawsze co najmniej jedna, w podanej kolejności.
    /// </summary>
    public IReadOnlyList<string> PrimeDirectives { get; }

    /// <summary>
    /// EN: Shared instance of the canonical coach identity, equal to <c>new CoreMemory()</c>.
    /// PL: Współdzielona instancja kanonicznej tożsamości coacha, równa <c>new CoreMemory()</c>.
    /// </summary>
    public static CoreMemory Default { get; } = new();
}
