using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port for persisting the per-user learning progress (part of the semantic memory layer) in SQL Server; implemented in Infrastructure and registered as scoped.
/// PL: Port utrwalania postępu nauki per użytkownik (część warstwy pamięci semantycznej) w SQL Server; implementowany w Infrastructure i rejestrowany jako scoped.
/// </summary>
public interface IProgressRepository
{
    /// <summary>
    /// EN: Loads the user's learning progress without tracking changes.
    /// PL: Wczytuje postęp nauki użytkownika bez śledzenia zmian.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the progress.
    /// PL: Właściciel postępu.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database query.
    /// PL: Anuluje zapytanie do bazy danych.
    /// </param>
    /// <returns>
    /// EN: The stored progress, or a new empty <see cref="LearningProgress"/> when nothing has been saved for the user.
    /// PL: Zapisany postęp albo nowy pusty <see cref="LearningProgress"/>, gdy dla użytkownika nic nie zapisano.
    /// </returns>
    Task<LearningProgress> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Inserts or updates the user's learning progress and saves it immediately.
    /// PL: Wstawia lub aktualizuje postęp nauki użytkownika i od razu go utrwala.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the progress; used as the record key.
    /// PL: Właściciel postępu; używany jako klucz rekordu.
    /// </param>
    /// <param name="progress">
    /// EN: Progress to store; must not be null.
    /// PL: Postęp do zapisania; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="progress"/> is null.
    /// PL: <paramref name="progress"/> jest null.
    /// </exception>
    Task SaveAsync(Guid userId, LearningProgress progress, CancellationToken cancellationToken);
}