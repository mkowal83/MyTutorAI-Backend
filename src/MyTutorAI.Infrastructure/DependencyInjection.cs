using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Common;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Options;
using MyTutorAI.Application.Users;
using MyTutorAI.Infrastructure.Auth;
using MyTutorAI.Infrastructure.Common;
using MyTutorAI.Infrastructure.Llm;
using MyTutorAI.Infrastructure.Persistence;
using MyTutorAI.Infrastructure.Persistence.Repository;
using MyTutorAI.Infrastructure.Prompting;
using MyTutorAI.Infrastructure.Redis;
using MyTutorAI.Infrastructure.Security;
using StackExchange.Redis;

namespace MyTutorAI.Infrastructure;

/// <summary>
/// EN: Dependency injection registration of Infrastructure services, called from the Api composition root.
/// PL: Rejestracja usług Infrastructure w DI, wywoływana z korzenia kompozycji w Api.
/// </summary>
public static class DependencyInjection
{
    private const string PromptsDirectoryName = "Prompts";

    /// <summary>
    /// EN: Registers <see cref="IPromptRenderer"/> (Scriban) and its template source as singletons. With <paramref name="useDiskTemplates"/> templates are read from the <c>Prompts</c> directory next to the assembly, falling back to the embedded resources when a file or the directory is missing; otherwise only the embedded resources are used.
    /// PL: Rejestruje <see cref="IPromptRenderer"/> (Scriban) i jego źródło szablonów jako singletony. Przy <paramref name="useDiskTemplates"/> szablony są czytane z katalogu <c>Prompts</c> obok assembly, z fallbackiem do zasobów osadzonych, gdy brakuje pliku lub katalogu; w przeciwnym razie używane są tylko zasoby osadzone.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <param name="useDiskTemplates">
    /// EN: True in Development to allow editing templates without rebuilding (a restart is still needed because parsed templates are cached).
    /// PL: True w Development, aby edytować szablony bez przebudowy (restart nadal jest potrzebny, bo sparsowane szablony są cache'owane).
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddPromptRendering(this IServiceCollection services, bool useDiskTemplates)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPromptTemplateSource>(_ =>
        {
            var embedded = new EmbeddedResourcePromptTemplateSource();
            var promptsDirectory = Path.Combine(AppContext.BaseDirectory, PromptsDirectoryName);

            return useDiskTemplates && Directory.Exists(promptsDirectory)
                ? new PhysicalFileProviderPromptTemplateSource(new PhysicalFileProvider(promptsDirectory), embedded)
                : embedded;
        });
        services.AddSingleton<IPromptRenderer, ScribanPromptRenderer>();

        return services;
    }

    /// <summary>
    /// EN: Registers <see cref="IStructuredOutputClient"/> as a typed <see cref="HttpClient"/> client. <c>LlmOptions</c> must be bound by the caller; no HTTP resilience handler is registered here.
    /// PL: Rejestruje <see cref="IStructuredOutputClient"/> jako typowanego klienta <see cref="HttpClient"/>. <c>LlmOptions</c> musi podpiąć wywołujący; nie rejestruje tu handlera odporności HTTP.
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
    public static IServiceCollection AddLlmClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IStructuredOutputClient, StructuredOutputClient>();

        return services;
    }

    /// <summary>
    /// EN: Registers the SQL Server <see cref="AppDbContext"/> and the repositories as scoped, and the Redis connection with <see cref="IWorkingMemoryStore"/> and <see cref="IStorageExceptionClassifier"/> as singletons; <see cref="IAgentStateStore"/> is scoped and its per-user lock provider a singleton. The SQL connection string is read from <see cref="DatabaseOptions"/>, which must be bound (with <c>ValidateOnStart</c>) by the caller; <see cref="RedisOptions"/> and <see cref="MemoryLimitsOptions"/> are bound here with DataAnnotations validation and <c>ValidateOnStart</c>. The Redis multiplexer does not abort when the server is unreachable — it reconnects in the background and individual commands fail instead. The schema is managed by EF migrations applied outside the application.
    /// PL: Rejestruje <see cref="AppDbContext"/> dla SQL Server i repozytoria jako scoped oraz połączenie Redis z <see cref="IWorkingMemoryStore"/> i <see cref="IStorageExceptionClassifier"/> jako singletony; <see cref="IAgentStateStore"/> jest scoped, a jego dostawca blokad per użytkownik — singletonem. Connection string SQL jest czytany z <see cref="DatabaseOptions"/>, które musi podpiąć wywołujący (z <c>ValidateOnStart</c>); <see cref="RedisOptions"/> i <see cref="MemoryLimitsOptions"/> są podpinane tutaj z walidacją DataAnnotations i <c>ValidateOnStart</c>. Multiplekser Redis nie przerywa działania, gdy serwer jest niedostępny — łączy się ponownie w tle, a błędem kończą się pojedyncze komendy. Schemat jest zarządzany migracjami EF stosowanymi poza aplikacją.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <param name="configuration">
    /// EN: Application configuration containing the <c>MemoryLimits</c> and <c>Redis</c> sections.
    /// PL: Konfiguracja aplikacji zawierająca sekcje <c>MemoryLimits</c> i <c>Redis</c>.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<MemoryLimitsOptions>()
            .Bind(configuration.GetSection(MemoryLimitsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));

        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(sp.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString);
            // Keep retrying in the background instead of throwing when Redis is unreachable at first use.
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        services.AddSingleton<IWorkingMemoryStore, WorkingMemoryStore>();
        services.AddSingleton<IStorageExceptionClassifier, StorageExceptionClassifier>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICoreMemoryRepository, CoreMemoryRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IEpisodicMemoryRepository, EpisodicMemoryRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        // Per-user locks must be shared across requests, so the provider is a singleton.
        services.AddSingleton<AgentStateLockProvider>();
        services.AddScoped<IAgentStateStore, AgentStateStore>();

        return services;
    }

    /// <summary>
    /// EN: Registers the authentication services: <see cref="IPasswordHasher"/> (PBKDF2), <see cref="IClock"/> and <see cref="IJwtTokenService"/> as singletons; <see cref="ICurrentUser"/> is registered by the Api. Also binds <see cref="JwtOptions"/> with DataAnnotations validation and <c>ValidateOnStart</c>, so missing or invalid JWT configuration stops the application at startup.
    /// PL: Rejestruje usługi uwierzytelniania: <see cref="IPasswordHasher"/> (PBKDF2), <see cref="IClock"/> i <see cref="IJwtTokenService"/> jako singletony; <see cref="ICurrentUser"/> rejestruje Api. Wiąże też <see cref="JwtOptions"/> z walidacją DataAnnotations i <c>ValidateOnStart</c>, więc brakująca lub błędna konfiguracja JWT zatrzymuje aplikację przy starcie.
    /// </summary>
    /// <param name="services">
    /// EN: Service collection to extend. Must not be null.
    /// PL: Kolekcja usług do rozszerzenia. Nie może być null.
    /// </param>
    /// <param name="configuration">
    /// EN: Application configuration holding the <c>Jwt</c> section.
    /// PL: Konfiguracja aplikacji z sekcją <c>Jwt</c>.
    /// </param>
    /// <returns>
    /// EN: The same <paramref name="services"/> instance, for chaining.
    /// PL: Ta sama instancja <paramref name="services"/>, do łańcuchowania wywołań.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
