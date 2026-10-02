namespace MyTutorAI.Application.Llm;

/// <summary>
/// EN: A single message in the conversation sent to <see cref="IStructuredOutputClient"/>.
/// PL: Pojedyncza wiadomość w konwersacji wysyłanej do <see cref="IStructuredOutputClient"/>.
/// </summary>
public sealed record PromptMessage
{
    /// <summary>
    /// EN: Creates a prompt message. Throws if <paramref name="role"/> is not a defined enum value or <paramref name="text"/> is null/empty/whitespace.
    /// PL: Tworzy wiadomość promptu. Zgłasza wyjątek, gdy <paramref name="role"/> nie jest zdefiniowaną wartością enum albo <paramref name="text"/> jest null/pusty/same białe znaki.
    /// </summary>
    /// <param name="role">
    /// EN: Sender role of the message.
    /// PL: Rola nadawcy wiadomości.
    /// </param>
    /// <param name="text">
    /// EN: Message content; required, non-empty, stored trimmed.
    /// PL: Treść wiadomości; wymagana, niepusta, zapisywana po przycięciu.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="role"/> is not one of the defined <see cref="PromptRole"/> values.
    /// PL: <paramref name="role"/> nie jest jedną ze zdefiniowanych wartości <see cref="PromptRole"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="text"/> is null, empty, or consists only of whitespace.
    /// PL: <paramref name="text"/> jest null, pusty albo składa się wyłącznie z białych znaków.
    /// </exception>
    public PromptMessage(PromptRole role, string text)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown prompt message role.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        Role = role;
        Text = text.Trim();
    }

    /// <summary>
    /// EN: Sender role of the message.
    /// PL: Rola nadawcy wiadomości.
    /// </summary>
    public PromptRole Role { get; }

    /// <summary>
    /// EN: Message content, trimmed and non-empty.
    /// PL: Treść wiadomości, przycięta i niepusta.
    /// </summary>
    public string Text { get; }
}
