using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTutorAI.Application.Auth;
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
    /// EN: Registers the use-case handlers (<see cref="GenerateDialogueHandler"/>, <see cref="RegisterUserHandler"/>) as scoped and their validators as singleton <see cref="IValidator{T}"/> instances. The handlers' ports (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>, <c>IUserRepository</c>, <c>IPasswordHasher</c>, <c>IClock</c>) and <c>FeaturesOptions</c> must be registered separately.
    /// PL: Rejestruje handlery przypadków użycia (<see cref="GenerateDialogueHandler"/>, <see cref="RegisterUserHandler"/>) jako scoped, a ich walidatory jako singletonowe <see cref="IValidator{T}"/>. Porty handlerów (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>, <c>IUserRepository</c>, <c>IPasswordHasher</c>, <c>IClock</c>) i <c>FeaturesOptions</c> trzeba zarejestrować osobno.
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

        services.AddScoped<RegisterUserHandler>();
        services.AddSingleton<IValidator<RegisterRequest>, RegisterRequestValidator>();

        return services;
    }
}
