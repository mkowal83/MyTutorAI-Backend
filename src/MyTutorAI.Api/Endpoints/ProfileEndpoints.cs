using MyTutorAI.Application.Contracts.Profile.Dtos;
using MyTutorAI.Application.Profile;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the profile group <c>/api/profile</c>; every endpoint requires an access token and returns only the data of the logged-in user. The group has no rate limiting.
/// PL: Endpointy Minimal API grupy profilu <c>/api/profile</c>; każdy endpoint wymaga access tokenu i zwraca wyłącznie dane zalogowanego użytkownika. Grupa nie ma rate limitingu.
/// </summary>
public static class ProfileEndpoints
{
    /// <summary>
    /// EN: Maps <c>GET /api/profile</c>, which returns the learner profile (declarative fields and semantic layers) as <see cref="StudentProfileDto"/> with 200, or 401 without a valid access token.
    /// PL: Mapuje <c>GET /api/profile</c>, który zwraca profil ucznia (pola deklaratywne i warstwy semantyczne) jako <see cref="StudentProfileDto"/> z kodem 200 albo 401 bez ważnego access tokenu.
    /// </summary>
    /// <param name="endpoints">
    /// EN: Route builder the endpoints are added to.
    /// PL: Builder tras, do którego dodawane są endpointy.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="endpoints"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="endpoints"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profile")
            .RequireAuthorization();

        group.MapGet("/", GetProfileAsync)
            .WithName("Profile_Get")
            .WithSummary("Returns the profile (declarative fields + semantic layers) of the logged-in student")
            .WithTags("Profile")
            .Produces<StudentProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static Task<StudentProfileDto> GetProfileAsync(
        GetProfileHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(cancellationToken);

}

