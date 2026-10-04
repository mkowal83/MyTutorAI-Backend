using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port for persisting the learner's semantic profile in SQL Server, one profile per user; implemented in Infrastructure and registered as scoped.
/// PL: Port utrwalania semantycznego profilu ucznia w SQL Server, jeden profil na użytkownika; implementowany w Infrastructure i rejestrowany jako scoped.
/// </summary>
public interface IStudentProfileRepository
{
    /// <summary>
    /// EN: Loads the user's profile without tracking changes.
    /// PL: Wczytuje profil użytkownika bez śledzenia zmian.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the profile.
    /// PL: Właściciel profilu.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database query.
    /// PL: Anuluje zapytanie do bazy danych.
    /// </param>
    /// <returns>
    /// EN: The stored profile, or a new unsaved profile with default name, level and goal when none exists.
    /// PL: Zapisany profil albo nowy, niezapisany profil z domyślnym imieniem, poziomem i celem, gdy profil nie istnieje.
    /// </returns>
    Task<StudentProfile> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Inserts or updates the profile, keyed by <see cref="StudentProfile.UserId"/>, and saves it immediately.
    /// PL: Wstawia lub aktualizuje profil, kluczowany przez <see cref="StudentProfile.UserId"/>, i od razu go utrwala.
    /// </summary>
    /// <param name="profile">
    /// EN: Profile to store; must not be null.
    /// PL: Profil do zapisania; nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database operations.
    /// PL: Anuluje operacje na bazie danych.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="profile"/> is null.
    /// PL: <paramref name="profile"/> jest null.
    /// </exception>
    Task SaveAsync(StudentProfile profile, CancellationToken cancellationToken);
}