using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Api.RateLimiting;

/// <summary>
/// EN: Rate limiting settings bound from the <c>RateLimit</c> configuration section. Only the authentication policy is used so far.
/// PL: Ustawienia rate limitingu wiązane z sekcji konfiguracji <c>RateLimit</c>. Na razie używana jest tylko polityka uwierzytelniania.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to this type (<c>RateLimit</c>).
    /// PL: Nazwa sekcji konfiguracji wiązanej z tym typem (<c>RateLimit</c>).
    /// </summary>
    public const string SectionName = "RateLimit";

    /// <summary>
    /// EN: Number of authentication requests allowed per client IP within one window; default 10.
    /// PL: Liczba żądań uwierzytelniania dozwolonych na adres IP klienta w jednym oknie; domyślnie 10.
    /// </summary>
    [Range(1, 10_000)]
    public int AuthPermitLimit { get; set; } = 10;

    /// <summary>
    /// EN: Length of the authentication window in seconds; default 60.
    /// PL: Długość okna uwierzytelniania w sekundach; domyślnie 60.
    /// </summary>
    [Range(1, 86_400)]
    public int AuthWindowSeconds { get; set; } = 60;

    /// <summary>
    /// EN: Number of authentication requests queued once the limit is reached; default 0 (reject immediately).
    /// PL: Liczba żądań uwierzytelniania kolejkowanych po osiągnięciu limitu; domyślnie 0 (natychmiastowe odrzucenie).
    /// </summary>
    [Range(0, 1_000)]
    public int AuthQueueLimit { get; set; }
}
