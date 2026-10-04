using FluentValidation;

namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Input validation rules for <see cref="LoginRequest"/>; registered in DI as scoped (assembly scan) and run by the Api validation filter before the handler.
/// PL: Reguły walidacji wejścia dla <see cref="LoginRequest"/>; rejestrowane w DI jako scoped (skan assembly) i uruchamiane przez filtr walidacji w Api przed handlerem.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// EN: Defines the rules: e-mail required and in a valid format; password required, with no length rules.
    /// PL: Definiuje reguły: e-mail wymagany i w poprawnym formacie; hasło wymagane, bez reguł długości.
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email has an invalid format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
