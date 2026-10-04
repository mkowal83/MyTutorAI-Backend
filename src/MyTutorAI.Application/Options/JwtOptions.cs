using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Application.Options;

/// <summary>
/// EN: JWT and refresh token settings bound from the <c>Jwt</c> section and validated with DataAnnotations at startup (<c>ValidateOnStart</c>); used both to issue and to validate access tokens.
/// PL: Ustawienia JWT i refresh tokenów wiązane z sekcji <c>Jwt</c> i walidowane DataAnnotations przy starcie (<c>ValidateOnStart</c>); używane zarówno do wydawania, jak i walidacji access tokenów.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// EN: Name of the configuration section.
    /// PL: Nazwa sekcji konfiguracji.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// EN: Token issuer (<c>iss</c> claim); required, no default.
    /// PL: Wystawca tokenu (claim <c>iss</c>); wymagany, bez wartości domyślnej.
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// EN: Token audience (<c>aud</c> claim); required, no default.
    /// PL: Odbiorca tokenu (claim <c>aud</c>); wymagany, bez wartości domyślnej.
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// EN: HMAC-SHA256 signing key, at least 32 characters; a secret supplied only via environment variables or a secret store, never in the repository.
    /// PL: Klucz podpisu HMAC-SHA256, co najmniej 32 znaki; sekret podawany wyłącznie przez zmienne środowiskowe lub magazyn sekretów, nigdy w repozytorium.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// EN: Access token lifetime in minutes, 1–1440; defaults to 15.
    /// PL: Czas życia access tokenu w minutach, 1–1440; domyślnie 15.
    /// </summary>
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>
    /// EN: Refresh token lifetime in days, 1–365; defaults to 14.
    /// PL: Czas życia refresh tokenu w dniach, 1–365; domyślnie 14.
    /// </summary>
    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 14;
}

