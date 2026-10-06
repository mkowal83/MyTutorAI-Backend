using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Semantic;

namespace MyTutorAI.Domain.Memory;

/// <summary>
/// EN: The tutor's memory state for one learner, combining the Core layer (coach identity) with the semantic layer (learner profile and learning progress).
/// PL: Stan pamięci tutora dla jednego ucznia, łączący warstwę Core (tożsamość coacha) z warstwą semantyczną (profil ucznia i postęp nauki).
/// </summary>
public sealed class AgentState
{
    /// <summary>
    /// EN: Learning goal assigned to a learner who has no stored profile yet.
    /// PL: Cel nauki przypisywany uczniowi, który nie ma jeszcze zapisanego profilu.
    /// </summary>
    public const string DefaultBlankGoal = "To practice English";

    /// <summary>
    /// EN: Language level assigned to a learner who has no stored profile yet.
    /// PL: Poziom językowy przypisywany uczniowi, który nie ma jeszcze zapisanego profilu.
    /// </summary>
    public const string DefaultBlankLevel = "B1";

    /// <summary>
    /// EN: Default session topic for an empty state.
    /// PL: Domyślny temat sesji dla pustego stanu.
    /// </summary>
    public const string DefaultBlankTopic = "General conversation in English";

    /// <summary>
    /// EN: Creates the state from already loaded memory layers.
    /// PL: Tworzy stan z już wczytanych warstw pamięci.
    /// </summary>
    /// <param name="core">
    /// EN: Core memory layer; must not be null.
    /// PL: Warstwa pamięci Core; nie może być null.
    /// </param>
    /// <param name="profile">
    /// EN: Learner profile; must not be null.
    /// PL: Profil ucznia; nie może być null.
    /// </param>
    /// <param name="progress">
    /// EN: Learning progress; must not be null.
    /// PL: Postęp nauki; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="core"/>, <paramref name="profile"/> or <paramref name="progress"/> is null.
    /// PL: <paramref name="core"/>, <paramref name="profile"/> lub <paramref name="progress"/> jest null.
    /// </exception>
    public AgentState(CoreMemory core, StudentProfile profile, LearningProgress progress)
    {
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(progress);

        Core = core;
        Profile = profile;
        Progress = progress;
    }

    /// <summary>
    /// EN: Immutable identity and prime directives of the coach (Core layer).
    /// PL: Niezmienna tożsamość i nadrzędne dyrektywy coacha (warstwa Core).
    /// </summary>
    public CoreMemory Core { get; }

    /// <summary>
    /// EN: Learner profile forming the semantic layer: identity, contexts and the semantic summary.
    /// PL: Profil ucznia tworzący warstwę semantyczną: tożsamość, konteksty i podsumowanie semantyczne.
    /// </summary>
    public StudentProfile Profile { get; }

    /// <summary>
    /// EN: Learning progress (semantic layer): recent mistakes, homework status and the focus of the next session.
    /// PL: Postęp nauki (warstwa semantyczna): ostatnie błędy, status zadania domowego i fokus następnej sesji.
    /// </summary>
    public LearningProgress Progress { get; }

    /// <summary>
    /// EN: Creates an empty state for a learner: the default Core memory, a new profile with no acquired context and empty learning progress.
    /// PL: Tworzy pusty stan dla ucznia: domyślną pamięć Core, nowy profil bez nabytego kontekstu i pusty postęp nauki.
    /// </summary>
    /// <param name="userId">
    /// EN: Learner identifier; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator ucznia; nie może być <see cref="Guid.Empty"/>.
    /// </param>
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
    /// <param name="topic">
    /// EN: Session topic; not stored in the created state.
    /// PL: Temat sesji; nie jest zapisywany w tworzonym stanie.
    /// </param>
    /// <returns>
    /// EN: New state with <see cref="CoreMemory.Default"/>, a fresh <see cref="StudentProfile"/> and an empty <see cref="LearningProgress"/>.
    /// PL: Nowy stan z <see cref="CoreMemory.Default"/>, świeżym <see cref="StudentProfile"/> i pustym <see cref="LearningProgress"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="userId"/> is empty, or <paramref name="name"/>, <paramref name="level"/> or <paramref name="goal"/> is null, empty or whitespace.
    /// PL: <paramref name="userId"/> jest pusty albo <paramref name="name"/>, <paramref name="level"/> lub <paramref name="goal"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    public static AgentState CreateBlank(
        Guid userId,
        string name,
        string level,
        string goal,
        string topic) =>
        new(
            CoreMemory.Default,
            new StudentProfile(userId, name, level, goal),
            new LearningProgress());
}
