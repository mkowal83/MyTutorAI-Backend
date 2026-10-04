using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Options;
using MyTutorAI.Domain.Users;
using System.Security.Cryptography;
using System.Text;

namespace MyTutorAI.Infrastructure.Auth;

/// <summary>
/// EN: <see cref="IJwtTokenService"/> issuing HMAC-SHA256 signed JWTs with settings from <see cref="JwtOptions"/>; registered as a singleton and thread-safe. The tokens are validated by the Api JWT Bearer authentication with the same options.
/// PL: <see cref="IJwtTokenService"/> wydający JWT podpisane HMAC-SHA256 z ustawieniami z <see cref="JwtOptions"/>; rejestrowany jako singleton i thread-safe. Tokeny są walidowane przez uwierzytelnianie JWT Bearer w Api z tymi samymi opcjami.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private static readonly JsonWebTokenHandler Handler = new();

    private readonly JwtOptions _options;
    private readonly IClock _clock;

    /// <summary>
    /// EN: Creates the service.
    /// PL: Tworzy serwis.
    /// </summary>
    /// <param name="options">
    /// EN: JWT settings; the value is read once at construction.
    /// PL: Ustawienia JWT; wartość jest odczytywana raz przy konstrukcji.
    /// </param>
    /// <param name="clock">
    /// EN: Source of the issuance time.
    /// PL: Źródło czasu wydania.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);

        _options = options.Value;
        _clock = clock;
    }

    /// <inheritdoc />
    /// <remarks>
    /// EN: Claims: <c>sub</c> (user id), <c>email</c>, a unique <c>jti</c>; <c>iat</c> and <c>nbf</c> equal the current time, <c>exp</c> is now plus <see cref="JwtOptions.AccessTokenMinutes"/>.
    /// PL: Claimy: <c>sub</c> (id użytkownika), <c>email</c>, unikalny <c>jti</c>; <c>iat</c> i <c>nbf</c> równe bieżącemu czasowi, <c>exp</c> to teraz plus <see cref="JwtOptions.AccessTokenMinutes"/>.
    /// </remarks>
    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = _clock.UtcNow.UtcDateTime;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_options.AccessTokenMinutes),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            },
        };

        return Handler.CreateToken(descriptor);
    }

    /// <inheritdoc />
    /// <remarks>
    /// EN: 32 bytes from a cryptographically secure generator, encoded as standard Base64 (may contain <c>+</c>, <c>/</c> and <c>=</c>).
    /// PL: 32 bajty z kryptograficznie bezpiecznego generatora, zakodowane standardowym Base64 (może zawierać <c>+</c>, <c>/</c> i <c>=</c>).
    /// </remarks>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
