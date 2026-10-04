using MyTutorAI.Application.Auth;

namespace MyTutorAI.Application.Abstractions;

/// <summary>
/// EN: Port for one-way password hashing; the implementation lives in Infrastructure.
/// PL: Port jednokierunkowego haszowania haseł; implementacja znajduje się w Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// EN: Hashes a plaintext password with a random salt.
    /// PL: Haszuje hasło w postaci jawnej z losową solą.
    /// </summary>
    /// <param name="password">
    /// EN: Plaintext password; must not be null.
    /// PL: Hasło w postaci jawnej; nie może być null.
    /// </param>
    /// <returns>
    /// EN: Self-describing hash (algorithm, parameters, salt) ready for storage.
    /// PL: Samoopisujący się hash (algorytm, parametry, sól) gotowy do zapisu.
    /// </returns>
    string Hash(string password);

    /// <summary>
    /// EN: Checks a plaintext password against a stored hash.
    /// PL: Sprawdza hasło w postaci jawnej względem zapisanego hasha.
    /// </summary>
    /// <param name="hashedPassword">
    /// EN: Hash previously produced by <see cref="Hash"/>.
    /// PL: Hash wygenerowany wcześniej przez <see cref="Hash"/>.
    /// </param>
    /// <param name="providedPassword">
    /// EN: Plaintext password to verify.
    /// PL: Hasło w postaci jawnej do weryfikacji.
    /// </param>
    /// <returns>
    /// EN: Verification outcome; <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> means the hash uses outdated parameters and should be replaced.
    /// PL: Wynik weryfikacji; <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> oznacza, że hash używa przestarzałych parametrów i powinien zostać zastąpiony.
    /// </returns>
    PasswordVerificationResult Verify(string hashedPassword, string providedPassword);
}
