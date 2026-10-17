using FluentValidation;

namespace MyTutorAI.Application.Contracts.Auth;

/// <summary>
/// EN: Validates <see cref="LogoutRequest"/>: <c>RefreshToken</c> must not be empty unless <c>AllDevices</c> is <c>true</c>. Registered by the assembly scan in <c>AddApplication</c>.
/// PL: Waliduje <see cref="LogoutRequest"/>: <c>RefreshToken</c> nie może być pusty, chyba że <c>AllDevices</c> ma wartość <c>true</c>. Rejestrowany przez skan assembly w <c>AddApplication</c>.
/// </summary>
public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    /// <summary>
    /// EN: Defines the validation rule for <see cref="LogoutRequest"/>.
    /// PL: Definiuje regułę walidacji dla <see cref="LogoutRequest"/>.
    /// </summary>
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .When(x => !x.AllDevices)
            .WithMessage("RefreshToken is required when AllDevices is false.");
    }
}
