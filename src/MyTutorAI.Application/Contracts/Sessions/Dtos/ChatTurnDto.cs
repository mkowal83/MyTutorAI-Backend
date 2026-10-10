using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Contracts.Sessions.Dtos;

/// <summary>
/// EN: DTO of a single conversation turn from the session's Working memory history.
/// PL: DTO pojedynczej tury rozmowy z historii pamięci Working sesji.
/// </summary>
/// <param name="Role">
/// EN: Author of the turn: learner, tutor or system.
/// PL: Autor tury: uczeń, tutor lub system.
/// </param>
/// <param name="Content">
/// EN: Message content; trimmed and never empty.
/// PL: Treść wiadomości; przycięta i nigdy pusta.
/// </param>
public sealed record ChatTurnDto(ChatRole Role, string Content);
