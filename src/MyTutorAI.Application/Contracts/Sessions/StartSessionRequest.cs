namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Request DTO for opening a new session of the current user.
/// PL: DTO żądania otwarcia nowej sesji bieżącego użytkownika.
/// </summary>
/// <param name="Topic">
/// EN: Session topic; trimmed by the handler, and an empty or whitespace value is rejected by the domain.
/// PL: Temat sesji; przycinany przez handler, a wartość pusta lub z samych białych znaków jest odrzucana przez domenę.
/// </param>
public sealed record StartSessionRequest(string Topic);
