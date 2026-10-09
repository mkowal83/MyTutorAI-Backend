namespace MyTutorAI.Domain.Memory.Working;

/// <summary>
/// EN: Author of a turn in the Working memory history.
/// PL: Autor tury w historii pamięci Working.
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// EN: Message written by the learner. Default value.
    /// PL: Wiadomość napisana przez ucznia. Wartość domyślna.
    /// </summary>
    User = 0,

    /// <summary>
    /// EN: Response from the tutor.
    /// PL: Odpowiedź tutora.
    /// </summary>
    Assistant = 1,

    /// <summary>
    /// EN: System message or instruction.
    /// PL: Wiadomość systemowa lub instrukcja.
    /// </summary>
    System = 2,
}