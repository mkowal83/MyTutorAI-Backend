using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Memory;
using MyTutorAI.Application.Llm;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Session;
using MyTutorAI.Domain.Memory.Workflow;
using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Memory;

/// <summary>
/// EN: Use case that renders the <see cref="PromptTemplates.TutorSystem"/> template from the current user's persistent memory layers with empty working memory, as at session start. Read-only: does not call the LLM and persists nothing.
/// PL: Przypadek użycia renderujący szablon <see cref="PromptTemplates.TutorSystem"/> z trwałych warstw pamięci bieżącego użytkownika z pustą pamięcią roboczą, jak na starcie sesji. Tylko odczyt: nie wywołuje LLM i niczego nie zapisuje.
/// </summary>
public sealed class GetPromptPreviewHandler
{
    /// <summary>
    /// EN: Maximum number of prompt characters returned before the truncation marker.
    /// PL: Maksymalna liczba znaków promptu zwracana przed znacznikiem ucięcia.
    /// </summary>
    private const int PreviewCharLimit = 1800;

    /// <summary>
    /// EN: Marker appended to the preview when the rendered prompt exceeds <see cref="PreviewCharLimit"/>.
    /// PL: Znacznik doklejany do podglądu, gdy wyrenderowany prompt przekracza <see cref="PreviewCharLimit"/>.
    /// </summary>
    private const string PreviewTruncationMarker = "\n\n...[preview cut off]...";

    private readonly ICurrentUser _currentUser;
    private readonly ICoreMemoryRepository _core;
    private readonly IStudentProfileRepository _profiles;
    private readonly IProgressRepository _progress;
    private readonly IEpisodicMemoryRepository _episodic;
    private readonly IPromptRenderer _promptRenderer;

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
    /// <param name="promptRenderer">
    /// EN: Renderer of the prompt template. Must not be null.
    /// PL: Renderer szablonu promptu. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetPromptPreviewHandler(
        ICurrentUser currentUser,
        ICoreMemoryRepository core,
        IStudentProfileRepository profiles,
        IProgressRepository progress,
        IEpisodicMemoryRepository episodic,
        IPromptRenderer promptRenderer)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(episodic);
        ArgumentNullException.ThrowIfNull(promptRenderer);

        _currentUser = currentUser;
        _core = core;
        _profiles = profiles;
        _progress = progress;
        _episodic = episodic;
        _promptRenderer = promptRenderer;
    }

    /// <summary>
    /// EN: Renders a preview of the tutor system prompt for the current user and the given topic and phase.
    /// PL: Renderuje podgląd promptu systemowego tutora dla bieżącego użytkownika oraz podanego tematu i fazy.
    /// </summary>
    /// <param name="topic">
    /// EN: Session topic to preview; null, empty or whitespace falls back to <see cref="AgentState.DefaultBlankTopic"/>, otherwise the value is trimmed.
    /// PL: Temat sesji do podglądu; null, pusty lub złożony z białych znaków zastępowany jest <see cref="AgentState.DefaultBlankTopic"/>, w przeciwnym razie wartość jest przycinana.
    /// </param>
    /// <param name="phase">
    /// EN: Phase to preview; null uses the opening phase from <see cref="PhasePolicy.DetermineOpenPhase"/> based on the learner's progress.
    /// PL: Faza do podglądu; null oznacza fazę otwarcia z <see cref="PhasePolicy.DetermineOpenPhase"/> na podstawie postępu ucznia.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the repository reads.
    /// PL: Anuluje odczyty z repozytoriów.
    /// </param>
    /// <returns>
    /// EN: Template name and the rendered prompt, truncated to <see cref="PreviewCharLimit"/> characters plus a marker when longer.
    /// PL: Nazwa szablonu i wyrenderowany prompt, przycięty do <see cref="PreviewCharLimit"/> znaków ze znacznikiem, gdy jest dłuższy.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: The template does not exist or fails to render; a configuration defect mapped to HTTP 500.
    /// PL: Szablon nie istnieje lub nie daje się wyrenderować; defekt konfiguracji mapowany na HTTP 500.
    /// </exception>
    public async Task<PromptPreviewResponse> HandleAsync(
        string? topic,
        SessionPhase? phase,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var effectiveTopic = string.IsNullOrWhiteSpace(topic)
            ? AgentState.DefaultBlankTopic
            : topic.Trim();

        // 1. Rehydrate the student's four persistent memory layers.
        var core = await _core.GetAsync(userId, cancellationToken);
        var profile = await _profiles.GetAsync(userId, cancellationToken);
        var progress = await _progress.GetAsync(userId, cancellationToken);
        var episodic = await _episodic.GetAsync(userId, cancellationToken);

        // 2. Throwaway AgentState: empty Working ("at startup" preview), provisional Workflow
        //    (phase overwritten right away — DetermineOpenPhase reads only Progress).
        var state = new AgentState(
            core,
            profile,
            progress,
            new WorkflowMemory(effectiveTopic),
            new WorkingMemory(),
            episodic);

        // 3. Opening phase of the next session (query > DetermineOpenPhase) + retarget Workflow.
        var effectivePhase = phase ?? PhasePolicy.DetermineOpenPhase(state);
        state.Workflow.Retarget(effectiveTopic, effectivePhase);

        // 4. Render the template + truncate to the limit.
        var rendered = _promptRenderer.Render(PromptTemplates.TutorSystem, state);
        var preview = rendered.Length <= PreviewCharLimit
            ? rendered
            : rendered[..PreviewCharLimit] + PreviewTruncationMarker;

        return new PromptPreviewResponse(PromptTemplates.TutorSystem, preview);
    }
}