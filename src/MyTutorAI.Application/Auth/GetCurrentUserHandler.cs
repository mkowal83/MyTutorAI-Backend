using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Contracts.Auth;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Users;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Use case that returns the account data of the current user, identified through <see cref="ICurrentUser"/> and loaded through <see cref="IUserRepository"/>. Read-only.
/// PL: Przypadek użycia zwracający dane konta bieżącego użytkownika, zidentyfikowanego przez <see cref="ICurrentUser"/> i wczytanego przez <see cref="IUserRepository"/>. Tylko odczyt.
/// </summary>
public sealed class GetCurrentUserHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _users;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="users">
    /// EN: Store from which the account is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywane jest konto. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetCurrentUserHandler(ICurrentUser currentUser, IUserRepository users)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(users);

        _currentUser = currentUser;
        _users = users;
    }

    /// <summary>
    /// EN: Returns the account data of the current user.
    /// PL: Zwraca dane konta bieżącego użytkownika.
    /// </summary>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read.
    /// PL: Anuluje odczyt z bazy danych.
    /// </param>
    /// <returns>
    /// EN: Identifier, normalized e-mail, display name and creation time of the account.
    /// PL: Identyfikator, znormalizowany e-mail, nazwa wyświetlana i czas utworzenia konta.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// EN: No account exists for the user id from the token.
    /// PL: Nie istnieje konto dla identyfikatora użytkownika z tokenu.
    /// </exception>
    public async Task<CurrentUserResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return new CurrentUserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAt);
    }
}

