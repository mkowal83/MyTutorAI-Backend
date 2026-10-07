using MyTutorAI.Domain.Memory.Episodic;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port to the durable store of the learner's episodic memory, one record per user; implemented in Infrastructure.
/// PL: Port do trwałego magazynu pamięci epizodycznej ucznia, jeden rekord na użytkownika; implementowany w Infrastructure.
/// </summary>
public interface IEpisodicMemoryRepository
{
    /// <summary>
    /// EN: Loads the episodic memory of the given user.
    /// PL: Wczytuje pamięć epizodyczną danego użytkownika.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the memory; only that user's record is read.
    /// PL: Właściciel pamięci; odczytywany jest wyłącznie rekord tego użytkownika.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read.
    /// PL: Anuluje odczyt z bazy danych.
    /// </param>
    /// <returns>
    /// EN: Stored episodic memory, or a new empty <see cref="EpisodicMemory"/> with the default limit when nothing has been saved; never null.
    /// PL: Zapisana pamięć epizodyczna albo nowa, pusta <see cref="EpisodicMemory"/> z domyślnym limitem, gdy nic nie zapisano; nigdy null.
    /// </returns>
    Task<EpisodicMemory> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Persists the whole episodic memory of the given user, inserting it on first save and replacing it afterwards.
    /// PL: Utrwala całą pamięć epizodyczną danego użytkownika, wstawiając ją przy pierwszym zapisie i zastępując przy kolejnych.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the memory; the record is stored under this user.
    /// PL: Właściciel pamięci; rekord jest zapisywany dla tego użytkownika.
    /// </param>
    /// <param name="episodic">
    /// EN: Memory to save; must not be null.
    /// PL: Pamięć do zapisania; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database write.
    /// PL: Anuluje zapis do bazy danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="episodic"/> is null.
    /// PL: <paramref name="episodic"/> jest null.
    /// </exception>
    Task SaveAsync(Guid userId, EpisodicMemory episodic, CancellationToken cancellationToken);
}

