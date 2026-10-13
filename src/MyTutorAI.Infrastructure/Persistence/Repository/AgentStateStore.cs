using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Exceptions;
using MyTutorAI.Domain.Memory;
using MyTutorAI.Domain.Memory.Workflow;

namespace MyTutorAI.Infrastructure.Persistence.Repository;

/// <summary>
/// EN: Implementation of <see cref="IAgentStateStore"/> that composes the layer repositories (SQL Server) and <see cref="IWorkingMemoryStore"/> (Redis); registered as scoped. Each load and each save holds the user's lock from <see cref="AgentStateLockProvider"/> only for its own duration.
/// PL: Implementacja <see cref="IAgentStateStore"/> składająca repozytoria warstw (SQL Server) i <see cref="IWorkingMemoryStore"/> (Redis); rejestrowana jako scoped. Każdy odczyt i każdy zapis trzyma blokadę użytkownika z <see cref="AgentStateLockProvider"/> tylko na czas własnego wykonania.
/// </summary>
/// <remarks>
/// EN: The four layers are saved one after another, each with its own database commit, so a failure in the middle leaves the earlier layers saved.
/// PL: Cztery warstwy są zapisywane jedna po drugiej, każda z własnym zatwierdzeniem w bazie, więc błąd w trakcie zostawia wcześniejsze warstwy zapisane.
/// </remarks>
/// <param name="locks">
/// EN: Per-user lock provider.
/// PL: Dostawca blokad per użytkownik.
/// </param>
/// <param name="core">
/// EN: Core layer repository.
/// PL: Repozytorium warstwy Core.
/// </param>
/// <param name="profiles">
/// EN: Student profile repository.
/// PL: Repozytorium profilu ucznia.
/// </param>
/// <param name="progress">
/// EN: Learning progress repository.
/// PL: Repozytorium postępu nauki.
/// </param>
/// <param name="episodic">
/// EN: Episodic memory repository.
/// PL: Repozytorium pamięci epizodycznej.
/// </param>
/// <param name="sessions">
/// EN: Session repository; source of the Workflow topic and phase.
/// PL: Repozytorium sesji; źródło tematu i fazy Workflow.
/// </param>
/// <param name="workingMemory">
/// EN: Redis store of the session's turns.
/// PL: Magazyn Redis tur sesji.
/// </param>
public sealed class AgentStateStore(
    AgentStateLockProvider locks,
    ICoreMemoryRepository core,
    IStudentProfileRepository profiles,
    IProgressRepository progress,
    IEpisodicMemoryRepository episodic,
    ISessionRepository sessions,
    IWorkingMemoryStore workingMemory) : IAgentStateStore
{
    /// <inheritdoc />
    public async Task<AgentState> LoadAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var gate = locks.GetLock(userId);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var session = await sessions.GetAsync(userId, sessionId, cancellationToken)
                ?? throw new SessionNotFoundException(userId, sessionId);

            return new AgentState(
                await core.GetAsync(userId, cancellationToken),
                await profiles.GetAsync(userId, cancellationToken),
                await progress.GetAsync(userId, cancellationToken),
                new WorkflowMemory(session.Topic, session.Phase),
                await workingMemory.GetAsync(userId, sessionId, cancellationToken),
                await episodic.GetAsync(userId, cancellationToken));
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(AgentState state, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        cancellationToken.ThrowIfCancellationRequested();

        var gate = locks.GetLock(state.UserId);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await core.SaveAsync(state.UserId, state.Core, cancellationToken);
            await profiles.SaveAsync(state.Profile, cancellationToken);
            await progress.SaveAsync(state.UserId, state.Progress, cancellationToken);
            await episodic.SaveAsync(state.UserId, state.Episodic, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }
}