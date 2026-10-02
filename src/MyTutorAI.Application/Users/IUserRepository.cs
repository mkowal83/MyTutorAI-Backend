using MyTutorAI.Application.Exceptions;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Application.Users;

/// <summary>
/// EN: Port for persisting user accounts; the implementation lives in Infrastructure.
/// PL: Port utrwalania kont użytkowników; implementacja znajduje się w Infrastructure.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// EN: Finds an account by its identifier.
    /// PL: Wyszukuje konto po identyfikatorze.
    /// </summary>
    /// <param name="id">
    /// EN: Account identifier.
    /// PL: Identyfikator konta.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the query.
    /// PL: Anuluje zapytanie.
    /// </param>
    /// <returns>
    /// EN: The account, or <c>null</c> when it does not exist.
    /// PL: Konto albo <c>null</c>, gdy nie istnieje.
    /// </returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Finds an account by its e-mail address.
    /// PL: Wyszukuje konto po adresie e-mail.
    /// </summary>
    /// <param name="normalizedEmail">
    /// EN: E-mail already normalized with <see cref="User.NormalizeEmail"/>.
    /// PL: E-mail już znormalizowany przez <see cref="User.NormalizeEmail"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the query.
    /// PL: Anuluje zapytanie.
    /// </param>
    /// <returns>
    /// EN: The account, or <c>null</c> when it does not exist.
    /// PL: Konto albo <c>null</c>, gdy nie istnieje.
    /// </returns>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Persists a new account.
    /// PL: Zapisuje nowe konto.
    /// </summary>
    /// <param name="user">
    /// EN: Account to store; must not be null.
    /// PL: Konto do zapisania; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the write.
    /// PL: Anuluje zapis.
    /// </param>
    /// <exception cref="DuplicateEmailException">
    /// EN: An account with the same e-mail already exists (e.g. a concurrent registration won the race).
    /// PL: Konto z tym samym e-mailem już istnieje (np. równoległa rejestracja wygrała wyścig).
    /// </exception>
    Task AddAsync(User user, CancellationToken cancellationToken);
}
