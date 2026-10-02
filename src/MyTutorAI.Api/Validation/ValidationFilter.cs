using FluentValidation;

namespace MyTutorAI.Api.Validation;

/// <summary>
/// EN: Endpoint filter that validates the <typeparamref name="TRequest"/> argument with the <see cref="IValidator{T}"/> resolved from DI before the endpoint handler runs.
/// PL: Filtr endpointu, który waliduje argument <typeparamref name="TRequest"/> za pomocą <see cref="IValidator{T}"/> pobranego z DI, zanim uruchomi się handler endpointu.
/// </summary>
/// <typeparam name="TRequest">
/// EN: Request DTO type bound by the endpoint; an <see cref="IValidator{T}"/> for it must be registered in DI.
/// PL: Typ DTO żądania wiązany przez endpoint; w DI musi być zarejestrowany <see cref="IValidator{T}"/> dla tego typu.
/// </typeparam>
public sealed class ValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    /// <summary>
    /// EN: Validates the first endpoint argument of type <typeparamref name="TRequest"/> and invokes <paramref name="next"/> only when validation succeeds.
    /// PL: Waliduje pierwszy argument endpointu typu <typeparamref name="TRequest"/> i wywołuje <paramref name="next"/> tylko przy poprawnej walidacji.
    /// </summary>
    /// <param name="context">
    /// EN: Invocation context with the bound endpoint arguments; its <c>RequestAborted</c> token cancels validation.
    /// PL: Kontekst wywołania z powiązanymi argumentami endpointu; jego token <c>RequestAborted</c> anuluje walidację.
    /// </param>
    /// <param name="next">
    /// EN: Next filter or the endpoint handler.
    /// PL: Kolejny filtr lub handler endpointu.
    /// </param>
    /// <returns>
    /// EN: Result of <paramref name="next"/>.
    /// PL: Wynik <paramref name="next"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: No <see cref="IValidator{T}"/> is registered for <typeparamref name="TRequest"/>, or the endpoint has no argument of that type.
    /// PL: W DI nie ma <see cref="IValidator{T}"/> dla <typeparamref name="TRequest"/> albo endpoint nie ma argumentu tego typu.
    /// </exception>
    /// <exception cref="ValidationException">
    /// EN: The request fails validation; the exception carries the per-field errors.
    /// PL: Żądanie nie przechodzi walidacji; wyjątek zawiera błędy poszczególnych pól.
    /// </exception>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetRequiredService<IValidator<TRequest>>();

        var request = context.Arguments.OfType<TRequest>().FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Endpoint with {nameof(ValidationFilter<TRequest>)}<{typeof(TRequest).Name}> " +
                $"does not accept an argument of type {typeof(TRequest).Name}.");

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}
