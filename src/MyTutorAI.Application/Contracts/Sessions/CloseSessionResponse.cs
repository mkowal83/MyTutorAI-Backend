using MyTutorAI.Application.Contracts.Sessions.Dtos;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Response DTO of <c>POST /api/sessions/{sessionId}/close</c>: the episode the closed session was compressed into and the phase the next session will open with.
/// PL: DTO odpowiedzi <c>POST /api/sessions/{sessionId}/close</c>: epizod, do którego skompresowano zamkniętą sesję, oraz faza, od której zacznie się następna sesja.
/// </summary>
/// <param name="Episode">
/// EN: Episode saved in the learner's episodic memory for the closed session.
/// PL: Epizod zapisany w pamięci epizodycznej ucznia dla zamkniętej sesji.
/// </param>
/// <param name="NextOpenPhase">
/// EN: Opening phase of the next session: <see cref="SessionPhase.HomeworkCheck"/> when homework is assigned or recent mistakes exist, otherwise <see cref="SessionPhase.Practice"/>.
/// PL: Faza otwarcia następnej sesji: <see cref="SessionPhase.HomeworkCheck"/>, gdy zadanie domowe jest przydzielone lub istnieją ostatnie błędy, w przeciwnym razie <see cref="SessionPhase.Practice"/>.
/// </param>
public sealed record CloseSessionResponse(EpisodeDto Episode, SessionPhase NextOpenPhase);