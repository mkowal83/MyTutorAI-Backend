using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Profile;
using MyTutorAI.Application.Contracts.Profile.Dtos;

namespace MyTutorAI.Application.Profile;

/// <summary>
/// EN: Use case that returns the learner profile of the current user, identified through <see cref="ICurrentUser"/> and loaded through <see cref="IStudentProfileRepository"/>. Read-only.
/// PL: Przypadek użycia zwracający profil ucznia bieżącego użytkownika, zidentyfikowanego przez <see cref="ICurrentUser"/> i wczytanego przez <see cref="IStudentProfileRepository"/>. Tylko odczyt.
/// </summary>
public sealed class GetProfileHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IStudentProfileRepository _profiles;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="profiles">
    /// EN: Store from which the profile is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywany jest profil. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetProfileHandler(ICurrentUser currentUser, IStudentProfileRepository profiles)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(profiles);

        _currentUser = currentUser;
        _profiles = profiles;
    }

    /// <summary>
    /// EN: Returns the profile of the current user; never fails because the profile is missing.
    /// PL: Zwraca profil bieżącego użytkownika; nigdy nie kończy się błędem z powodu braku profilu.
    /// </summary>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read.
    /// PL: Anuluje odczyt z bazy danych.
    /// </param>
    /// <returns>
    /// EN: The stored profile, or a default unsaved profile (default name, level and goal, empty semantic layers) when the user has none yet.
    /// PL: Zapisany profil albo domyślny, niezapisany profil (domyślne imię, poziom i cel, puste warstwy semantyczne), gdy użytkownik jeszcze go nie ma.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no authenticated user id in the current request.
    /// PL: Bieżące żądanie nie zawiera identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    public async Task<StudentProfileDto> HandleAsync(CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(_currentUser.UserId, cancellationToken);

        return StudentProfileMapper.ToDto(profile);
    }
}