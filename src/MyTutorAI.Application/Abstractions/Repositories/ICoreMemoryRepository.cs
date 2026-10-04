using MyTutorAI.Domain.Memory.Core;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port for persisting the per-user Core memory layer (coach identity and prime directives) in SQL Server; implemented in Infrastructure and registered as scoped.
/// PL: Port utrwalania warstwy pamięci Core per użytkownik (tożsamość coacha i nadrzędne dyrektywy) w SQL Server; implementowany w Infrastructure i rejestrowany jako scoped.
/// </summary>
public interface ICoreMemoryRepository
{
    /// <summary>
    /// EN: Loads the user's Core memory without tracking changes.
    /// PL: Wczytuje pamięć Core użytkownika bez śledzenia zmian.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the Core memory.
    /// PL: Właściciel pamięci Core.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database query.
    /// PL: Anuluje zapytanie do bazy danych.
    /// </param>
    /// <returns>
    /// EN: The stored Core memory, or <see cref="CoreMemory.Default"/> when nothing has been saved for the user.
    /// PL: Zapisana pamięć Core albo <see cref="CoreMemory.Default"/>, gdy dla użytkownika nic nie zapisano.
    /// </returns>
    Task<CoreMemory> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Inserts or updates the user's Core memory and saves it immediately.
    /// PL: Wstawia lub aktualizuje pamięć Core użytkownika i od razu ją utrwala.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the Core memory; used as the record key.
    /// PL: Właściciel pamięci Core; używany jako klucz rekordu.
    /// </param>
    /// <param name="core">
    /// EN: Core memory to store; must not be null.
    /// PL: Pamięć Core do zapisania; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="core"/> is null.
    /// PL: <paramref name="core"/> jest null.
    /// </exception>
    Task SaveAsync(Guid userId, CoreMemory core, CancellationToken cancellationToken);
}
