using Microsoft.Extensions.Options;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Domain.Memory.Working;
using System.Text.Json;
using StackExchange.Redis;
using MyTutorAI.Application.Options;

namespace MyTutorAI.Infrastructure.Redis;

/// <summary>
/// EN: Redis implementation of <see cref="IWorkingMemoryStore"/>: each session is a Redis list of JSON-serialized <see cref="ChatTurn"/> entries (snake_case) whose time-to-live comes from <see cref="RedisOptions.WorkingMemoryTtlMinutes"/>. Registered in DI as a singleton; Redis connectivity failures surface as StackExchange.Redis exceptions.
/// PL: Implementacja <see cref="IWorkingMemoryStore"/> na Redis: każda sesja to lista Redis z wpisami <see cref="ChatTurn"/> serializowanymi do JSON (snake_case), której TTL pochodzi z <see cref="RedisOptions.WorkingMemoryTtlMinutes"/>. Rejestrowana w DI jako singleton; błędy połączenia z Redis wychodzą jako wyjątki StackExchange.Redis.
/// </summary>
public sealed class WorkingMemoryStore : IWorkingMemoryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly IDatabase _database;
    private readonly TimeSpan _ttl;

    /// <summary>
    /// EN: Creates the store; the TTL is read once from the options at construction time.
    /// PL: Tworzy magazyn; TTL jest odczytywany z opcji jednorazowo przy tworzeniu.
    /// </summary>
    /// <param name="database">
    /// EN: Redis database used for all commands. Must not be null.
    /// PL: Baza Redis używana do wszystkich komend. Nie może być null.
    /// </param>
    /// <param name="options">
    /// EN: Redis settings providing the Working memory TTL. Must not be null.
    /// PL: Ustawienia Redis dostarczające TTL pamięci Working. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="database"/> or <paramref name="options"/> is null.
    /// PL: <paramref name="database"/> lub <paramref name="options"/> jest null.
    /// </exception>
    public WorkingMemoryStore(IDatabase database, IOptions<RedisOptions> options)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(options);

        _database = database;
        _ttl = TimeSpan.FromMinutes(options.Value.WorkingMemoryTtlMinutes);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EN: Reading does not refresh the TTL.
    /// PL: Odczyt nie odświeża TTL.
    /// </remarks>
    public async Task<WorkingMemory> GetAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entries = await _database.ListRangeAsync(Key(userId, sessionId)).ConfigureAwait(false);

        var working = new WorkingMemory();
        foreach (var entry in entries)
        {
            var turn = Deserialize(entry);
            working.Append(turn.Role, turn.Content);
        }

        return working;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EN: The push and the TTL refresh are two separate Redis commands, not an atomic operation; the token is not passed to Redis, so cancellation after the check does not stop the write.
    /// PL: Dopisanie i odświeżenie TTL to dwie osobne komendy Redis, a nie operacja atomowa; token nie jest przekazywany do Redis, więc anulowanie po sprawdzeniu nie zatrzymuje zapisu.
    /// </remarks>
    public async Task AppendAsync(
        Guid userId, Guid sessionId, ChatRole role, string content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = Key(userId, sessionId);
        var turn = new ChatTurn(role, content);
        await _database.ListRightPushAsync(key, Serialize(turn)).ConfigureAwait(false);
        await _database.KeyExpireAsync(key, _ttl).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task ClearAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _database.KeyDeleteAsync(Key(userId, sessionId));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EN: Reads only the requested tail of the Redis list; reading does not refresh the TTL.
    /// PL: Czyta tylko żądany koniec listy Redis; odczyt nie odświeża TTL.
    /// </remarks>
    public async Task<IReadOnlyList<ChatTurn>> GetContextWindowAsync(
        Guid userId, Guid sessionId, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var entries = await _database.ListRangeAsync(Key(userId, sessionId), -limit, -1).ConfigureAwait(false);
        return entries.Select(Deserialize).ToArray();
    }

    // Key format "wm:{userId}:{sessionId}" — the user id in the key isolates sessions between users.
    private static string Key(Guid userId, Guid sessionId) => $"wm:{userId}:{sessionId}";

    private static RedisValue Serialize(ChatTurn turn) => JsonSerializer.Serialize(turn, SerializerOptions);

    private static ChatTurn Deserialize(RedisValue entry) =>
        JsonSerializer.Deserialize<ChatTurn>(entry.ToString(), SerializerOptions)!;
}