namespace MyTutorAI.Application.Contracts.Profile;

/// <summary>
/// EN: Input of the profile update; body of <c>PUT /api/profile</c>. Validated by <see cref="UpdateProfileRequestValidator"/>; replaces all declarative profile fields, while the LLM semantic layers stay unchanged.
/// PL: Dane wejściowe aktualizacji profilu; body <c>PUT /api/profile</c>. Walidowane przez <see cref="UpdateProfileRequestValidator"/>; zastępuje wszystkie deklaratywne pola profilu, a warstwy semantyczne LLM pozostają bez zmian.
/// </summary>
/// <param name="Name">
/// EN: Learner's first name; required, at most 100 characters.
/// PL: Imię ucznia; wymagane, najwyżej 100 znaków.
/// </param>
/// <param name="Level">
/// EN: Language level as free-form text, e.g. <c>"B1"</c>; required, at most 40 characters.
/// PL: Poziom językowy jako dowolny tekst, np. <c>"B1"</c>; wymagany, najwyżej 40 znaków.
/// </param>
/// <param name="Goal">
/// EN: Main learning goal; required, at most 500 characters.
/// PL: Główny cel nauki; wymagany, najwyżej 500 znaków.
/// </param>
/// <param name="NativeLanguage">
/// EN: Learner's native language; required, at most 60 characters.
/// PL: Język ojczysty ucznia; wymagany, najwyżej 60 znaków.
/// </param>
/// <param name="TargetLanguage">
/// EN: Language being learned; required, at most 60 characters.
/// PL: Język nauki; wymagany, najwyżej 60 znaków.
/// </param>
/// <param name="Preferences">
/// EN: Working-style and pace preferences replacing the stored list; <c>null</c> clears the list.
/// PL: Preferencje stylu pracy i tempa zastępujące zapisaną listę; <c>null</c> czyści listę.
/// </param>
/// <param name="Goals">
/// EN: Operational goals replacing the stored ones; <c>null</c> clears them.
/// PL: Cele operacyjne zastępujące zapisane; <c>null</c> je czyści.
/// </param>
/// <param name="WorkContext">
/// EN: Declarative part of the professional context; <c>null</c> clears it, while the LLM-collected high-stakes situations are kept.
/// PL: Deklaratywna część kontekstu zawodowego; <c>null</c> ją czyści, a zebrane przez LLM sytuacje o wysokiej stawce są zachowywane.
/// </param>
/// <param name="CommunicationPreferences">
/// EN: Communication preferences replacing the stored ones as a whole; <c>null</c> clears them.
/// PL: Preferencje komunikacyjne zastępujące w całości zapisane; <c>null</c> je czyści.
/// </param>
public sealed record UpdateProfileRequest(
    string Name,
    string Level,
    string Goal,
    string NativeLanguage,
    string TargetLanguage,
    IReadOnlyList<string>? Preferences,
    IReadOnlyList<ProfileGoalInput>? Goals,
    ProfileWorkContextInput? WorkContext,
    ProfileCommunicationPreferencesInput? CommunicationPreferences);

/// <summary>
/// EN: Declarative professional context within <see cref="UpdateProfileRequest"/>; mapped onto the domain <c>WorkContext</c>.
/// PL: Deklaratywny kontekst zawodowy w ramach <see cref="UpdateProfileRequest"/>; mapowany na domenowy <c>WorkContext</c>.
/// </summary>
/// <param name="Profession">
/// EN: Profession or position; <c>null</c> when unknown; at most 120 characters.
/// PL: Zawód lub stanowisko; <c>null</c>, gdy nieznane; najwyżej 120 znaków.
/// </param>
/// <param name="CompanyName">
/// EN: Employer; <c>null</c> when unknown; at most 120 characters.
/// PL: Pracodawca; <c>null</c>, gdy nieznany; najwyżej 120 znaków.
/// </param>
/// <param name="Industry">
/// EN: Industry; <c>null</c> when unknown; at most 120 characters.
/// PL: Branża; <c>null</c>, gdy nieznana; najwyżej 120 znaków.
/// </param>
/// <param name="Stakeholders">
/// EN: People the learner communicates with in the language; <c>null</c> gives an empty list; each entry non-empty, at most 200 characters.
/// PL: Osoby, z którymi uczeń komunikuje się w tym języku; <c>null</c> daje pustą listę; każdy wpis niepusty, najwyżej 200 znaków.
/// </param>
public sealed record ProfileWorkContextInput(
    string? Profession,
    string? CompanyName,
    string? Industry,
    IReadOnlyList<string>? Stakeholders);

/// <summary>
/// EN: Communication preferences within <see cref="UpdateProfileRequest"/>; mapped onto the domain <c>CommunicationPreferences</c>.
/// PL: Preferencje komunikacyjne w ramach <see cref="UpdateProfileRequest"/>; mapowane na domenowe <c>CommunicationPreferences</c>.
/// </summary>
/// <param name="FeedbackStyle">
/// EN: How the learner wants to receive feedback; <c>null</c> when unknown; at most 120 characters.
/// PL: Jak uczeń chce otrzymywać informację zwrotną; <c>null</c>, gdy nieznane; najwyżej 120 znaków.
/// </param>
/// <param name="CorrectionIntensity">
/// EN: How intensively the learner wants to be corrected; <c>null</c> when unknown; at most 120 characters.
/// PL: Jak intensywnie uczeń chce być poprawiany; <c>null</c>, gdy nieznane; najwyżej 120 znaków.
/// </param>
/// <param name="Pace">
/// EN: Preferred session pace; <c>null</c> when unknown; at most 120 characters.
/// PL: Preferowane tempo sesji; <c>null</c>, gdy nieznane; najwyżej 120 znaków.
/// </param>
/// <param name="PreferredFormats">
/// EN: Preferred working formats; <c>null</c> gives an empty list; each entry non-empty, at most 200 characters.
/// PL: Preferowane formy pracy; <c>null</c> daje pustą listę; każdy wpis niepusty, najwyżej 200 znaków.
/// </param>
public sealed record ProfileCommunicationPreferencesInput(
    string? FeedbackStyle,
    string? CorrectionIntensity,
    string? Pace,
    IReadOnlyList<string>? PreferredFormats);

/// <summary>
/// EN: A single operational goal within <see cref="UpdateProfileRequest"/>; mapped onto the domain <c>LearnerGoal</c>.
/// PL: Pojedynczy cel operacyjny w ramach <see cref="UpdateProfileRequest"/>; mapowany na domenowy <c>LearnerGoal</c>.
/// </summary>
/// <param name="Goal">
/// EN: Specific goal to practice; required, at most 300 characters.
/// PL: Konkretny cel do ćwiczenia; wymagany, najwyżej 300 znaków.
/// </param>
/// <param name="Reason">
/// EN: Why the learner cares about the goal; <c>null</c> when unknown.
/// PL: Dlaczego uczniowi zależy na celu; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="TargetSituation">
/// EN: Real-life situation in which the goal will be used; <c>null</c> when unknown.
/// PL: Rzeczywista sytuacja, w której cel zostanie wykorzystany; <c>null</c>, gdy nieznana.
/// </param>
/// <param name="SuccessMetric">
/// EN: Observable measure of achieving the goal; <c>null</c> when unknown.
/// PL: Obserwowalna miara osiągnięcia celu; <c>null</c>, gdy nieznana.
/// </param>
/// <param name="Deadline">
/// EN: Deadline as free-form text; <c>null</c> when none.
/// PL: Termin jako dowolny tekst; <c>null</c>, gdy brak.
/// </param>
public sealed record ProfileGoalInput(
    string Goal,
    string? Reason,
    string? TargetSituation,
    string? SuccessMetric,
    string? Deadline);
