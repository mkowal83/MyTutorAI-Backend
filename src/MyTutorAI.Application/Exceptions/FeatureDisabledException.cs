namespace MyTutorAI.Application.Exceptions;

/// <summary>
/// EN: Thrown by a handler when an optional feature is disabled by configuration.
/// PL: Zgłaszany przez handler, gdy opcjonalna funkcja jest wyłączona konfiguracją.
/// </summary>
public sealed class FeatureDisabledException : Exception
{

    /// <summary>
    /// EN: Creates the exception with a message naming the disabled feature.
    /// PL: Tworzy wyjątek z komunikatem wskazującym wyłączoną funkcję.
    /// </summary>
    /// <param name="featureName">
    /// EN: Name of the disabled feature, e.g. <c>dialogue</c>.
    /// PL: Nazwa wyłączonej funkcji, np. <c>dialogue</c>.
    /// </param>
    public FeatureDisabledException(string featureName)
        : base($"Feature \"{featureName}\" is disabled in this deployment.")
    {
        FeatureName = featureName;
    }

    /// <summary>
    /// EN: Name of the disabled feature passed to the constructor.
    /// PL: Nazwa wyłączonej funkcji przekazana do konstruktora.
    /// </summary>
    public string FeatureName { get; }
}