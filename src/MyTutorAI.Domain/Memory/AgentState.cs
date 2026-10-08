using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Episodic;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;

namespace MyTutorAI.Domain.Memory;

/// <summary>
/// EN: The tutor's memory state for one learner, combining the Core layer (coach identity), the semantic layer (learner profile and learning progress), the workflow layer (phase and topic of the current session) and the episodic layer (completed sessions).
/// PL: Stan pamięci tutora dla jednego ucznia, łączący warstwę Core (tożsamość coacha), warstwę semantyczną (profil ucznia i postęp nauki), warstwę Workflow (faza i temat bieżącej sesji) oraz warstwę epizodyczną (zakończone sesje).
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
    /// <param name="workflowMemory">
    /// EN: Workflow memory with the current session's phase and topic; must not be null.
    /// PL: Pamięć Workflow z fazą i tematem bieżącej sesji; nie może być null.
    /// </param>
    /// <param name="episodic">
    /// EN: Episodic memory; must not be null.
    /// PL: Pamięć epizodyczna; nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: <paramref name="core"/>, <paramref name="profile"/>, <paramref name="progress"/>, <paramref name="workflowMemory"/> or <paramref name="episodic"/> is null.
    /// PL: <paramref name="core"/>, <paramref name="profile"/>, <paramref name="progress"/>, <paramref name="workflowMemory"/> lub <paramref name="episodic"/> jest null.
    /// </exception>
    public AgentState(CoreMemory core, StudentProfile profile, LearningProgress progress, WorkflowMemory workflowMemory, EpisodicMemory episodic)
    {
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(workflowMemory);
        ArgumentNullException.ThrowIfNull(episodic);

        Core = core;
        Profile = profile;
        Progress = progress;
        Workflow = workflowMemory;
        Episodic = episodic;
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
    /// EN: Workflow memory layer: conversation phase and topic of the current session.
    /// PL: Warstwa pamięci Workflow: faza rozmowy i temat bieżącej sesji.
    /// </summary>
    public WorkflowMemory Workflow { get; }

    /// <summary>
    /// EN: Episodic memory layer: compressed records of completed sessions.
    /// PL: Warstwa pamięci epizodycznej: skompresowane zapisy zakończonych sesji.
    /// </summary>
    public EpisodicMemory Episodic { get; }


    /// <summary>
    /// EN: Creates an empty state for a learner: the default Core memory, a new profile with no acquired context, empty learning progress, workflow memory for the given topic in the <see cref="SessionPhase.Practice"/> phase and empty episodic memory.
    /// PL: Tworzy pusty stan dla ucznia: domyślną pamięć Core, nowy profil bez nabytego kontekstu, pusty postęp nauki, pamięć Workflow dla podanego tematu w fazie <see cref="SessionPhase.Practice"/> i pustą pamięć epizodyczną.
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
    /// EN: Session topic stored trimmed in <see cref="Workflow"/>; must not be null, empty or whitespace.
    /// PL: Temat sesji zapisywany po przycięciu w <see cref="Workflow"/>; nie może być null, pusty ani składać się z samych białych znaków.
    /// </param>
    /// <returns>
    /// EN: New state with <see cref="CoreMemory.Default"/>, a fresh <see cref="StudentProfile"/>, an empty <see cref="LearningProgress"/>, a <see cref="WorkflowMemory"/> in the <see cref="SessionPhase.Practice"/> phase and an empty <see cref="EpisodicMemory"/>.
    /// PL: Nowy stan z <see cref="CoreMemory.Default"/>, świeżym <see cref="StudentProfile"/>, pustym <see cref="LearningProgress"/>, <see cref="WorkflowMemory"/> w fazie <see cref="SessionPhase.Practice"/> i pustą <see cref="EpisodicMemory"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="userId"/> is empty, or <paramref name="name"/>, <paramref name="level"/>, <paramref name="goal"/> or <paramref name="topic"/> is null, empty or whitespace.
    /// PL: <paramref name="userId"/> jest pusty albo <paramref name="name"/>, <paramref name="level"/>, <paramref name="goal"/> lub <paramref name="topic"/> jest null, pusty lub zawiera same białe znaki.
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
            new LearningProgress(),
            new WorkflowMemory(topic, SessionPhase.Practice),
            new EpisodicMemory()
            );
}
