using MyTutorAI.Api.RateLimiting;
using MyTutorAI.Api.Validation;
using MyTutorAI.Application.Auth;
using MyTutorAI.Application.Contracts;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the authentication group <c>/api/auth</c>; anonymous and rate limited per client IP.
/// PL: Endpointy Minimal API grupy uwierzytelniania <c>/api/auth</c>; anonimowe i z rate limitingiem per adres IP klienta.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// EN: Maps <c>POST /api/auth/register</c>. The response is 202 without a body both for a new and an already registered e-mail, so the endpoint does not reveal which e-mails have accounts.
    /// PL: Mapuje <c>POST /api/auth/register</c>. Odpowiedź to 202 bez body zarówno dla nowego, jak i już zarejestrowanego e-maila, więc endpoint nie zdradza, które e-maile mają konta.
    /// </summary>
    /// <param name="endpoints">
    /// EN: Route builder the endpoints are added to.
    /// PL: Builder tras, do którego dodawane są endpointy.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="endpoints"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="endpoints"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .RequireRateLimiting(RateLimitingExtensions.AuthPolicy)
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
            .WithName("Auth_Register")
            .WithSummary("Registers a new account")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync( RegisterRequest request,
                                                      RegisterUserHandler handler,
                                                      CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, cancellationToken);
        return TypedResults.Accepted((string?)null);
    }
}
