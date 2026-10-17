using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Application.Contracts.Profile.Dtos;

/// <summary>
/// EN: Response of <c>GET /api/profile</c>: the declarative learning basics and the semantic layers of the logged-in learner's profile. The semantic layers are exposed as the domain types from <c>MyTutorAI.Domain.Memory.Semantic</c>.
/// PL: Odpowiedź <c>GET /api/profile</c>: zadeklarowane podstawy nauki i warstwy semantyczne profilu zalogowanego ucznia. Warstwy semantyczne są wystawiane jako typy domenowe z <c>MyTutorAI.Domain.Memory.Semantic</c>.
/// </summary>
/// <param name="UserId">
/// EN: Identifier of the profile owner, equal to the user id from the access token.
/// PL: Identyfikator właściciela profilu, równy identyfikatorowi użytkownika z access tokenu.
/// </param>
/// <param name="Name">
/// EN: Learner's first name.
/// PL: Imię ucznia.
/// </param>
/// <param name="Level">
/// EN: Language level, e.g. <c>"B1"</c>.
/// PL: Poziom językowy, np. <c>"B1"</c>.
/// </param>
/// <param name="Goal">
/// EN: Main learning goal, i.e. why the learner is learning.
/// PL: Główny cel nauki, czyli dlaczego uczeń się uczy.
/// </param>
/// <param name="NativeLanguage">
/// EN: Learner's native language.
/// PL: Język ojczysty ucznia.
/// </param>
/// <param name="TargetLanguage">
/// EN: Language the learner is learning.
/// PL: Język, którego uczy się uczeń.
/// </param>
/// <param name="Preferences">
/// EN: Preferences about working style and pace; never null, may be empty.
/// PL: Preferencje dotyczące stylu pracy i tempa; nigdy null, może być pusta.
/// </param>
/// <param name="Goals">
/// EN: Operational goals of the learner; never null, may be empty.
/// PL: Operacyjne cele ucznia; nigdy null, może być pusta.
/// </param>
/// <param name="WorkContext">
/// EN: Professional context; never null.
/// PL: Kontekst zawodowy; nigdy null.
/// </param>
/// <param name="CommunicationPreferences">
/// EN: Communication preferences; never null.
/// PL: Preferencje komunikacyjne; nigdy null.
/// </param>
/// <param name="PersonalContext">
/// EN: Personal context; never null.
/// PL: Kontekst osobisty; nigdy null.
/// </param>
/// <param name="SemanticSignals">
/// EN: Semantic signals; never null.
/// PL: Sygnały semantyczne; nigdy null.
/// </param>
/// <param name="PerformanceProfile">
/// EN: Communication performance profile; never null.
/// PL: Profil wykonania komunikacyjnego; nigdy null.
/// </param>
/// <param name="ContextNotes">
/// EN: Short semantic summary of the learner; <c>null</c> when not set.
/// PL: Krótkie podsumowanie semantyczne ucznia; <c>null</c>, gdy nieustawione.
/// </param>
public sealed record StudentProfileDto(
    Guid UserId,
    string Name,
    string Level,
    string Goal,
    string NativeLanguage,
    string TargetLanguage,
    IReadOnlyList<string> Preferences,
    IReadOnlyList<LearnerGoal> Goals,
    WorkContext WorkContext,
    CommunicationPreferences CommunicationPreferences,
    PersonalContext PersonalContext,
    SemanticSignals SemanticSignals,
    PerformanceProfile PerformanceProfile,
    string? ContextNotes);
