namespace MyTutorAI.Domain.Memory.Working;

/// <summary>
/// EN: Immutable single turn of a session conversation, stored in the Working memory history.
/// PL: Niezmienna pojedyncza tura rozmowy w sesji, przechowywana w historii pamięci Working.
/// </summary>
public sealed record ChatTurn
{
    /// <summary>
    /// EN: Creates a turn with the given author and content.
    /// PL: Tworzy turę z podanym autorem i treścią.
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
    public ChatTurn(ChatRole role, string content)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown chat role.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Role = role;
        Content = content.Trim();
    }

    /// <summary>
    /// EN: Author of the turn: learner, tutor or system.
    /// PL: Autor tury: uczeń, tutor lub system.
    /// </summary>
    public ChatRole Role { get; }

    /// <summary>
    /// EN: Message content; trimmed and never empty.
    /// PL: Treść wiadomości; przycięta i nigdy pusta.
    /// </summary>
    public string Content { get; }
}

