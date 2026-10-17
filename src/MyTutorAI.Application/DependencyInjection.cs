using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTutorAI.Application.Auth;
using MyTutorAI.Application.Contracts.Auth;
using MyTutorAI.Application.Demo;
using MyTutorAI.Application.Profile;
using MyTutorAI.Application.Sessions;

namespace MyTutorAI.Application;

/// <summary>
/// EN: Dependency injection registration of Application use-case handlers and validators, called from the Api composition root.
/// PL: Rejestracja handlerów przypadków użycia i walidatorów Application w DI, wywoływana z korzenia kompozycji w Api.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// EN: Registers the use-case handlers (<see cref="GenerateDialogueHandler"/>, <see cref="RegisterUserHandler"/>, <see cref="StartSessionHandler"/>, <see cref="LoginHandler"/>, <see cref="GetSessionStateHandler"/>, <see cref="SendMessageHandler"/>, <see cref="CloseSessionHandler"/>, <see cref="RefreshTokenHandler"/>, <see cref="LogoutHandler"/>, <see cref="GetCurrentUserHandler"/>, <see cref="GetProfileHandler"/>, <see cref="UpdateProfileHandler"/>) as scoped and all <see cref="IValidator{T}"/> implementations of this assembly as scoped (assembly scan). The handlers' ports (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>, <c>IUserRepository</c>, <c>IPasswordHasher</c>, <c>IClock</c>, <c>ICurrentUser</c>, <c>IJwtTokenService</c>, <c>IRefreshTokenRepository</c>, <c>ICoreMemoryRepository</c>, <c>IStudentProfileRepository</c>, <c>IProgressRepository</c>, <c>IEpisodicMemoryRepository</c>, <c>ISessionRepository</c>, <c>IWorkingMemoryStore</c>, <c>IAgentStateStore</c>) and <c>FeaturesOptions</c>, <c>JwtOptions</c>, <c>MemoryLimitsOptions</c> must be registered separately.
    /// PL: Rejestruje handlery przypadków użycia (<see cref="GenerateDialogueHandler"/>, <see cref="RegisterUserHandler"/>, <see cref="StartSessionHandler"/>, <see cref="LoginHandler"/>, <see cref="GetSessionStateHandler"/>, <see cref="SendMessageHandler"/>, <see cref="CloseSessionHandler"/>, <see cref="RefreshTokenHandler"/>, <see cref="LogoutHandler"/>, <see cref="GetCurrentUserHandler"/>, <see cref="GetProfileHandler"/>, <see cref="UpdateProfileHandler"/>) jako scoped oraz wszystkie implementacje <see cref="IValidator{T}"/> z tego assembly jako scoped (skan assembly). Porty handlerów (<c>IPromptRenderer</c>, <c>IStructuredOutputClient</c>, <c>IUserRepository</c>, <c>IPasswordHasher</c>, <c>IClock</c>, <c>ICurrentUser</c>, <c>IJwtTokenService</c>, <c>IRefreshTokenRepository</c>, <c>ICoreMemoryRepository</c>, <c>IStudentProfileRepository</c>, <c>IProgressRepository</c>, <c>IEpisodicMemoryRepository</c>, <c>ISessionRepository</c>, <c>IWorkingMemoryStore</c>, <c>IAgentStateStore</c>) oraz <c>FeaturesOptions</c>, <c>JwtOptions</c>, <c>MemoryLimitsOptions</c> trzeba zarejestrować osobno.
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

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<GenerateDialogueHandler>();

        services.AddScoped<RegisterUserHandler>();

        services.AddScoped<StartSessionHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<GetSessionStateHandler>();
        services.AddScoped<SendMessageHandler>();
        services.AddScoped<CloseSessionHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<GetProfileHandler>();
        services.AddScoped<UpdateProfileHandler>();

        return services;
    }
}
