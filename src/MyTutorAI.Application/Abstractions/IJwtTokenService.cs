using MyTutorAI.Domain.Users;

namespace MyTutorAI.Application.Abstractions;

/// <summary>
/// EN: Port issuing authentication tokens; implemented in Infrastructure and registered as a singleton. Expected to be thread-safe.
/// PL: Port wydający tokeny uwierzytelniające; implementowany w Infrastructure i rejestrowany jako singleton. Powinien być thread-safe.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// EN: Creates a signed JWT access token identifying the user; its issuer, audience and lifetime come from <c>JwtOptions</c>.
    /// PL: Tworzy podpisany access token JWT identyfikujący użytkownika; jego issuer, audience i czas życia pochodzą z <c>JwtOptions</c>.
    /// </summary>
    /// <param name="user">
    /// EN: Authenticated account; must not be null.
    /// PL: Uwierzytelnione konto; nie może być null.
    /// </param>
    /// <returns>
    /// EN: Compact serialized JWT, sent by the client as a Bearer token.
    /// PL: Zserializowany JWT w formie kompaktowej, wysyłany przez klienta jako token Bearer.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="user"/> is null.
    /// PL: <paramref name="user"/> jest null.
    /// </exception>
    string GenerateAccessToken(User user);

    /// <summary>
    /// EN: Generates a new random 256-bit refresh token in plaintext; the caller must hash it before storage and return the plaintext only to the client.
    /// PL: Generuje nowy losowy 256-bitowy refresh token w postaci jawnej; wywołujący musi go zahaszować przed zapisem i zwrócić postać jawną wyłącznie klientowi.
    /// </summary>
    /// <returns>
    /// EN: Opaque token string, unique per call with overwhelming probability.
    /// PL: Nieprzezroczysty ciąg tokenu, z ogromnym prawdopodobieństwem unikalny dla każdego wywołania.
    /// </returns>
    string GenerateRefreshToken();
}
