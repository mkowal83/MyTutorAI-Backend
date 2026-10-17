namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Response DTO with the account data of the authenticated user; returned by <see cref="MyTutorAI.Application.Auth.GetCurrentUserHandler"/>.
/// PL: DTO odpowiedzi z danymi konta uwierzytelnionego użytkownika; zwracane przez <see cref="MyTutorAI.Application.Auth.GetCurrentUserHandler"/>.
/// </summary>
/// <param name="UserId">
/// EN: Account identifier.
/// PL: Identyfikator konta.
/// </param>
/// <param name="Email">
/// EN: Normalized e-mail address (trimmed, lowercase), unique across all accounts.
/// PL: Znormalizowany adres e-mail (przycięty, małe litery), unikalny wśród wszystkich kont.
/// </param>
/// <param name="DisplayName">
/// EN: Trimmed name shown to the student.
/// PL: Przycięta nazwa pokazywana uczniowi.
/// </param>
/// <param name="CreatedAt">
/// EN: Moment the account was created (UTC).
/// PL: Moment utworzenia konta (UTC).
/// </param>
public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAt);
