using FluentValidation;
using MyTutorAI.Application.Contracts;

namespace MyTutorAI.Application.Demo;


/// <summary>
/// EN: Input validation rules for <see cref="DialogueGenerationRequest"/>; registered in DI as a singleton and run by the Api validation filter before the handler.
/// PL: Reguły walidacji wejścia dla <see cref="DialogueGenerationRequest"/>; rejestrowane w DI jako singleton i uruchamiane przez filtr walidacji w Api przed handlerem.
/// </summary>
public sealed class DialogueGenerationRequestValidator : AbstractValidator<DialogueGenerationRequest>
{
    /// <summary>
    /// EN: Maximum allowed length of <see cref="DialogueGenerationRequest.Topic"/>, in characters.
    /// PL: Maksymalna dozwolona długość <see cref="DialogueGenerationRequest.Topic"/> w znakach.
    /// </summary>
    public const int TopicMaxLength = 200;

    /// <summary>
    /// EN: Defines the rules: <see cref="DialogueGenerationRequest.Topic"/> must not be empty or whitespace and must not exceed <see cref="TopicMaxLength"/> characters.
    /// PL: Definiuje reguły: <see cref="DialogueGenerationRequest.Topic"/> nie może być pusty ani złożony z samych białych znaków i nie może przekraczać <see cref="TopicMaxLength"/> znaków.
    /// </summary>
    public DialogueGenerationRequestValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty()
            .MaximumLength(TopicMaxLength);
    }
}
