using MyTutorAI.Application.Abstractions;
using MyTutorAI.Application.Abstractions.Repositories;
using MyTutorAI.Application.Contracts.Memory;
using MyTutorAI.Application.Llm.Mapping;

namespace MyTutorAI.Application.Memory;

/// <summary>
/// EN: Use case that returns all retained episodes of the current user, identified through <see cref="ICurrentUser"/> and loaded from the episodic repository. Read-only.
/// PL: Przypadek użycia zwracający wszystkie przechowywane epizody bieżącego użytkownika, zidentyfikowanego przez <see cref="ICurrentUser"/> i wczytanego z repozytorium epizodów. Tylko odczyt.
/// </summary>
public sealed class GetEpisodesHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IEpisodicMemoryRepository _episodic;

    /// <summary>
    /// EN: Creates the handler; registered in DI as scoped.
    /// PL: Tworzy handler; rejestrowany w DI jako scoped.
    /// </summary>
    /// <param name="currentUser">
    /// EN: Identity of the user making the request. Must not be null.
    /// PL: Tożsamość użytkownika wykonującego żądanie. Nie może być null.
    /// </param>
    /// <param name="episodic">
    /// EN: Store from which the episodic memory is loaded. Must not be null.
    /// PL: Magazyn, z którego wczytywana jest pamięć epizodyczna. Nie może być null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// EN: Any argument is null.
    /// PL: Którykolwiek argument jest null.
    /// </exception>
    public GetEpisodesHandler(ICurrentUser currentUser, IEpisodicMemoryRepository episodic)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(episodic);

        _currentUser = currentUser;
        _episodic = episodic;
    }

    /// <summary>
    /// EN: Returns the episodes of the current user's closed sessions, newest first; never fails because data is missing.
    /// PL: Zwraca epizody zamkniętych sesji bieżącego użytkownika, od najnowszego; nigdy nie kończy się błędem z powodu braku danych.
    /// </summary>
    /// <param name="cancellationToken">
    /// EN: Cancels the database read.
    /// PL: Anuluje odczyt z bazy danych.
    /// </param>
    /// <returns>
    /// EN: All retained episodes ordered from newest to oldest with their count; an empty list and <c>0</c> when nothing has been saved.
    /// PL: Wszystkie przechowywane epizody uporządkowane od najnowszego do najstarszego wraz z ich liczbą; pusta lista i <c>0</c>, gdy nic nie zapisano.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// EN: There is no authenticated user id in the current request.
    /// PL: Bieżące żądanie nie zawiera identyfikatora uwierzytelnionego użytkownika.
    /// </exception>
    public async Task<EpisodesResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var episodic = await _episodic.GetAsync(_currentUser.UserId, cancellationToken);

        var episodes = episodic.Episodes
            .Reverse()
            .Select(EpisodeMapper.ToDto)
            .ToArray();

        return new EpisodesResponse(episodes, episodes.Length);
    }
}