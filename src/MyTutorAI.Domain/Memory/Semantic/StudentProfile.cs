namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Semantic memory layer of one learner: declared learning basics plus the professional, personal, communication and performance context built up from conversations.
/// PL: Warstwa pamięci semantycznej jednego ucznia: zadeklarowane podstawy nauki oraz kontekst zawodowy, osobisty, komunikacyjny i wykonania budowany z rozmów.
/// </summary>
public sealed class StudentProfile
{
    private readonly List<string> _preferences = [];
    private readonly List<LearnerGoal> _goals = [];

    /// <summary>
    /// EN: Creates a profile with the required identity and learning basics; all contexts start empty.
    /// PL: Tworzy profil z wymaganą tożsamością i podstawami nauki; wszystkie konteksty startują puste.
    /// </summary>
    /// <param name="userId">
    /// EN: Owning user's identifier; must not be <see cref="Guid.Empty"/>; immutable after creation.
    /// PL: Identyfikator użytkownika-właściciela; nie może być <see cref="Guid.Empty"/>; niezmienny po utworzeniu.
    /// </param>
    /// <param name="name">
    /// EN: Learner's first name; must not be null, empty or whitespace.
    /// PL: Imię ucznia; nie może być null, puste ani składać się z samych białych znaków.
    /// </param>
    /// <param name="level">
    /// EN: Language level, e.g. <c>"B1"</c>; must not be null, empty or whitespace.
    /// PL: Poziom językowy, np. <c>"B1"</c>; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="goal">
    /// EN: Main learning goal, i.e. why the learner is learning; must not be null, empty or whitespace.
    /// PL: Główny cel nauki, czyli dlaczego uczeń się uczy; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="nativeLanguage">
    /// EN: Native language; defaults to <c>"Polish"</c>; must not be empty or whitespace.
    /// PL: Język ojczysty; domyślnie <c>"Polish"</c>; nie może być pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="targetLanguage">
    /// EN: Language being learned; defaults to <c>"English"</c>; must not be empty or whitespace.
    /// PL: Język nauki; domyślnie <c>"English"</c>; nie może być pusty ani składać się z samych białych znaków.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="userId"/> is empty, or any text argument is null, empty or whitespace.
    /// PL: <paramref name="userId"/> jest pusty albo którykolwiek argument tekstowy jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public StudentProfile(
        Guid userId,
        string name,
        string level,
        string goal,
        string nativeLanguage = "Polish",
        string targetLanguage = "English")
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId must not be empty.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(level);
        ArgumentException.ThrowIfNullOrWhiteSpace(goal);
        ArgumentException.ThrowIfNullOrWhiteSpace(nativeLanguage);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLanguage);

        UserId = userId;
        Name = name;
        Level = level;
        Goal = goal;
        NativeLanguage = nativeLanguage;
        TargetLanguage = targetLanguage;
    }

    /// <summary>
    /// EN: Owning user's identifier (the JWT <c>sub</c> / <c>User.Id</c>); immutable after creation.
    /// PL: Identyfikator użytkownika-właściciela (JWT <c>sub</c> / <c>User.Id</c>); niezmienny po utworzeniu.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// EN: Learner's first name.
    /// PL: Imię ucznia.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// EN: Language level, e.g. <c>"B1"</c>.
    /// PL: Poziom językowy, np. <c>"B1"</c>.
    /// </summary>
    public string Level { get; private set; }

    /// <summary>
    /// EN: Main learning goal, i.e. why the learner is learning.
    /// PL: Główny cel nauki, czyli dlaczego uczeń się uczy.
    /// </summary>
    public string Goal { get; private set; }

    /// <summary>
    /// EN: Learner's native language.
    /// PL: Język ojczysty ucznia.
    /// </summary>
    public string NativeLanguage { get; private set; }

    /// <summary>
    /// EN: Language the learner is learning.
    /// PL: Język, którego uczy się uczeń.
    /// </summary>
    public string TargetLanguage { get; private set; }

    /// <summary>
    /// EN: Learner's preferences about working style and pace; replaced via <see cref="SetPreferences"/>.
    /// PL: Preferencje ucznia dotyczące stylu pracy i tempa; zastępowane przez <see cref="SetPreferences"/>.
    /// </summary>
    public IReadOnlyList<string> Preferences => _preferences;

    /// <summary>
    /// EN: Learner's operational goals; replaced via <see cref="SetGoals"/>.
    /// PL: Operacyjne cele ucznia; zastępowane przez <see cref="SetGoals"/>.
    /// </summary>
    public IReadOnlyList<LearnerGoal> Goals => _goals;

    /// <summary>
    /// EN: Professional context; never null, replaced via <see cref="SetWorkContext"/>.
    /// PL: Kontekst zawodowy; nigdy null, zastępowany przez <see cref="SetWorkContext"/>.
    /// </summary>
    public WorkContext WorkContext { get; private set; } = new();

    /// <summary>
    /// EN: Communication preferences; never null, replaced via <see cref="SetCommunicationPreferences"/>.
    /// PL: Preferencje komunikacyjne; nigdy null, zastępowane przez <see cref="SetCommunicationPreferences"/>.
    /// </summary>
    public CommunicationPreferences CommunicationPreferences { get; private set; } = new();

    /// <summary>
    /// EN: Personal context; never null, replaced via <see cref="SetPersonalContext"/>.
    /// PL: Kontekst osobisty; nigdy null, zastępowany przez <see cref="SetPersonalContext"/>.
    /// </summary>
    public PersonalContext PersonalContext { get; private set; } = new();

    /// <summary>
    /// EN: Semantic signals; never null, replaced via <see cref="SetSemanticSignals"/>.
    /// PL: Sygnały semantyczne; nigdy null, zastępowane przez <see cref="SetSemanticSignals"/>.
    /// </summary>
    public SemanticSignals SemanticSignals { get; private set; } = new();

    /// <summary>
    /// EN: Communication performance profile; never null, replaced via <see cref="SetPerformanceProfile"/>.
    /// PL: Profil wykonania komunikacyjnego; nigdy null, zastępowany przez <see cref="SetPerformanceProfile"/>.
    /// </summary>
    public PerformanceProfile PerformanceProfile { get; private set; } = new();

    /// <summary>
    /// EN: Short semantic summary of the learner; <c>null</c> when not set; set via <see cref="SetContextNotes"/>.
    /// PL: Krótkie podsumowanie semantyczne ucznia; <c>null</c>, gdy nieustawione; ustawiane przez <see cref="SetContextNotes"/>.
    /// </summary>
    public string? ContextNotes { get; private set; }

    /// <summary>
    /// EN: Replaces the learner's identity and learning basics; values are stored as given.
    /// PL: Zastępuje tożsamość i podstawy nauki ucznia; wartości są zapisywane tak, jak podano.
    /// </summary>
    /// <param name="name">
    /// EN: Learner's first name; must not be null, empty or whitespace.
    /// PL: Imię ucznia; nie może być null, puste ani składać się z samych białych znaków.
    /// </param>
    /// <param name="level">
    /// EN: Language level; must not be null, empty or whitespace.
    /// PL: Poziom językowy; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="goal">
    /// EN: Main learning goal; must not be null, empty or whitespace.
    /// PL: Główny cel nauki; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="nativeLanguage">
    /// EN: Native language; must not be null, empty or whitespace.
    /// PL: Język ojczysty; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <param name="targetLanguage">
    /// EN: Language being learned; must not be null, empty or whitespace.
    /// PL: Język nauki; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: Any argument is null, empty or whitespace; the profile is then left unchanged.
    /// PL: Którykolwiek argument jest null, pusty lub zawiera same białe znaki; profil pozostaje wtedy bez zmian.
    /// </exception>
    public void UpdateLearningBasics(
        string name,
        string level,
        string goal,
        string nativeLanguage,
        string targetLanguage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(level);
        ArgumentException.ThrowIfNullOrWhiteSpace(goal);
        ArgumentException.ThrowIfNullOrWhiteSpace(nativeLanguage);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLanguage);

        Name = name;
        Level = level;
        Goal = goal;
        NativeLanguage = nativeLanguage;
        TargetLanguage = targetLanguage;
    }

    /// <summary>
    /// EN: Replaces the preference list with the given entries, trimmed, with blank ones dropped and order preserved; duplicates are not removed.
    /// PL: Zastępuje listę preferencji podanymi wpisami, przyciętymi, bez pustych i z zachowaniem kolejności; duplikaty nie są usuwane.
    /// </summary>
    /// <param name="preferences">
    /// EN: New preferences; must not be null.
    /// PL: Nowe preferencje; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="preferences"/> is null.
    /// PL: <paramref name="preferences"/> jest null.
    /// </exception>
    public void SetPreferences(IEnumerable<string> preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        _preferences.Clear();
        _preferences.AddRange(NormalizeText(preferences));
    }

    /// <summary>
    /// EN: Replaces the operational goals with a copy of the given ones, dropping <c>null</c> entries.
    /// PL: Zastępuje cele operacyjne kopią podanych, pomijając wpisy <c>null</c>.
    /// </summary>
    /// <param name="goals">
    /// EN: New goals; must not be null.
    /// PL: Nowe cele; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="goals"/> is null.
    /// PL: <paramref name="goals"/> jest null.
    /// </exception>
    public void SetGoals(IEnumerable<LearnerGoal> goals)
    {
        ArgumentNullException.ThrowIfNull(goals);
        _goals.Clear();
        _goals.AddRange(goals.Where(g => g is not null));
    }

    /// <summary>
    /// EN: Replaces the professional context as a whole.
    /// PL: Zastępuje w całości kontekst zawodowy.
    /// </summary>
    /// <param name="workContext">
    /// EN: New professional context; must not be null.
    /// PL: Nowy kontekst zawodowy; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="workContext"/> is null.
    /// PL: <paramref name="workContext"/> jest null.
    /// </exception>
    public void SetWorkContext(WorkContext workContext)
    {
        ArgumentNullException.ThrowIfNull(workContext);
        WorkContext = workContext;
    }

    /// <summary>
    /// EN: Replaces the communication preferences as a whole.
    /// PL: Zastępuje w całości preferencje komunikacyjne.
    /// </summary>
    /// <param name="communicationPreferences">
    /// EN: New communication preferences; must not be null.
    /// PL: Nowe preferencje komunikacyjne; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="communicationPreferences"/> is null.
    /// PL: <paramref name="communicationPreferences"/> jest null.
    /// </exception>
    public void SetCommunicationPreferences(CommunicationPreferences communicationPreferences)
    {
        ArgumentNullException.ThrowIfNull(communicationPreferences);
        CommunicationPreferences = communicationPreferences;
    }

    /// <summary>
    /// EN: Replaces the personal context as a whole.
    /// PL: Zastępuje w całości kontekst osobisty.
    /// </summary>
    /// <param name="personalContext">
    /// EN: New personal context; must not be null.
    /// PL: Nowy kontekst osobisty; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="personalContext"/> is null.
    /// PL: <paramref name="personalContext"/> jest null.
    /// </exception>
    public void SetPersonalContext(PersonalContext personalContext)
    {
        ArgumentNullException.ThrowIfNull(personalContext);
        PersonalContext = personalContext;
    }

    /// <summary>
    /// EN: Replaces the semantic signals as a whole.
    /// PL: Zastępuje w całości sygnały semantyczne.
    /// </summary>
    /// <param name="semanticSignals">
    /// EN: New semantic signals; must not be null.
    /// PL: Nowe sygnały semantyczne; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="semanticSignals"/> is null.
    /// PL: <paramref name="semanticSignals"/> jest null.
    /// </exception>
    public void SetSemanticSignals(SemanticSignals semanticSignals)
    {
        ArgumentNullException.ThrowIfNull(semanticSignals);
        SemanticSignals = semanticSignals;
    }

    /// <summary>
    /// EN: Replaces the performance profile as a whole.
    /// PL: Zastępuje w całości profil wykonania.
    /// </summary>
    /// <param name="performanceProfile">
    /// EN: New performance profile; must not be null.
    /// PL: Nowy profil wykonania; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="performanceProfile"/> is null.
    /// PL: <paramref name="performanceProfile"/> jest null.
    /// </exception>
    public void SetPerformanceProfile(PerformanceProfile performanceProfile)
    {
        ArgumentNullException.ThrowIfNull(performanceProfile);
        PerformanceProfile = performanceProfile;
    }

    /// <summary>
    /// EN: Sets the semantic summary, stored trimmed.
    /// PL: Ustawia podsumowanie semantyczne, zapisywane po przycięciu.
    /// </summary>
    /// <param name="contextNotes">
    /// EN: New summary; <c>null</c>, empty or whitespace clears it to <c>null</c>.
    /// PL: Nowe podsumowanie; <c>null</c>, pusty tekst lub same białe znaki czyszczą je do <c>null</c>.
    /// </param>
    public void SetContextNotes(string? contextNotes) =>
        ContextNotes = string.IsNullOrWhiteSpace(contextNotes) ? null : contextNotes.Trim();

    /// <summary>
    /// EN: Clears everything learned from conversations (preferences, goals, all contexts and the summary) and sets a new main goal; name, level and languages are kept.
    /// PL: Czyści wszystko, czego dowiedziano się z rozmów (preferencje, cele, wszystkie konteksty i podsumowanie), i ustawia nowy główny cel; imię, poziom i języki pozostają.
    /// </summary>
    /// <param name="goal">
    /// EN: New main learning goal; must not be null, empty or whitespace.
    /// PL: Nowy główny cel nauki; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="goal"/> is null, empty or whitespace; the profile is then left unchanged.
    /// PL: <paramref name="goal"/> jest null, pusty lub zawiera same białe znaki; profil pozostaje wtedy bez zmian.
    /// </exception>
    public void ClearAcquiredContext(string goal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(goal);

        Goal = goal;
        _preferences.Clear();
        _goals.Clear();
        WorkContext = new WorkContext();
        CommunicationPreferences = new CommunicationPreferences();
        PersonalContext = new PersonalContext();
        SemanticSignals = new SemanticSignals();
        PerformanceProfile = new PerformanceProfile();
        ContextNotes = null;
    }

    private static IEnumerable<string> NormalizeText(IEnumerable<string> items) =>
        items.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim());
}
