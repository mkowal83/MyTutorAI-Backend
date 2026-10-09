using MyTutorAI.Domain.Memory.Working;

namespace MyTutorAI.Application.Abstractions.Repositories;

/// <summary>
/// EN: Port to the Working memory layer: the full turn history of one open session, keyed by (user, session) so one user never sees another user's turns. Implemented in Infrastructure on Redis and registered in DI as a singleton.
/// PL: Port do warstwy pamięci Working: pełna historia tur jednej otwartej sesji, kluczowana po (użytkownik, sesja), więc użytkownik nigdy nie widzi tur innego użytkownika. Implementowany w Infrastructure na Redis i rejestrowany w DI jako singleton.
/// </summary>
public interface IWorkingMemoryStore
{
    /// <summary>
    /// EN: Loads the whole turn history of the session in insertion order.
    /// PL: Wczytuje całą historię tur sesji w kolejności dodania.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session.
    /// PL: Właściciel sesji.
    /// </param>
    /// <param name="sessionId">
    /// EN: Session whose history is read.
    /// PL: Sesja, której historia jest czytana.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call; checked before the storage is queried.
    /// PL: Anuluje wywołanie; sprawdzany przed odpytaniem magazynu.
    /// </param>
    /// <returns>
    /// EN: Working memory with all stored turns, or an empty <see cref="WorkingMemory"/> when nothing has been saved or the entry has expired.
    /// PL: Pamięć Working ze wszystkimi zapisanymi turami albo pusta <see cref="WorkingMemory"/>, gdy nic nie zapisano lub wpis wygasł.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// EN: <paramref name="cancellationToken"/> is cancelled before the storage is queried.
    /// PL: <paramref name="cancellationToken"/> jest anulowany przed odpytaniem magazynu.
    /// </exception>
    Task<WorkingMemory> GetAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Appends a turn to the end of the session history and refreshes the entry's time-to-live; the store-side counterpart of <see cref="WorkingMemory.Append"/>.
    /// PL: Dopisuje turę na koniec historii sesji i odświeża TTL wpisu; odpowiednik po stronie magazynu dla <see cref="WorkingMemory.Append"/>.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session.
    /// PL: Właściciel sesji.
    /// </param>
    /// <param name="sessionId">
    /// EN: Session the turn belongs to.
    /// PL: Sesja, do której należy tura.
    /// </param>
    /// <param name="role">
    /// EN: Author of the turn; must be a defined <see cref="ChatRole"/> value.
    /// PL: Autor tury; musi być zdefiniowaną wartością <see cref="ChatRole"/>.
    /// </param>
    /// <param name="content">
    /// EN: Message content; must not be null, empty or whitespace, and is stored trimmed.
    /// PL: Treść wiadomości; nie może być null, pusta ani składać się z samych białych znaków, zapisywana jest po przycięciu.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call; checked before the storage is written.
    /// PL: Anuluje wywołanie; sprawdzany przed zapisem do magazynu.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="role"/> is not a defined <see cref="ChatRole"/> value.
    /// PL: <paramref name="role"/> nie jest zdefiniowaną wartością <see cref="ChatRole"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// EN: <paramref name="content"/> is null, empty or whitespace.
    /// PL: <paramref name="content"/> jest null, pusty lub zawiera same białe znaki.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// EN: <paramref name="cancellationToken"/> is cancelled before the storage is written.
    /// PL: <paramref name="cancellationToken"/> jest anulowany przed zapisem do magazynu.
    /// </exception>
    Task AppendAsync(Guid userId, Guid sessionId, ChatRole role, string content, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Removes the whole turn history of the session; the store-side counterpart of <see cref="WorkingMemory.Clear"/>. Idempotent: clearing a missing or expired entry succeeds.
    /// PL: Usuwa całą historię tur sesji; odpowiednik po stronie magazynu dla <see cref="WorkingMemory.Clear"/>. Idempotentne: czyszczenie brakującego lub wygasłego wpisu kończy się sukcesem.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session.
    /// PL: Właściciel sesji.
    /// </param>
    /// <param name="sessionId">
    /// EN: Session whose history is removed.
    /// PL: Sesja, której historia jest usuwana.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call; checked before the storage is written.
    /// PL: Anuluje wywołanie; sprawdzany przed zapisem do magazynu.
    /// </param>
    /// <exception cref="OperationCanceledException">
    /// EN: <paramref name="cancellationToken"/> is cancelled before the storage is written.
    /// PL: <paramref name="cancellationToken"/> jest anulowany przed zapisem do magazynu.
    /// </exception>
    Task ClearAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// EN: Reads only the most recent turns of the session without loading the whole history; the store-side counterpart of <see cref="WorkingMemory.GetContextWindow"/>.
    /// PL: Czyta tylko najnowsze tury sesji bez wczytywania całej historii; odpowiednik po stronie magazynu dla <see cref="WorkingMemory.GetContextWindow"/>.
    /// </summary>
    /// <param name="userId">
    /// EN: Owner of the session.
    /// PL: Właściciel sesji.
    /// </param>
    /// <param name="sessionId">
    /// EN: Session whose turns are read.
    /// PL: Sesja, której tury są czytane.
    /// </param>
    /// <param name="limit">
    /// EN: Maximum number of turns to return; must be positive.
    /// PL: Maksymalna liczba zwracanych tur; musi być dodatnia.
    /// </param>
    /// <param name="cancellationToken">
    /// EN: Cancels the call; checked before the storage is queried.
    /// PL: Anuluje wywołanie; sprawdzany przed odpytaniem magazynu.
    /// </param>
    /// <returns>
    /// EN: Up to <paramref name="limit"/> last turns in chronological order; empty when the session has no stored turns.
    /// PL: Co najwyżej <paramref name="limit"/> ostatnich tur w kolejności chronologicznej; pusta lista, gdy sesja nie ma zapisanych tur.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// EN: <paramref name="limit"/> is zero or negative.
    /// PL: <paramref name="limit"/> jest zerem lub liczbą ujemną.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// EN: <paramref name="cancellationToken"/> is cancelled before the storage is queried.
    /// PL: <paramref name="cancellationToken"/> jest anulowany przed odpytaniem magazynu.
    /// </exception>
    Task<IReadOnlyList<ChatTurn>> GetContextWindowAsync(Guid userId, Guid sessionId, int limit, CancellationToken cancellationToken);
}
