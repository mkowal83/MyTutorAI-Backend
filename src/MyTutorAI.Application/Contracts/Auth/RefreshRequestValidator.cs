using FluentValidation;

namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Input validation rules for <see cref="RefreshRequest"/>; registered in DI as scoped (assembly scan) and run by the Api validation filter before the handler.
/// PL: Reguły walidacji wejścia dla <see cref="RefreshRequest"/>; rejestrowane w DI jako scoped (skan assembly) i uruchamiane przez filtr walidacji Api przed handlerem.
/// </summary>
public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    /// <summary>
    /// EN: Defines the rules: refresh token required (not null, empty or whitespace), with no format rules.
    /// PL: Definiuje reguły: refresh token wymagany (nie null, nie pusty i nie same białe znaki), bez reguł formatu.
    /// </summary>
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}
