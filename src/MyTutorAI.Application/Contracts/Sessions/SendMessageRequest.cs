namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Request DTO of <c>POST /api/sessions/{sessionId}/messages</c>: one learner message sent to the tutor.
/// PL: DTO żądania <c>POST /api/sessions/{sessionId}/messages</c>: jedna wiadomość ucznia wysłana do tutora.
/// </summary>
/// <param name="Message">
/// EN: Learner's message; required, at most 4000 characters (see <see cref="SendMessageRequestValidator"/>); surrounding whitespace is trimmed before use.
/// PL: Wiadomość ucznia; wymagana, najwyżej 4000 znaków (zob. <see cref="SendMessageRequestValidator"/>); białe znaki na brzegach są przycinane przed użyciem.
/// </param>
public sealed record SendMessageRequest(string Message);