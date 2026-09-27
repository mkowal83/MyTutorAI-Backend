using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTutorAI.Application.Contracts;
using MyTutorAI.Application.Demo;

namespace MyTutorAI.Application;

/// <summary>
/// EN: Dependency injection registration of Application use-case handlers and validators, called from the Api composition root.
/// PL: Rejestracja handlerów przypadków użycia i walidatorów Application w DI, wywoływana z korzenia kompozycji w Api.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// EN: Registers <see cref="GenerateDialogueHandler"/> as scoped and <see cref="DialogueGenerationRequestValidator"/> as the singleton <see cref="IValidator{T}"/> of <see cref="DialogueGenerationRequest"/>. The handler's ports (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>) and <c>FeaturesOptions</c> must be registered separately.
    /// PL: Rejestruje <see cref="GenerateDialogueHandler"/> jako scoped oraz <see cref="DialogueGenerationRequestValidator"/> jako singletonowy <see cref="IValidator{T}"/> dla <see cref="DialogueGenerationRequest"/>. Porty handlera (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>) i <c>FeaturesOptions</c> trzeba zarejestrować osobno.
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
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<GenerateDialogueHandler>();
        services.AddSingleton<IValidator<DialogueGenerationRequest>, DialogueGenerationRequestValidator>();

        return services;
    }
}
