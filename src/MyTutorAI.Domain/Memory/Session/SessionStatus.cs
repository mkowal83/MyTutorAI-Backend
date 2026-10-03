namespace MyTutorAI.Domain.Memory.Session;

/// <summary>
/// EN: Lifecycle status of a <see cref="Session"/>.
/// PL: Status cyklu życia <see cref="Session"/>.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// EN: Open session in progress; the status of every newly created session.
    /// PL: Otwarta sesja w toku; status każdej nowo utworzonej sesji.
    /// </summary>
    Active = 0,

    /// <summary>
    /// EN: Session closed by <see cref="Session.Close"/>.
    /// PL: Sesja zamknięta przez <see cref="Session.Close"/>.
    /// </summary>
    Closed = 1,
}
