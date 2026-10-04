using FluentValidation;

namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Input validation rules for <see cref="StartSessionRequest"/>; registered in DI as scoped (assembly scan).
/// PL: Reguły walidacji wejścia dla <see cref="StartSessionRequest"/>; rejestrowane w DI jako scoped (skan assembly).
/// </summary>
public sealed class StartSessionRequestValidator : AbstractValidator<StartSessionRequest>
{
    /// <summary>
    /// EN: Defines the rules: topic required, at most 200 characters.
    /// PL: Definiuje reguły: temat wymagany, najwyżej 200 znaków.
    /// </summary>
    public StartSessionRequestValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("The subject is required.")
            .MaximumLength(200).WithMessage("The subject may have at most 200 characters.");
    }
}
