using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Application.Options;

/// <summary>
/// EN: Redis settings for the Working memory layer, bound from the <c>Redis</c> configuration section.
/// PL: Ustawienia Redis dla warstwy pamięci Working, wiązane z sekcji konfiguracji <c>Redis</c>.
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to this type (<c>Redis</c>).
    /// PL: Nazwa sekcji konfiguracji wiązanej z tym typem (<c>Redis</c>).
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// EN: Redis connection string; required.
    /// PL: Connection string Redis; wymagany.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// EN: Time-to-live of Working memory entries in Redis, in minutes; refreshed on every exchange.
    /// PL: TTL wpisów warstwy Working w Redis, w minutach; odświeżany przy każdej wymianie.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int WorkingMemoryTtlMinutes { get; set; } = 120;
}
