using System.Collections.Concurrent;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: Provides one async lock per user that serializes loading and saving of that user's <see cref="Domain.Memory.AgentState"/> within this process. Registered as a singleton so the locks are shared across requests; thread-safe.
/// PL: Udostępnia jedną asynchroniczną blokadę na użytkownika, serializującą wczytywanie i zapis <see cref="Domain.Memory.AgentState"/> tego użytkownika w obrębie tego procesu. Rejestrowany jako singleton, aby blokady były współdzielone między żądaniami; thread-safe.
/// </summary>
/// <remarks>
/// EN: Locks are process-local and are never removed, so they do not coordinate multiple application instances.
/// PL: Blokady są lokalne dla procesu i nigdy nie są usuwane, więc nie koordynują wielu instancji aplikacji.
/// </remarks>
public sealed class AgentStateLockProvider
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    /// <summary>
    /// EN: Returns the lock of the user, creating it on first use.
    /// PL: Zwraca blokadę użytkownika, tworząc ją przy pierwszym użyciu.
    /// </summary>
    /// <param name="userId">
    /// EN: User whose lock is requested.
    /// PL: Użytkownik, którego blokada jest żądana.
    /// </param>
    /// <returns>
    /// EN: Semaphore allowing one holder at a time; always the same instance for the same user.
    /// PL: Semafor dopuszczający jednego posiadacza naraz; zawsze ta sama instancja dla tego samego użytkownika.
    /// </returns>
    public SemaphoreSlim GetLock(Guid userId) => _locks.GetOrAdd(userId, static _ => new SemaphoreSlim(1, 1));
}
