using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Application.Options;

/// <summary>
/// EN: Size limits of the learner memory used by the session use cases, read from the <c>MemoryLimits</c> configuration section.
/// PL: Limity rozmiaru pamięci ucznia używane przez przypadki użycia sesji, odczytywane z sekcji konfiguracji <c>MemoryLimits</c>.
/// </summary>
public sealed class MemoryLimitsOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to this type (<c>MemoryLimits</c>).
    /// PL: Nazwa sekcji konfiguracji wiązanej z tym typem (<c>MemoryLimits</c>).
    /// </summary>
    public const string SectionName = "MemoryLimits";

    /// <summary>
    /// EN: Number of most recent Working memory turns sent to the LLM with each learner message; at least 1.
    /// PL: Liczba najnowszych tur pamięci Working wysyłanych do LLM z każdą wiadomością ucznia; co najmniej 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ContextWindow { get; set; } = 6;

    /// <summary>
    /// EN: Maximum number of episodes stored per user; at least 1.
    /// PL: Maksymalna liczba epizodów przechowywanych na użytkownika; co najmniej 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxEpisodes { get; set; } = 10;
}