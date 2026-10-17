namespace MyTutorAI.Application.Abstractions;

/// <summary>
/// EN: Application port that recognises storage-specific exceptions (SQL Server, Redis) without exposing their types to the caller; implemented in Infrastructure and registered as a singleton.
/// PL: Port Application rozpoznający wyjątki specyficzne dla magazynów (SQL Server, Redis) bez ujawniania ich typów wywołującemu; implementowany w Infrastructure i rejestrowany jako singleton.
/// </summary>
public interface IStorageExceptionClassifier
{
    /// <summary>
    /// EN: Classifies the exception as a storage failure. Never throws.
    /// PL: Klasyfikuje wyjątek jako awarię magazynu. Nigdy nie zgłasza wyjątku.
    /// </summary>
    /// <param name="exception">
    /// EN: Exception to classify.
    /// PL: Wyjątek do sklasyfikowania.
    /// </param>
    /// <returns>
    /// EN: Kind of the storage failure, or <see cref="StorageFailureKind.None"/> when the exception does not come from storage.
    /// PL: Rodzaj awarii magazynu albo <see cref="StorageFailureKind.None"/>, gdy wyjątek nie pochodzi z magazynu.
    /// </returns>
    StorageFailureKind Classify(Exception exception);
}

/// <summary>
/// EN: Kind of storage failure recognised by <see cref="IStorageExceptionClassifier"/>.
/// PL: Rodzaj awarii magazynu rozpoznawany przez <see cref="IStorageExceptionClassifier"/>.
/// </summary>
public enum StorageFailureKind
{
    /// <summary>
    /// EN: Not a storage failure.
    /// PL: To nie jest awaria magazynu.
    /// </summary>
    None,

    /// <summary>
    /// EN: Concurrent modification of the same data (optimistic concurrency conflict).
    /// PL: Równoczesna modyfikacja tych samych danych (konflikt optymistycznej współbieżności).
    /// </summary>
    Conflict,

    /// <summary>
    /// EN: Storage is unreachable or timed out; the request may be retried later.
    /// PL: Magazyn jest niedostępny lub przekroczono limit czasu; żądanie można ponowić później.
    /// </summary>
    Unavailable,
}
