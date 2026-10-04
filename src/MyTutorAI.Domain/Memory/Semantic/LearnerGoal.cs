namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: A single operational goal of the learner, optionally with its reason, target situation, success metric and deadline.
/// PL: Pojedynczy cel operacyjny ucznia, opcjonalnie z powodem, sytuacją docelową, miarą sukcesu i terminem.
/// </summary>
/// <param name="Goal">
/// EN: Specific goal to practice; must not be null, empty or whitespace, otherwise <see cref="ArgumentException"/>.
/// PL: Konkretny cel do ćwiczenia; nie może być null, pusty ani składać się z samych białych znaków, w przeciwnym razie <see cref="ArgumentException"/>.
/// </param>
/// <param name="Reason">
/// EN: Why the learner cares about the goal; <c>null</c> when unknown.
/// PL: Dlaczego uczniowi zależy na celu; <c>null</c>, gdy nieznane.
/// </param>
/// <param name="TargetSituation">
/// EN: Real-life situation in which the goal will be used; <c>null</c> when unknown.
/// PL: Rzeczywista sytuacja, w której cel zostanie wykorzystany; <c>null</c>, gdy nieznana.
/// </param>
/// <param name="SuccessMetric">
/// EN: Observable measure of achieving the goal; <c>null</c> when unknown.
/// PL: Obserwowalna miara osiągnięcia celu; <c>null</c>, gdy nieznana.
/// </param>
/// <param name="Deadline">
/// EN: Deadline as free-form text; <c>null</c> when none.
/// PL: Termin jako dowolny tekst; <c>null</c>, gdy brak.
/// </param>
public sealed record LearnerGoal(
    string Goal,
    string? Reason = null,
    string? TargetSituation = null,
    string? SuccessMetric = null,
    string? Deadline = null)
{
    /// <summary>
    /// EN: Specific goal to practice; never null, empty or whitespace when set through the constructor.
    /// PL: Konkretny cel do ćwiczenia; nigdy null, pusty ani złożony z samych białych znaków, gdy ustawiony przez konstruktor.
    /// </summary>
    public string Goal { get; init; } = string.IsNullOrWhiteSpace(Goal)
        ? throw new ArgumentException("Goal must not be empty.", nameof(Goal))
        : Goal;
}