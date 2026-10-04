using System.Security.Cryptography;
using System.Text;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Computes the stored form of a refresh token, so the database never holds the plaintext token and a token can still be looked up by its hash.
/// PL: Wylicza zapisywaną postać refresh tokenu, dzięki czemu baza nigdy nie przechowuje tokenu w postaci jawnej, a token nadal można wyszukać po hashu.
/// </summary>
public static class RefreshTokenHasher
{
    /// <summary>
    /// EN: Hashes the token with SHA-256 over its UTF-8 bytes; deterministic and thread-safe.
    /// PL: Haszuje token algorytmem SHA-256 po jego bajtach UTF-8; deterministyczne i thread-safe.
    /// </summary>
    /// <param name="plaintextToken">
    /// EN: Plaintext refresh token; must not be null, empty or whitespace.
    /// PL: Refresh token w postaci jawnej; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <returns>
    /// EN: 64 uppercase hexadecimal characters.
    /// PL: 64 znaki szesnastkowe pisane wielkimi literami.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="plaintextToken"/> is null, empty or whitespace (<see cref="ArgumentNullException"/> for null).
    /// PL: <paramref name="plaintextToken"/> jest null, pusty lub składa się z samych białych znaków (<see cref="ArgumentNullException"/> dla null).
    /// </exception>
    public static string Hash(string plaintextToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintextToken);

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(plaintextToken));
        return Convert.ToHexString(digest);
    }
}
