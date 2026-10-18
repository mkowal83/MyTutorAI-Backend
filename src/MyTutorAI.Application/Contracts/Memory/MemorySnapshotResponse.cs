using MyTutorAI.Application.Contracts.Profile.Dtos;
using MyTutorAI.Application.Contracts.Sessions.Dtos;
using MyTutorAI.Domain.Memory.Core;
using MyTutorAI.Domain.Memory.Semantic;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Application.Contracts.Memory;

/// <summary>
/// EN: Response DTO of <c>GET /api/memory</c>: a read-only snapshot of the logged-in learner's memory layers (core, semantic profile, progress, workflow, episodes).
/// PL: DTO odpowiedzi <c>GET /api/memory</c>: migawka warstw pamięci zalogowanego ucznia (core, profil semantyczny, postęp, workflow, epizody) tylko do odczytu.
/// </summary>
/// <param name="Core">
/// EN: Coach identity and prime directives; <see cref="CoreMemory.Default"/> when nothing has been saved for the user.
/// PL: Tożsamość coacha i nadrzędne zasady; <see cref="CoreMemory.Default"/>, gdy dla użytkownika nic nie zapisano.
/// </param>
/// <param name="Profile">
/// EN: Learner profile with declarative fields and semantic layers.
/// PL: Profil ucznia z polami deklaratywnymi i warstwami semantycznymi.
/// </param>
/// <param name="RecentMistakes">
/// EN: Recent mistakes from the learner's progress; empty when there are none.
/// PL: Ostatnie błędy z postępu ucznia; pusta lista, gdy ich nie ma.
/// </param>
/// <param name="HomeworkStatus">
/// EN: Current homework status from the learner's progress.
/// PL: Bieżący status zadania domowego z postępu ucznia.
/// </param>
/// <param name="NextFocus">
/// EN: Focus planned for the next session; <c>null</c> when not set.
/// PL: Fokus zaplanowany na następną sesję; <c>null</c>, gdy nie jest ustawiony.
/// </param>
/// <param name="Phase">
/// EN: Opening phase of the next session: <see cref="SessionPhase.HomeworkCheck"/> or <see cref="SessionPhase.Practice"/>, never <see cref="SessionPhase.WrapUp"/>.
/// PL: Faza otwarcia następnej sesji: <see cref="SessionPhase.HomeworkCheck"/> lub <see cref="SessionPhase.Practice"/>, nigdy <see cref="SessionPhase.WrapUp"/>.
/// </param>
/// <param name="Topic">
/// EN: Always the default topic <c>AgentState.DefaultBlankTopic</c>; the snapshot does not reflect an open session.
/// PL: Zawsze domyślny temat <c>AgentState.DefaultBlankTopic</c>; migawka nie odzwierciedla otwartej sesji.
/// </param>
/// <param name="SemanticAssetsCount">
/// EN: Total number of items in the profile's lists (preferences, goals, work and personal context, semantic signals, performance profile).
/// PL: Łączna liczba elementów list profilu (preferencje, cele, kontekst zawodowy i osobisty, sygnały semantyczne, profil wykonania).
/// </param>
/// <param name="WorkingMemoryCount">
/// EN: Always <c>0</c>; the snapshot does not read working memory.
/// PL: Zawsze <c>0</c>; migawka nie odczytuje pamięci roboczej.
/// </param>
/// <param name="EpisodeCount">
/// EN: Number of stored episodes of closed sessions.
/// PL: Liczba przechowywanych epizodów zamkniętych sesji.
/// </param>
/// <param name="RecentEpisodes">
/// EN: Up to 4 newest episodes, ordered from oldest to newest; empty when there are none.
/// PL: Do 4 najnowszych epizodów, uporządkowanych od najstarszego do najnowszego; pusta lista, gdy ich nie ma.
/// </param>
public sealed record MemorySnapshotResponse(
    CoreMemory Core,
    StudentProfileDto Profile,
    IReadOnlyList<string> RecentMistakes,
    HomeworkStatus HomeworkStatus,
    string? NextFocus,
    SessionPhase Phase,
    string Topic,
    int SemanticAssetsCount,
    int WorkingMemoryCount,
    int EpisodeCount,
    IReadOnlyList<EpisodeDto> RecentEpisodes);
