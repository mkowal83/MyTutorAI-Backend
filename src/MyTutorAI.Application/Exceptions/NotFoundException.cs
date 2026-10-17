namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown by Application handlers when a requested resource does not exist. Mapped by the Api global exception handler to HTTP 404, with <see cref="Exception.Message"/> returned as the problem <c>detail</c>.
/// PL: Zgłaszany przez handlery Application, gdy żądany zasób nie istnieje. Mapowany przez globalny handler wyjątków w Api na HTTP 404, a <see cref="Exception.Message"/> trafia do <c>detail</c> problemu.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// EN: Creates the exception with a message naming the resource and its key.
    /// PL: Tworzy wyjątek z komunikatem wskazującym zasób i jego klucz.
    /// </summary>
    /// <param name="resourceName">
    /// EN: Name of the missing resource type, e.g. <c>User</c>; included in the client-visible message.
    /// PL: Nazwa typu brakującego zasobu, np. <c>User</c>; trafia do komunikatu widocznego dla klienta.
    /// </param>
    /// <param name="key">
    /// EN: Key used in the lookup; included in the client-visible message.
    /// PL: Klucz użyty przy wyszukiwaniu; trafia do komunikatu widocznego dla klienta.
    /// </param>
    public NotFoundException(string resourceName, object key)
        : base($"Resource \"{resourceName}\" with key \"{key}\" was not found.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    /// <summary>
    /// EN: Name of the missing resource type.
    /// PL: Nazwa typu brakującego zasobu.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// EN: Key used in the lookup.
    /// PL: Klucz użyty przy wyszukiwaniu.
    /// </summary>
    public object Key { get; }
}
