namespace MyTutorAI.Application.Common;

/// <summary>
/// EN: Port for the current time, so handlers do not read the system clock directly.
/// PL: Port bieżącego czasu, aby handlery nie odczytywały bezpośrednio zegara systemowego.
/// </summary>
public interface IClock
{
    /// <summary>
    /// EN: Current moment in UTC.
    /// PL: Bieżący moment w UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
