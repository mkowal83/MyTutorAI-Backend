namespace MyTutorAI.Domain.Memory.Session;

/// <summary>
/// EN: Conversation phase within a session; values are declared in the order the phases follow each other.
/// PL: Faza rozmowy w ramach sesji; wartości są zadeklarowane w kolejności, w jakiej fazy następują po sobie.
/// </summary>
public enum SessionPhase
{
    /// <summary>
    /// EN: Homework review / check-in. The tutor does not start a new role-play or reveal the topic.
    /// PL: Sprawdzenie zadania domowego / check-in. Tutor nie zaczyna nowego role-play i nie ujawnia tematu.
    /// </summary>
    HomeworkCheck = 0,

    /// <summary>
    /// EN: Appropriate exercise/role-play on the assigned topic. Correction of critical or recurring errors only.
    /// PL: Odpowiednie ćwiczenie/role-play na zadany temat. Poprawiane są tylko błędy krytyczne lub powtarzające się.
    /// </summary>
    Practice = 1,

    /// <summary>
    /// EN: Closing the session: a summary, identifying 1–2 areas for improvement, and a brief homework assignment.
    /// PL: Zamknięcie sesji: podsumowanie, wskazanie 1–2 obszarów do poprawy i krótkie zadanie domowe.
    /// </summary>
    WrapUp = 2,
}

