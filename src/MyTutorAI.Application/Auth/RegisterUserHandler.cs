using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Contracts.Auth;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Application.Users;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Use case creating a new account with a hashed password and a unique e-mail. The outcome is deliberately indistinguishable for a free and an already registered e-mail, so the caller cannot enumerate accounts. Issues no tokens.
/// PL: Przypadek użycia tworzący nowe konto z zahaszowanym hasłem i unikalnym e-mailem. Wynik jest celowo nieodróżnialny dla wolnego i już zarejestrowanego e-maila, więc wywołujący nie może enumerować kont. Nie wydaje tokenów.
/// </summary>
public sealed class RegisterUserHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;

    /// <summary>
    /// EN: Creates the handler.
    /// PL: Tworzy handler.
    /// </summary>
    /// <param name="users">
    /// EN: Account store.
    /// PL: Magazyn kont.
    /// </param>
    /// <param name="passwordHasher">
    /// EN: Hashes the password before storage.
    /// PL: Haszuje hasło przed zapisem.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the account creation time.
    /// PL: Źródło czasu utworzenia konta.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public RegisterUserHandler(IUserRepository users, IPasswordHasher passwordHasher, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(clock);

        _users = users;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    /// <summary>
    /// EN: Registers the account unless the normalized e-mail is already taken, in which case it silently does nothing. The password is hashed in both cases, so response time does not reveal whether the e-mail exists.
    /// PL: Rejestruje konto, chyba że znormalizowany e-mail jest już zajęty — wtedy po cichu nic nie robi. Hasło jest haszowane w obu przypadkach, więc czas odpowiedzi nie zdradza, czy e-mail istnieje.
    /// </summary>
    /// <param name="request">
    /// EN: Validated registration request; must not be null.
    /// PL: Zwalidowane żądanie rejestracji; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    public async Task HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = User.NormalizeEmail(request.Email);
        var passwordHash = _passwordHasher.Hash(request.Password);

        if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return;
        }

        var user = new User(Guid.NewGuid(), email, passwordHash, request.DisplayName, _clock.UtcNow);

        try
        {
            await _users.AddAsync(user, cancellationToken);
        }
        catch (DuplicateEmailException)
        {
            // A concurrent registration of the same email won the race; the result must match the already-taken email case.
        }
    }
}
