using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyTutorAI.Infrastructure.Persistence;

/// <summary>
/// EN: Brings the SQL Server schema of <see cref="AppDbContext"/> up to date at application startup, so a local environment needs no <c>dotnet ef</c> tooling. Called by the Api host in the Development environment only.
/// PL: Aktualizuje schemat SQL Server dla <see cref="AppDbContext"/> przy starcie aplikacji, dzięki czemu środowisko lokalne nie wymaga narzędzia <c>dotnet ef</c>. Wywoływane przez host Api wyłącznie w środowisku Development.
/// </summary>
public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// EN: Creates the database if it does not exist and applies all pending EF Core migrations; when the schema is already current, only logs that fact.
    /// PL: Tworzy bazę danych, jeśli nie istnieje, i stosuje wszystkie oczekujące migracje EF Core; gdy schemat jest aktualny, tylko to loguje.
    /// </summary>
    /// <remarks>
    /// EN: Idempotent: a repeated call with no pending migrations changes nothing. There is no connection retry, so an unreachable SQL Server fails the call and, when awaited before the host runs, stops the application from starting.
    /// PL: Idempotentne: ponowne wywołanie bez oczekujących migracji niczego nie zmienia. Nie ma ponawiania połączenia, więc niedostępny SQL Server kończy wywołanie błędem i, gdy jest awaitowane przed uruchomieniem hosta, zatrzymuje start aplikacji.
    /// </remarks>
    /// <param name="services">
    /// EN: Root service provider of the built host; a dedicated DI scope is created from it for the scoped <see cref="AppDbContext"/>.
    /// PL: Główny service provider zbudowanego hosta; tworzony jest z niego osobny scope DI dla scoped <see cref="AppDbContext"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels reading the migration history and applying migrations.
    /// PL: Anuluje odczyt historii migracji i stosowanie migracji.
    /// </param>
    /// <returns>
    /// EN: A task that completes when the schema matches the latest migration.
    /// PL: Zadanie, które kończy się, gdy schemat odpowiada najnowszej migracji.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="services"/> is null.
    /// PL: <paramref name="services"/> jest null.
    /// </exception>
    /// <exception cref="Microsoft.Data.SqlClient.SqlException">
    /// EN: SQL Server is unreachable, the login fails, or a migration is rejected by the server.
    /// PL: SQL Server jest niedostępny, logowanie się nie powiodło albo serwer odrzucił migrację.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// EN: <paramref name="cancellationToken"/> was cancelled.
    /// PL: <paramref name="cancellationToken"/> został anulowany.
    /// </exception>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseMigrationExtensions));

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database schema is up to date");
            return;
        }

        logger.LogInformation("Applying {Count} pending database migration(s)", pending.Count);
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied");
    }
}
