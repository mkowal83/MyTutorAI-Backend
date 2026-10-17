namespace MyTutorAI.Application.Abstractions;

/// <summary>
/// EN: Application port exposing the identity of the user making the current request; implemented in Api (it reads the HTTP context) and registered as scoped.
/// PL: Port Application udostępniający tożsamość użytkownika wykonującego bieżące żądanie; implementowany w Api (czyta kontekst HTTP) i rejestrowany jako scoped.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// EN: Identifier of the authenticated user; data access must be scoped by this value.
    /// PL: Identyfikator uwierzytelnionego użytkownika; dostęp do danych musi być zawężany po tej wartości.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no current request or its principal carries no user id claim.
    /// PL: Nie ma bieżącego żądania albo jego principal nie zawiera claimu z identyfikatorem użytkownika.
    /// </exception>
    Guid UserId { get; }

    /// <summary>
    /// EN: True when the current request is authenticated; false when it is not or there is no current request. Never throws.
    /// PL: True, gdy bieżące żądanie jest uwierzytelnione; false, gdy nie jest lub nie ma bieżącego żądania. Nigdy nie zgłasza wyjątku.
    /// </summary>
    bool IsAuthenticated { get; }
}
