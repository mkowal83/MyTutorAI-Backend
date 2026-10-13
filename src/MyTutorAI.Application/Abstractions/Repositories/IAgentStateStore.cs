using MyTutorAI.Domain.Memory;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port that loads and saves the full <see cref="AgentState"/> of a learner's session as one unit, on top of the individual memory layer repositories. Implemented in Infrastructure; used by message-handling use cases.
/// PL: Port, który wczytuje i zapisuje pełny <see cref="AgentState"/> sesji ucznia jako jedną całość, ponad repozytoriami poszczególnych warstw pamięci. Implementowany w Infrastructure; używany przez przypadki użycia obsługujące wiadomości.
/// </summary>
/// <remarks>
/// EN: Load and save are serialized per user, but no lock is held between them, so the LLM call made in between never blocks other requests of the user.
/// PL: Odczyt i zapis są serializowane per użytkownik, ale między nimi nie jest trzymana żadna blokada, więc wywołanie LLM wykonywane pomiędzy nimi nigdy nie blokuje innych żądań użytkownika.
/// </remarks>
public interface IAgentStateStore
{
    /// <summary>
    /// EN: Assembles the state of an existing session of the user: Core, Profile, Progress and Episodic layers from SQL Server, Workflow (topic and phase) from the session, and Working memory turns from Redis. A layer with no stored data is returned with its defaults.
    /// PL: Składa stan istniejącej sesji użytkownika: warstwy Core, Profile, Progress i Episodic z SQL Server, Workflow (temat i faza) z sesji oraz tury pamięci Working z Redis. Warstwa bez zapisanych danych zwracana jest z wartościami domyślnymi.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session; every read is scoped to this user.
    /// PL: Właściciel sesji; każdy odczyt jest zawężony do tego użytkownika.
    /// </param>
    /// <param name="sessionId">
    /// EN: Session to load.
    /// PL: Sesja do wczytania.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels waiting for the per-user lock and the storage reads.
    /// PL: Anuluje oczekiwanie na blokadę użytkownika i odczyty z magazynów.
    /// </param>
    /// <returns>
    /// EN: New state instance detached from storage; changes are persisted only by <see cref="SaveAsync"/>.
    /// PL: Nowa instancja stanu odłączona od magazynów; zmiany utrwala dopiero <see cref="SaveAsync"/>.
    /// </returns>
    /// <exception cref="Exceptions.SessionNotFoundException">
    /// EN: The session does not exist or belongs to another user.
    /// PL: Sesja nie istnieje lub należy do innego użytkownika.
    /// </exception>
    Task<AgentState> LoadAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Persists the Core, Profile, Progress and Episodic layers of <paramref name="state"/> in SQL Server, keyed by <see cref="AgentState.UserId"/>. Working memory and the session's Workflow phase are not saved here; the caller persists them separately.
    /// PL: Utrwala warstwy Core, Profile, Progress i Episodic z <paramref name="state"/> w SQL Server, kluczowane przez <see cref="AgentState.UserId"/>. Pamięć Working i faza Workflow sesji nie są tu zapisywane; wywołujący utrwala je osobno.
    /// </summary>
    /// <param name="state">
    /// EN: State to persist; must not be null.
    /// PL: Stan do utrwalenia; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels waiting for the per-user lock and the writes; layers already saved stay saved.
    /// PL: Anuluje oczekiwanie na blokadę użytkownika i zapisy; warstwy już zapisane pozostają zapisane.
    /// </param>
    /// <returns>
    /// EN: Task completed when all four layers are saved.
    /// PL: Zadanie zakończone po zapisaniu wszystkich czterech warstw.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="state"/> is null.
    /// PL: <paramref name="state"/> jest null.
    /// </exception>
    Task SaveAsync(AgentState state, CancellationToken cancellationToken);
}
