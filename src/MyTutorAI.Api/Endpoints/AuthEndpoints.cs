using MyTutorAI.Api.RateLimiting;
using MyTutorAI.Api.Validation;
using MyTutorAI.Application.Auth;
using MyTutorAI.Application.Contracts.Auth;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the authentication group <c>/api/auth</c>; anonymous and rate limited per client IP.
/// PL: Endpointy Minimal API grupy uwierzytelniania <c>/api/auth</c>; anonimowe i z rate limitingiem per adres IP klienta.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// EN: Maps <c>POST /api/auth/register</c> and <c>POST /api/auth/login</c>. Register returns 202 without a body both for a new and an already registered e-mail, so it does not reveal which e-mails have accounts; login returns 200 with <see cref="AuthResponse"/> or 401 with the same message for an unknown e-mail and a wrong password.
    /// PL: Mapuje <c>POST /api/auth/register</c> i <c>POST /api/auth/login</c>. Rejestracja zwraca 202 bez body zarówno dla nowego, jak i już zarejestrowanego e-maila, więc nie zdradza, które e-maile mają konta; logowanie zwraca 200 z <see cref="AuthResponse"/> albo 401 z tym samym komunikatem dla nieznanego e-maila i błędnego hasła.
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

        group.MapPost("/login", LoginAsync)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .WithName("Auth_Login")
            .WithSummary("Logs in the account — pair of tokens (access + refresh)")
            .WithTags("Auth")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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

    private static Task<AuthResponse> LoginAsync(
        LoginRequest request, LoginHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(request, cancellationToken);
}
