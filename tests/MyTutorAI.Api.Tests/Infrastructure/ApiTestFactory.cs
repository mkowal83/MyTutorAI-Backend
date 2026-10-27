using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyTutorAI.Application.Llm;
using MyTutorAI.Application.Options;
using MyTutorAI.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace MyTutorAI.Api.Tests.Infrastructure;

/// <summary>
/// EN: API host for integration tests: disposable SQL Server and Redis containers (Testcontainers, requires Docker) started once per test collection, a test-only JWT configuration, and the LLM client replaced by <see cref="FakeStructuredOutputClient"/>.
/// PL: Host API dla testów integracyjnych: jednorazowe kontenery SQL Server i Redis (Testcontainers, wymaga Dockera) uruchamiane raz na kolekcję testów, testowa konfiguracja JWT, oraz klient LLM zastąpiony przez <see cref="FakeStructuredOutputClient"/>.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    /// <summary>
    /// EN: LLM client used by the host; tests read its call counters.
    /// PL: Klient LLM używany przez host; testy czytają jego liczniki wywołań.
    /// </summary>
    public FakeStructuredOutputClient Llm { get; } = new();

    /// <summary>
    /// EN: Starts both containers, then builds the host and applies the EF migrations to the fresh database.
    /// PL: Uruchamia oba kontenery, potem buduje host i nakłada migracje EF na świeżą bazę.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_sql.StartAsync(), _redis.StartAsync());

        // The host is built lazily on first access to Services, so the container connection strings are already known.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _sql.DisposeAsync();
        await _redis.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"] = _sql.GetConnectionString(),
                [$"{RedisOptions.SectionName}:{nameof(RedisOptions.ConnectionString)}"] = _redis.GetConnectionString(),
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}"] = "integration-tests",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = "integration-tests",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)}"] = "integration-tests-signing-key-0123456789abcdef",
                // TestServer has no client IP, so all auth requests share one rate limit partition.
                ["RateLimit:AuthPermitLimit"] = "10000",
                // Keeps the test output readable (no per-command EF logs).
                ["Logging:LogLevel:Default"] = "Warning",
                // Required by ValidateOnStart; the real LLM client is replaced below.
                [$"{LlmOptions.SectionName}:{nameof(LlmOptions.ApiKey)}"] = "integration-tests",
                [$"{LlmOptions.SectionName}:{nameof(LlmOptions.Model)}"] = "integration-tests",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IStructuredOutputClient>();
            services.AddSingleton<IStructuredOutputClient>(Llm);
        });
    }
}
