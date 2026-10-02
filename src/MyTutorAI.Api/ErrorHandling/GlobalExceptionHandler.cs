using System.Data.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyTutorAI.Application.Exceptions;

namespace MyTutorAI.Api.ErrorHandling;

/// <summary>
/// EN: Maps exceptions escaping endpoints to a consistent <c>application/problem+json</c> response: <see cref="ValidationException"/> → 400 with per-field errors, <see cref="LlmException"/> → 502, <see cref="DbException"/> (database unreachable or failing, provider-agnostic) → 503 with <c>Retry-After</c>, anything else → 500 without internal details.
/// PL: Mapuje wyjątki wychodzące z endpointów na spójną odpowiedź <c>application/problem+json</c>: <see cref="ValidationException"/> → 400 z błędami per pole, <see cref="LlmException"/> → 502, <see cref="DbException"/> (baza niedostępna lub zwraca błąd, niezależnie od providera) → 503 z <c>Retry-After</c>, wszystko inne → 500 bez szczegółów wewnętrznych.
/// </summary>
/// <param name="problemDetails">
/// EN: Writes the ProblemDetails body (adds <c>traceId</c> and content negotiation).
/// PL: Zapisuje body ProblemDetails (dodaje <c>traceId</c> i negocjację treści).
/// </param>
/// <param name="logger">
/// EN: Logs unexpected errors; request bodies are never logged.
/// PL: Loguje nieoczekiwane błędy; body żądań nigdy nie są logowane.
/// </param>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // Sugerowany klientowi czas (w sekundach) do ponowienia żądania po 503.
    private const string DatabaseRetryAfterSeconds = "30";

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;
        switch (exception)
        {
            case ValidationException validation:
                problem = new HttpValidationProblemDetails(
                    validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => ToCamelCase(g.Key), g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Status = StatusCodes.Status400BadRequest,
                };
                break;

            case LlmException:
                logger.LogWarning(exception, "LLM integration failure.");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "The language model did not return a valid response.",
                };
                break;

            case DbException:
                // Szczegóły ze stack trace loguje już EF Core (kategorie Database.Connection / Query / Update).
                logger.LogWarning("Database unavailable or query execution failed ({ExceptionType}).",
                    exception.GetType().Name);
                httpContext.Response.Headers.RetryAfter = DatabaseRetryAfterSeconds;
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Service temporarily unavailable. Please try again later.",
                };
                break;

            default:
                logger.LogError(exception, "Unhandled exception.");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected server error occurred.",
                };
                break;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    // Nazwy pól w błędach zgodne z JSON-em żądania (camelCase), a nie z właściwościami C#.
    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
