namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: The learner's communication preferences, which control how the tutor gives feedback and runs the session.
/// PL: Preferencje komunikacyjne ucznia, które sterują tym, jak tutor udziela informacji zwrotnej i prowadzi sesję.
/// </summary>
/// <param name="FeedbackStyle">
/// EN: How the learner wants to receive feedback; <c>null</c> when unknown.
/// PL: Jak uczeń chce otrzymywać informację zwrotną; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="CorrectionIntensity">
/// EN: How intensively the learner wants to be corrected; <c>null</c> when unknown.
/// PL: Jak intensywnie uczeń chce być poprawiany; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="Pace">
/// EN: Preferred session pace; <c>null</c> when unknown.
/// PL: Preferowane tempo sesji; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="PreferredFormats">
/// EN: Preferred working formats; copied, <c>null</c> gives an empty list.
/// PL: Preferowane formy pracy; kopiowane, <c>null</c> daje pustą listę.
/// </param>
public sealed record CommunicationPreferences(
    string? FeedbackStyle = null,
    string? CorrectionIntensity = null,
    string? Pace = null,
    IReadOnlyList<string>? PreferredFormats = null)
{
    /// <summary>
    /// EN: Preferred working formats, e.g. roleplay, mock interview, short openers; never null.
    /// PL: Preferowane formy pracy, np. roleplay, próbna rozmowa kwalifikacyjna, krótkie otwarcia; nigdy null.
    /// </summary>
    public IReadOnlyList<string> PreferredFormats { get; init; } =
        PreferredFormats is null ? [] : [.. PreferredFormats];
}