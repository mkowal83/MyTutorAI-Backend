using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Auth;
using IdentityHasher = Microsoft.AspNetCore.Identity.PasswordHasher<MyTutorAI.Domain.Users.User>;
using IdentityVerificationResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace MyTutorAI.Infrastructure.Security;

/// <summary>
/// EN: <see cref="IPasswordHasher"/> adapter over ASP.NET Core Identity's <c>PasswordHasher</c> (PBKDF2 with HMAC-SHA512, random salt, versioned format). The Identity hasher ignores the user instance, so none is passed.
/// PL: Adapter <see cref="IPasswordHasher"/> na <c>PasswordHasher</c> z ASP.NET Core Identity (PBKDF2 z HMAC-SHA512, losowa sól, wersjonowany format). Hasher Identity ignoruje instancję użytkownika, więc żadna nie jest przekazywana.
/// </summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly IdentityHasher _inner = new();

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return _inner.HashPassword(null!, password);
    }

    /// <inheritdoc />
    public PasswordVerificationResult Verify(string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);
        ArgumentNullException.ThrowIfNull(providedPassword);

        return _inner.VerifyHashedPassword(null!, hashedPassword, providedPassword) switch
        {
            IdentityVerificationResult.Success => PasswordVerificationResult.Success,
            IdentityVerificationResult.SuccessRehashNeeded => PasswordVerificationResult.SuccessRehashNeeded,
            _ => PasswordVerificationResult.Failed,
        };
    }
}
