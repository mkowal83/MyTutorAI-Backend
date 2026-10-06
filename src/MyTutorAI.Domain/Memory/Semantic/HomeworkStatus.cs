namespace MyTutorAI.Domain.Memory.Semantic;

/// <summary>
/// EN: Status of the learner's homework, carried over between sessions as part of the learning progress.
/// PL: Status zadania domowego ucznia, przenoszony między sesjami jako część postępu nauki.
/// </summary>
public enum HomeworkStatus
{
    /// <summary>
    /// EN: No homework assigned; initial state.
    /// PL: Brak zadanego zadania; stan początkowy.
    /// </summary>
    None = 0,

    /// <summary>
    /// EN: Homework assigned and awaiting review.
    /// PL: Zadanie zadane i oczekuje na sprawdzenie.
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// EN: Homework reviewed and passed.
    /// PL: Zadanie sprawdzone i zaliczone.
    /// </summary>
    Done = 2,
}
