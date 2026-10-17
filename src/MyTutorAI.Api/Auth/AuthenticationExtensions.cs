using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Options;

namespace MyTutorAI.Api.Auth;

/// <summary>
/// EN: JWT Bearer authentication of the Api, validating access tokens issued by <c>JwtTokenService</c>.
/// PL: Uwierzytelnianie JWT Bearer w Api, walidujące access tokeny wydawane przez <c>JwtTokenService</c>.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// EN: Registers JWT Bearer as the default authentication scheme, the authorization services, the <c>IHttpContextAccessor</c> and <see cref="ICurrentUser"/> (<see cref="CurrentUser"/>) as scoped. Validation parameters (issuer, audience, HMAC-SHA256 signing key) come from <see cref="JwtOptions"/>, which must be bound by the caller (<c>AddAuthServices</c>).
    /// PL: Rejestruje JWT Bearer jako domyślny schemat uwierzytelniania, usługi autoryzacji, <c>IHttpContextAccessor</c> oraz <see cref="ICurrentUser"/> (<see cref="CurrentUser"/>) jako scoped. Parametry walidacji (issuer, audience, klucz podpisu HMAC-SHA256) pochodzą z <see cref="JwtOptions"/>, które musi podpiąć wywołujący (<c>AddAuthServices</c>).
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
