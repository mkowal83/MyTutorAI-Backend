using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Exceptions;

namespace MyTutorAI.Api.ErrorHandling;


/// <summary>
/// EN: Single place that turns unhandled exceptions into the API's uniform <c>application/problem+json</c> error format with a predictable HTTP status: <see cref="ValidationException"/> → 400 with per-field errors, <see cref="BadHttpRequestException"/> (unreadable body, e.g. malformed JSON) → its own status (usually 400) with a generic detail, <see cref="UnauthorizedException"/> → 401, <see cref="NotFoundException"/>, <see cref="FeatureDisabledException"/> and <see cref="SessionNotFoundException"/> → 404, <see cref="ConflictException"/>, <see cref="EmptySessionException"/> and <see cref="StorageFailureKind.Conflict"/> → 409, <see cref="LlmException"/> → 502, <see cref="StorageFailureKind.Unavailable"/> → 503 with <c>Retry-After</c>, anything else → 500. Storage exceptions (SQL Server, Redis) are recognised through <see cref="IStorageExceptionClassifier"/>, so the Api does not depend on their types. Registered as a singleton by <c>AddExceptionHandler</c>.
/// PL: Jedno miejsce zamieniające nieobsłużone wyjątki na jednolity format błędów API <c>application/problem+json</c> z przewidywalnym statusem HTTP: <see cref="ValidationException"/> → 400 z błędami pól, <see cref="BadHttpRequestException"/> (nieczytelne body, np. błędny JSON) → jego własny status (zwykle 400) z ogólnym opisem, <see cref="UnauthorizedException"/> → 401, <see cref="NotFoundException"/>, <see cref="FeatureDisabledException"/> i <see cref="SessionNotFoundException"/> → 404, <see cref="ConflictException"/>, <see cref="EmptySessionException"/> i <see cref="StorageFailureKind.Conflict"/> → 409, <see cref="LlmException"/> → 502, <see cref="StorageFailureKind.Unavailable"/> → 503 z <c>Retry-After</c>, wszystko inne → 500. Wyjątki magazynów (SQL Server, Redis) są rozpoznawane przez <see cref="IStorageExceptionClassifier"/>, więc Api nie zależy od ich typów. Rejestrowany jako singleton przez <c>AddExceptionHandler</c>.
/// </summary>
/// <param name="problemDetailsService">
/// EN: Writes the problem response.
/// PL: Zapisuje odpowiedź z opisem problemu.
/// </param>
/// <param name="logger">
/// EN: Receives the full exception: Error level for 500, Warning for mapped exceptions.
/// PL: Otrzymuje pełny wyjątek: poziom Error dla 500, Warning dla zmapowanych wyjątków.
/// </param>
/// <param name="storageExceptionClassifier">
/// EN: Recognises storage conflicts (→ 409) and unavailability (→ 503).
/// PL: Rozpoznaje konflikty (→ 409) i niedostępność magazynów (→ 503).
/// </param>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger,
    IStorageExceptionClassifier storageExceptionClassifier) : IExceptionHandler
{
    // Suggested client retry delay (seconds) after a 503.
    private const string StorageRetryAfterSeconds = "30";

    // Client-safe detail for unreadable request bodies (malformed JSON, wrong shape, too large).
    private const string BadRequestDetail = "The request body is not valid JSON or does not match the expected format.";

    /// <summary>
    /// EN: Logs the exception, sets the status code and writes the problem body. <c>Exception.Message</c> is returned as <c>detail</c> except for 409, 500 and 503, where it is hidden; validation error keys are camelCase to match the request JSON.
    /// PL: Loguje wyjątek, ustawia kod statusu i zapisuje body problemu. <c>Exception.Message</c> jest zwracany jako <c>detail</c>, z wyjątkiem 409, 500 i 503, gdzie jest ukrywany; klucze błędów walidacji są w camelCase, zgodnie z JSON-em żądania.
    /// </summary>
    /// <param name="httpContext">
    /// EN: Context of the failed request; its response is modified.
    /// PL: Kontekst nieudanego żądania; jego odpowiedź jest modyfikowana.
    /// </param>
    /// <param name="exception">
    /// EN: Exception to translate.
    /// PL: Wyjątek do przetłumaczenia.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Not used; the write is not cancellable here.
    /// PL: Nieużywany; zapis nie jest tu anulowalny.
    /// </param>
    /// <returns>
    /// EN: <c>true</c> when the problem response was written; <c>false</c> lets the next handler or the default middleware respond.
    /// PL: <c>true</c>, gdy odpowiedź z problemem została zapisana; <c>false</c> pozwala odpowiedzieć kolejnemu handlerowi lub domyślnemu middleware.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapToProblem(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Handled exception {ExceptionType} → HTTP {StatusCode} for {Method} {Path}",
                exception.GetType().Name,
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        if (statusCode == StatusCodes.Status503ServiceUnavailable)
        {
            httpContext.Response.Headers.RetryAfter = StorageRetryAfterSeconds;
        }

        ProblemDetails problemDetails = exception is ValidationException validationException
            ? new HttpValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => ToCamelCase(g.Key), g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = statusCode,
                Title = title,
            }
            : new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                // For 409/500/503 we don't expose Exception.Message to the client (it could contain internal
                // details/stack trace) — the full exception has already been sent to ILogger above.
                Detail = exception is BadHttpRequestException
                    // Framework message names C# parameter types (e.g. "SendMessageRequest request") — hide it.
                    ? BadRequestDetail
                    : statusCode is StatusCodes.Status409Conflict
                                 or StatusCodes.Status500InternalServerError
                                 or StatusCodes.Status503ServiceUnavailable
                        ? null
                        : exception.Message,
            };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private (int StatusCode, string Title) MapToProblem(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Request validation error"),
        NotFoundException or FeatureDisabledException or SessionNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        LlmException => (StatusCodes.Status502BadGateway, "LLM service error"),
        ConflictException or EmptySessionException => (StatusCodes.Status409Conflict, "Conflict error"),
        BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request"),
        _ => storageExceptionClassifier.Classify(exception) switch
        {
            StorageFailureKind.Conflict => (StatusCodes.Status409Conflict, "The resource was modified by another request. Please try again."),
            StorageFailureKind.Unavailable => (StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable. Please try again later."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected server error occurred"),
        },
    };

    // Field names in errors match the request JSON (camelCase), not the C# properties.
    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}