using Microsoft.AspNetCore.Http;
using MyTutorAI.Application.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyTutorAI.Api.Auth;

/// <summary>
/// EN: <see cref="ICurrentUser"/> implementation that reads the user from the current <see cref="HttpContext"/>; registered in DI as scoped.
/// PL: Implementacja <see cref="ICurrentUser"/> czytająca użytkownika z bieżącego <see cref="HttpContext"/>; rejestrowana w DI jako scoped.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// EN: Creates the service over the HTTP context accessor.
    /// PL: Tworzy usługę opartą na akcesorze kontekstu HTTP.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// EN: Accessor of the current HTTP request. Must not be null.
    /// PL: Akcesor bieżącego żądania HTTP. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="httpContextAccessor"/> is null.
    /// PL: <paramref name="httpContextAccessor"/> jest null.
    /// </exception>
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// EN: True when the request principal's identity is authenticated; false when there is no HTTP context or identity.
    /// PL: True, gdy tożsamość principala żądania jest uwierzytelniona; false, gdy brak kontekstu HTTP lub tożsamości.
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// EN: User id parsed from the <see cref="ClaimTypes.NameIdentifier"/> claim, falling back to the JWT <c>sub</c> claim.
    /// PL: Identyfikator użytkownika odczytany z claimu <see cref="ClaimTypes.NameIdentifier"/>, z fallbackiem do claimu JWT <c>sub</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no HTTP context or user, or neither claim is present.
    /// PL: Brak kontekstu HTTP lub użytkownika albo nie ma żadnego z claimów.
    /// </exception>
    /// <exception cref="FormatException">
    /// EN: The claim value is not a valid GUID.
    /// PL: Wartość claimu nie jest poprawnym GUID.
    /// </exception>
    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException(
                    "No HttpContext/User available — CurrentUser.UserId requires an authenticated request.");

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? throw new InvalidOperationException(
                    "No user id claim (ClaimTypes.NameIdentifier / sub) present on the current principal.");

            return Guid.Parse(claim.Value);
        }
    }
}
