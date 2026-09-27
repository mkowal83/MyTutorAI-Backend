namespace MyTutorAI.Application.Options;


/// <summary>
/// EN: Configuration switches of the optional demo features. A disabled feature's endpoint is not mapped, so it behaves as non-existent (routing 404).
/// PL: Przełączniki konfiguracyjne opcjonalnych funkcji demo. Endpoint wyłączonej funkcji nie jest mapowany, więc zachowuje się jak nieistniejący (404 routingu).
/// </summary>
public sealed class FeaturesOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to these options.
    /// PL: Nazwa sekcji konfiguracji wiązanej z tymi opcjami.
    /// </summary>
    public const string SectionName = "Features";

    /// <summary>
    /// EN: Enables the demo example-dialogue generator; defaults to false.
    /// PL: Włącza demonstracyjny generator przykładowego dialogu; domyślnie false.
    /// </summary>
    public bool AllowDialogue { get; set; }
}
