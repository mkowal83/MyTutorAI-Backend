using Microsoft.EntityFrameworkCore;
using MyTutorAI.Application.Abstractions;
using StackExchange.Redis;
using System.Data.Common;

namespace MyTutorAI.Infrastructure.Common;

/// <summary>
/// EN: <see cref="IStorageExceptionClassifier"/> for EF Core / SQL Server and StackExchange.Redis: <see cref="DbUpdateConcurrencyException"/> → <see cref="StorageFailureKind.Conflict"/>; <see cref="DbException"/>, <see cref="RedisConnectionException"/> and <see cref="RedisTimeoutException"/> → <see cref="StorageFailureKind.Unavailable"/>.
/// PL: <see cref="IStorageExceptionClassifier"/> dla EF Core / SQL Server i StackExchange.Redis: <see cref="DbUpdateConcurrencyException"/> → <see cref="StorageFailureKind.Conflict"/>; <see cref="DbException"/>, <see cref="RedisConnectionException"/> i <see cref="RedisTimeoutException"/> → <see cref="StorageFailureKind.Unavailable"/>.
/// </summary>
internal sealed class StorageExceptionClassifier : IStorageExceptionClassifier
{
    /// <inheritdoc />
    public StorageFailureKind Classify(Exception exception) => exception switch
    {
        DbUpdateConcurrencyException => StorageFailureKind.Conflict,
        DbException or RedisConnectionException or RedisTimeoutException => StorageFailureKind.Unavailable,
        _ => StorageFailureKind.None,
    };
}
