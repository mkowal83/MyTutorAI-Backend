using MyTutorAI.Application.Contracts.Profile.Dtos;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Contracts.Profile;

/// <summary>
/// EN: Maps the domain <see cref="StudentProfile"/> to the API contract <see cref="StudentProfileDto"/>. Pure, stateless and thread-safe; no I/O.
/// PL: Mapuje domenowy <see cref="StudentProfile"/> na kontrakt API <see cref="StudentProfileDto"/>. Czysty, bezstanowy i thread-safe; bez I/O.
/// </summary>
public static class StudentProfileMapper
{
    /// <summary>
    /// EN: Copies every field of the profile one to one; collections and semantic layers are passed by reference, not cloned.
    /// PL: Kopiuje każde pole profilu jeden do jednego; kolekcje i warstwy semantyczne są przekazywane przez referencję, bez klonowania.
    /// </summary>
    /// <param name="profile">
    /// EN: Profile to map; must not be null.
    /// PL: Profil do zmapowania; nie może być null.
    /// </param>
    /// <returns>
    /// EN: DTO with the same values as <paramref name="profile"/>.
    /// PL: DTO z tymi samymi wartościami co <paramref name="profile"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="profile"/> is null.
    /// PL: <paramref name="profile"/> jest null.
    /// </exception>
    public static StudentProfileDto ToDto(StudentProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new StudentProfileDto(
            profile.UserId,
            profile.Name,
            profile.Level,
            profile.Goal,
            profile.NativeLanguage,
            profile.TargetLanguage,
            profile.Preferences,
            profile.Goals,
            profile.WorkContext,
            profile.CommunicationPreferences,
            profile.PersonalContext,
            profile.SemanticSignals,
            profile.PerformanceProfile,
            profile.ContextNotes);
    }
}

