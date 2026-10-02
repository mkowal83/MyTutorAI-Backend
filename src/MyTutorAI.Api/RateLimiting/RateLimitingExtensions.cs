using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MyTutorAI.Api.RateLimiting;

/// <summary>
/// EN: Rate limiting policies of the Api; rejected requests get a 429 ProblemDetails response.
/// PL: Polityki rate limitingu Api; odrzucone żądania dostają odpowiedź 429 ProblemDetails.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// EN: Name of the policy for the anonymous authentication endpoints, partitioned by client IP address.
    /// PL: Nazwa polityki dla anonimowych endpointów uwierzytelniania, partycjonowanej po adresie IP klienta.
    /// </summary>
    public const string AuthPolicy = "auth";

    /// <summary>
    /// EN: Binds <see cref="RateLimitOptions"/> (DataAnnotations + <c>ValidateOnStart</c>) and registers the rate limiter with the <see cref="AuthPolicy"/> policy: a fixed window per client IP sized by the <c>Auth*</c> options.
    /// PL: Wiąże <see cref="RateLimitOptions"/> (DataAnnotations + <c>ValidateOnStart</c>) i rejestruje rate limiter z polityką <see cref="AuthPolicy"/>: stałe okno per adres IP klienta o parametrach z opcji <c>Auth*</c>.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetails.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests. Please try again later.",
                    },
                });
            };

            options.AddPolicy(AuthPolicy, httpContext =>
            {
                var limits = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.AuthPermitLimit,
                        Window = TimeSpan.FromSeconds(limits.AuthWindowSeconds),
                        QueueLimit = limits.AuthQueueLimit,
                    });
            });
        });

        return services;
    }
}
