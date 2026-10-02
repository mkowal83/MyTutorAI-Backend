namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Outcome of <see cref="IPasswordHasher.Verify"/>.
/// PL: Wynik <see cref="IPasswordHasher.Verify"/>.
/// </summary>
public enum PasswordVerificationResult
{
    /// <summary>
    /// EN: The password does not match the hash.
    /// PL: Hasło nie pasuje do hasha.
    /// </summary>
    Failed = 0,

    /// <summary>
    /// EN: The password matches the hash.
    /// PL: Hasło pasuje do hasha.
    /// </summary>
    Success = 1,

    /// <summary>
    /// EN: The password matches, but the hash should be recomputed with current parameters.
    /// PL: Hasło pasuje, ale hash należy przeliczyć z bieżącymi parametrami.
    /// </summary>
    SuccessRehashNeeded = 2,
}
