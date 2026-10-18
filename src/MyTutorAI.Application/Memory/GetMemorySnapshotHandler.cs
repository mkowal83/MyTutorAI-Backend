using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Memory;
using MyTutorAI.Application.Contracts.Profile;
using MyTutorAI.Application.Llm.Mapping;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Memory;

/// <summary>
/// EN: Use case that returns a snapshot of the current user's persistent memory layers, identified through <see cref="ICurrentUser"/> and loaded from the Core, profile, progress and episodic repositories. Read-only; does not read working memory.
/// PL: Przypadek użycia zwracający migawkę trwałych warstw pamięci bieżącego użytkownika, zidentyfikowanego przez <see cref="ICurrentUser"/> i wczytanego z repozytoriów Core, profilu, postępu i epizodów. Tylko odczyt; nie odczytuje pamięci roboczej.
/// </summary>
public sealed class GetMemorySnapshotHandler
{
    private const int RecentEpisodesLimit = 4;

    private readonly ICurrentUser _currentUser;
    private readonly ICoreMemoryRepository _core;
    private readonly IStudentProfileRepository _profiles;
    private readonly IProgressRepository _progress;
    private readonly IEpisodicMemoryRepository _episodic;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="core">
    /// EN: Store from which the Core memory is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywana jest pamięć Core. Nie może być null.
    /// </param>
    /// <param name="profiles">
    /// EN: Store from which the profile is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywany jest profil. Nie może być null.
    /// </param>
    /// <param name="progress">
    /// EN: Store from which the learning progress is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywany jest postęp nauki. Nie może być null.
    /// </param>
    /// <param name="episodic">
    /// EN: Store from which the episodic memory is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywana jest pamięć epizodyczna. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetMemorySnapshotHandler(
        ICurrentUser currentUser,
        ICoreMemoryRepository core,
        IStudentProfileRepository profiles,
        IProgressRepository progress,
        IEpisodicMemoryRepository episodic)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(episodic);

        _currentUser = currentUser;
        _core = core;
        _profiles = profiles;
        _progress = progress;
        _episodic = episodic;
    }

    /// <summary>
    /// EN: Returns the memory snapshot of the current user, including the opening phase the next session would start with; never fails because data is missing.
    /// PL: Zwraca migawkę pamięci bieżącego użytkownika, w tym fazę otwarcia, od której zaczęłaby się następna sesja; nigdy nie kończy się błędem z powodu braku danych.
    /// </summary>
    /// <param name="cancellationToken">
    /// EN: Cancels the database reads.
    /// PL: Anuluje odczyty z bazy danych.
    /// </param>
    /// <returns>
    /// EN: The snapshot built from stored data, or from default empty layers when the user has nothing saved yet.
    /// PL: Migawka zbudowana z zapisanych danych albo z domyślnych, pustych warstw, gdy użytkownik nie ma jeszcze nic zapisanego.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no authenticated user id in the current request.
    /// PL: Bieżące żądanie nie zawiera identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    public async Task<MemorySnapshotResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // 1. Rehydrate the student's four persistent memory layers
        var core = await _core.GetAsync(userId, cancellationToken);
        var profile = await _profiles.GetAsync(userId, cancellationToken);
        var progress = await _progress.GetAsync(userId, cancellationToken);
        var episodic = await _episodic.GetAsync(userId, cancellationToken);

        // 2. Throwaway AgentState — needed only as input to DetermineOpenPhase (reads Progress).
        var state = new AgentState(
            core,
            profile,
            progress,
            new WorkflowMemory(AgentState.DefaultBlankTopic),
            new WorkingMemory(),
            episodic);

        // 3. Opening phase of the next session
        var nextOpenPhase = PhasePolicy.DetermineOpenPhase(state);

        var recentEpisodes = episodic
            .Recent(RecentEpisodesLimit)
            .Select(EpisodeMapper.ToDto)
            .ToArray();

        // 4. Assemble the response — pure projection
        return new MemorySnapshotResponse(
            Core: core,
            Profile: StudentProfileMapper.ToDto(profile),
            RecentMistakes: progress.RecentMistakes,
            HomeworkStatus: progress.HomeworkStatus,
            NextFocus: progress.NextFocus,
            Phase: nextOpenPhase,
            Topic: AgentState.DefaultBlankTopic,
            SemanticAssetsCount: CountSemanticAssets(profile),
            WorkingMemoryCount: 0,
            EpisodeCount: episodic.Count,
            RecentEpisodes: recentEpisodes);
    }

    private static int CountSemanticAssets(StudentProfile profile) =>
        profile.Preferences.Count
        + profile.Goals.Count
        + profile.WorkContext.HighStakesSituations.Count
        + profile.WorkContext.Stakeholders.Count
        + profile.PersonalContext.PersonalFacts.Count
        + profile.PersonalContext.RelationshipHooks.Count
        + profile.PersonalContext.UpcomingEvents.Count
        + profile.PersonalContext.ScheduleConstraints.Count
        + profile.SemanticSignals.ConfidenceBlockers.Count
        + profile.SemanticSignals.MotivationDrivers.Count
        + profile.SemanticSignals.RecurringTopics.Count
        + profile.SemanticSignals.VocabularyTargets.Count
        + profile.PerformanceProfile.TargetScenarios.Count
        + profile.PerformanceProfile.ToneTargets.Count
        + profile.PerformanceProfile.Strengths.Count
        + profile.PerformanceProfile.RecurringWeaknesses.Count
        + profile.PerformanceProfile.SuccessCriteria.Count;
}