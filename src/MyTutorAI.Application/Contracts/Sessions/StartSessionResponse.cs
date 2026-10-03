using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Response DTO describing a newly opened session.
/// PL: DTO odpowiedzi opisujące nowo otwartą sesję.
/// </summary>
/// <param name="SessionId">
/// EN: Identifier of the created session.
/// PL: Identyfikator utworzonej sesji.
/// </param>
/// <param name="Phase">
/// EN: Opening phase of the session.
/// PL: Faza otwarcia sesji.
/// </param>
/// <param name="Topic">
/// EN: Trimmed session topic.
/// PL: Przycięty temat sesji.
/// </param>
/// <param name="Opener">
/// EN: First tutor message shown to the user.
/// PL: Pierwsza wiadomość tutora pokazywana użytkownikowi.
/// </param>
/// <param name="StartedAt">
/// EN: UTC moment the session was started.
/// PL: Moment rozpoczęcia sesji w UTC.
/// </param>
public sealed record StartSessionResponse(
    Guid SessionId,
    SessionPhase Phase,
    string Topic,
    string Opener,
    DateTimeOffset StartedAt);
