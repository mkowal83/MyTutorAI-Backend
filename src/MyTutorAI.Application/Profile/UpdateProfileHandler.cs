using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Profile;
using MyTutorAI.Application.Contracts.Profile.Dtos;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Profile;

/// <summary>
/// EN: Use case that overwrites the declarative profile fields of the current user, identified through <see cref="ICurrentUser"/>, and saves them through <see cref="IStudentProfileRepository"/>. Semantic layers built by the LLM are preserved.
/// PL: Przypadek użycia nadpisujący deklaratywne pola profilu bieżącego użytkownika, zidentyfikowanego przez <see cref="ICurrentUser"/>, i zapisujący je przez <see cref="IStudentProfileRepository"/>. Warstwy semantyczne budowane przez LLM są zachowywane.
/// </summary>
public sealed class UpdateProfileHandler
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
    /// EN: Store from which the profile is loaded and to which it is saved. Must not be null.
    /// PL: Magazyn, z którego profil jest wczytywany i do którego jest zapisywany. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public UpdateProfileHandler(ICurrentUser currentUser, IStudentProfileRepository profiles)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(profiles);

        _currentUser = currentUser;
        _profiles = profiles;
    }

    /// <summary>
    /// EN: Overwrites the declarative fields of the current user's profile and saves it immediately; <c>null</c> collections and sections clear the stored values.
    /// PL: Nadpisuje deklaratywne pola profilu bieżącego użytkownika i od razu go zapisuje; kolekcje i sekcje <c>null</c> czyszczą zapisane wartości.
    /// </summary>
    /// <param name="request">
    /// EN: New declarative profile data, already validated by <see cref="UpdateProfileRequestValidator"/>. Must not be null.
    /// PL: Nowe deklaratywne dane profilu, zwalidowane wcześniej przez <see cref="UpdateProfileRequestValidator"/>. Nie może być null.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read and write.
    /// PL: Anuluje odczyt i zapis w bazie danych.
    /// </param>
    /// <returns>
    /// EN: The profile as saved, including the unchanged semantic layers.
    /// PL: Profil w zapisanej postaci, razem z niezmienionymi warstwami semantycznymi.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="request"/> is null.
    /// PL: <paramref name="request"/> jest null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no authenticated user id in the current request.
    /// PL: Bieżące żądanie nie zawiera identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    /// <remarks>
    /// EN: When the user has no stored profile yet, the first call creates it; the high-stakes situations of the work context are always carried over from the stored profile.
    /// PL: Gdy użytkownik nie ma jeszcze zapisanego profilu, pierwsze wywołanie go tworzy; sytuacje o wysokiej stawce z kontekstu zawodowego są zawsze przenoszone z zapisanego profilu.
    /// </remarks>
    public async Task<StudentProfileDto> HandleAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Existing aggregate — carries the immutable UserId and the LLM semantic layers that must be preserved.
        var profile = await _profiles.GetAsync(_currentUser.UserId, cancellationToken);

        // 2. Identity and learning configuration.
        profile.UpdateLearningBasics(
            request.Name,
            request.Level,
            request.Goal,
            request.NativeLanguage,
            request.TargetLanguage);

        // 3. Declarative collections — null = clear.
        profile.SetPreferences(request.Preferences ?? []);
        profile.SetGoals((request.Goals ?? []).Select(ToLearnerGoal));

        // 4. Work context — scalars + Stakeholders from the request; HighStakesSituations (LLM)
        //    is carried over from the existing VO via the `with` expression.
        var work = request.WorkContext;
        profile.SetWorkContext(profile.WorkContext with
        {
            Profession = work?.Profession,
            CompanyName = work?.CompanyName,
            Industry = work?.Industry,
            Stakeholders = work?.Stakeholders ?? [],
        });

        // 5. Communication preferences — full replacement (this VO has no LLM fields).
        var comm = request.CommunicationPreferences;
        profile.SetCommunicationPreferences(new CommunicationPreferences(
            comm?.FeedbackStyle,
            comm?.CorrectionIntensity,
            comm?.Pace,
            comm?.PreferredFormats ?? []));

        // 6. Persist the profile layer only.
        await _profiles.SaveAsync(profile, cancellationToken);

        return StudentProfileMapper.ToDto(profile);
    }

    private static LearnerGoal ToLearnerGoal(ProfileGoalInput input) =>
        new(input.Goal, input.Reason, input.TargetSituation, input.SuccessMetric, input.Deadline);
}