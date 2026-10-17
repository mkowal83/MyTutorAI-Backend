using MyTutorAI.Api.RateLimiting;
using MyTutorAI.Api.Validation;
using MyTutorAI.Application.Auth;
using MyTutorAI.Application.Contracts.Auth;

namespace MyTutorAI.Api.Endpoints;

/// <summary>
/// EN: Minimal API endpoints of the authentication group <c>/api/auth</c>; rate limited per client IP and anonymous except logout and current-user data, which require an access token.
/// PL: Endpointy Minimal API grupy uwierzytelniania <c>/api/auth</c>; z rate limitingiem per adres IP klienta i anonimowe z wyjątkiem wylogowania i danych bieżącego użytkownika, które wymagają access tokenu.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// EN: Maps <c>POST /api/auth/register</c>, <c>POST /api/auth/login</c>, <c>POST /api/auth/refresh</c>, <c>POST /api/auth/logout</c> and <c>GET /api/auth/me</c>. Register returns 202 without a body both for a new and an already registered e-mail, so it does not reveal which e-mails have accounts; login returns 200 with <see cref="AuthResponse"/> or 401 with the same message for an unknown e-mail and a wrong password; refresh rotates the refresh token and returns 200 with a new <see cref="AuthResponse"/>, 400 for a missing token or 401 with the same message for any invalid token; logout requires an access token and returns 204 also for an unknown, expired or another user's refresh token, 400 for a missing token without <c>AllDevices</c> or 401 without a valid access token; me requires an access token and returns 200 with <see cref="CurrentUserResponse"/>, 401 without a valid access token or 404 when the account no longer exists.
    /// PL: Mapuje <c>POST /api/auth/register</c>, <c>POST /api/auth/login</c>, <c>POST /api/auth/refresh</c>, <c>POST /api/auth/logout</c> i <c>GET /api/auth/me</c>. Rejestracja zwraca 202 bez body zarówno dla nowego, jak i już zarejestrowanego e-maila, więc nie zdradza, które e-maile mają konta; logowanie zwraca 200 z <see cref="AuthResponse"/> albo 401 z tym samym komunikatem dla nieznanego e-maila i błędnego hasła; odświeżenie rotuje refresh token i zwraca 200 z nowym <see cref="AuthResponse"/>, 400 dla brakującego tokenu albo 401 z tym samym komunikatem dla każdego nieprawidłowego tokenu; wylogowanie wymaga access tokenu i zwraca 204 także dla nieznanego, wygasłego lub cudzego refresh tokenu, 400 dla brakującego tokenu bez <c>AllDevices</c> albo 401 bez ważnego access tokenu; me wymaga access tokenu i zwraca 200 z <see cref="CurrentUserResponse"/>, 401 bez ważnego access tokenu albo 404, gdy konto już nie istnieje.
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

        group.MapPost("/refresh", RefreshAsync)
            .AddEndpointFilter<ValidationFilter<RefreshRequest>>()
            .WithName("Auth_Refresh")
            .WithSummary("Rotates refresh token - returns a new pair of tokens")
            .WithTags("Auth")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/logout", LogoutAsync)
            .AddEndpointFilter<ValidationFilter<LogoutRequest>>()
            .RequireAuthorization()
            .WithName("Auth_Logout")
            .WithSummary("The refresh token for the device—or for all devices—is activated.")
            .WithTags("Auth")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("Auth_GetCurrentUser")
            .WithSummary("Returns the account data of the logged-in user")
            .WithTags("Auth")
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
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

    private static Task<AuthResponse> RefreshAsync(
        RefreshRequest request, RefreshTokenHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(request, cancellationToken);

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request, LogoutHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static Task<CurrentUserResponse> GetCurrentUserAsync(
        GetCurrentUserHandler handler, CancellationToken cancellationToken)
        => handler.HandleAsync(cancellationToken);
}
