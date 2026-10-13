using FluentValidation;

namespace MyTutorAI.Application.Contracts.Sessions;

/// <summary>
/// EN: Validates <see cref="SendMessageRequest"/>: <c>Message</c> is required and at most 4000 characters. Failures are returned as per-field 400 errors by the API validation filter.
/// PL: Waliduje <see cref="SendMessageRequest"/>: <c>Message</c> jest wymagane i ma najwyżej 4000 znaków. Błędy zwracane są przez filtr walidacji API jako błędy pól z kodem 400.
/// </summary>
public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    /// <summary>
    /// EN: Creates the validator with its rules.
    /// PL: Tworzy walidator z jego regułami.
    /// </summary>
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters.");
    }
}