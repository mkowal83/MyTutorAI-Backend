using FluentValidation;
using MyTutorAI.Application.Contracts;
using MyTutorAI.Domain.Users;

namespace MyTutorAI.Application.Auth;

/// <summary>
/// EN: Input validation rules for <see cref="RegisterRequest"/>; registered in DI as a singleton and run by the Api validation filter before the handler.
/// PL: Reguły walidacji wejścia dla <see cref="RegisterRequest"/>; rejestrowane w DI jako singleton i uruchamiane przez filtr walidacji w Api przed handlerem.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// EN: Minimum allowed length of <see cref="RegisterRequest.Password"/>, in characters.
    /// PL: Minimalna dozwolona długość <see cref="RegisterRequest.Password"/> w znakach.
    /// </summary>
    public const int PasswordMinLength = 8;

    /// <summary>
    /// EN: Maximum allowed length of <see cref="RegisterRequest.Password"/>, in characters.
    /// PL: Maksymalna dozwolona długość <see cref="RegisterRequest.Password"/> w znakach.
    /// </summary>
    public const int PasswordMaxLength = 128;

    /// <summary>
    /// EN: Defines the rules: e-mail required, valid format, at most <see cref="User.EmailMaxLength"/>; password required, <see cref="PasswordMinLength"/>–<see cref="PasswordMaxLength"/>; display name required, at most <see cref="User.DisplayNameMaxLength"/>.
    /// PL: Definiuje reguły: e-mail wymagany, poprawny format, najwyżej <see cref="User.EmailMaxLength"/>; hasło wymagane, <see cref="PasswordMinLength"/>–<see cref="PasswordMaxLength"/>; nazwa wyświetlana wymagana, najwyżej <see cref="User.DisplayNameMaxLength"/>.
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(PasswordMinLength, PasswordMaxLength);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(User.DisplayNameMaxLength);
    }
}
