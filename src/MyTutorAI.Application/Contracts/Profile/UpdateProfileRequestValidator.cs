using FluentValidation;

namespace MyTutorAI.Application.Contracts.Profile;

/// <summary>
/// EN: Input validation rules for <see cref="UpdateProfileRequest"/>; registered in DI as scoped (assembly scan) and run by the Api validation filter before the handler.
/// PL: Reguły walidacji wejścia dla <see cref="UpdateProfileRequest"/>; rejestrowane w DI jako scoped (skan assembly) i uruchamiane przez filtr walidacji w Api przed handlerem.
/// </summary>
public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    /// <summary>
    /// EN: Defines the rules: name (max 100), level (max 40), goal (max 500) and both languages (max 60) required; preference entries non-empty, max 300; each goal's text required, max 300; work context and communication preference scalars max 120, their list entries non-empty, max 200; optional sections are checked only when present.
    /// PL: Definiuje reguły: imię (maks. 100), poziom (maks. 40), cel (maks. 500) i oba języki (maks. 60) wymagane; wpisy preferencji niepuste, maks. 300; treść każdego celu wymagana, maks. 300; pola skalarne kontekstu zawodowego i preferencji komunikacyjnych maks. 120, wpisy ich list niepuste, maks. 200; sekcje opcjonalne sprawdzane tylko, gdy są obecne.
    /// </summary>
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required.")
            .MaximumLength(40).WithMessage("Level must not exceed 40 characters.");

        RuleFor(x => x.Goal)
            .NotEmpty().WithMessage("Goal is required.")
            .MaximumLength(500).WithMessage("Goal must not exceed 500 characters.");

        RuleFor(x => x.NativeLanguage)
            .NotEmpty().WithMessage("NativeLanguage is required.")
            .MaximumLength(60).WithMessage("NativeLanguage must not exceed 60 characters.");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty().WithMessage("TargetLanguage is required.")
            .MaximumLength(60).WithMessage("TargetLanguage must not exceed 60 characters.");

        RuleForEach(x => x.Preferences)
            .NotEmpty().WithMessage("Preference must not be empty.")
            .MaximumLength(300).WithMessage("Preference must not exceed 300 characters.")
            .When(x => x.Preferences is not null);

        RuleForEach(x => x.Goals)
            .ChildRules(goal =>
            {
                goal.RuleFor(g => g.Goal)
                    .NotEmpty().WithMessage("Operational goal's Goal is required.")
                    .MaximumLength(300).WithMessage("Operational goal's Goal must not exceed 300 characters.");
            })
            .When(x => x.Goals is not null);

        When(x => x.WorkContext is not null, () =>
        {
            RuleFor(x => x.WorkContext!.Profession)
                .MaximumLength(120).WithMessage("Profession must not exceed 120 characters.");
            RuleFor(x => x.WorkContext!.CompanyName)
                .MaximumLength(120).WithMessage("CompanyName must not exceed 120 characters.");
            RuleFor(x => x.WorkContext!.Industry)
                .MaximumLength(120).WithMessage("Industry must not exceed 120 characters.");
            RuleForEach(x => x.WorkContext!.Stakeholders)
                .NotEmpty().WithMessage("Stakeholder must not be empty.")
                .MaximumLength(200).WithMessage("Stakeholder must not exceed 200 characters.")
                .When(x => x.WorkContext!.Stakeholders is not null);
        });

        When(x => x.CommunicationPreferences is not null, () =>
        {
            RuleFor(x => x.CommunicationPreferences!.FeedbackStyle)
                .MaximumLength(120).WithMessage("FeedbackStyle must not exceed 120 characters.");
            RuleFor(x => x.CommunicationPreferences!.CorrectionIntensity)
                .MaximumLength(120).WithMessage("CorrectionIntensity must not exceed 120 characters.");
            RuleFor(x => x.CommunicationPreferences!.Pace)
                .MaximumLength(120).WithMessage("Pace must not exceed 120 characters.");
            RuleForEach(x => x.CommunicationPreferences!.PreferredFormats)
                .NotEmpty().WithMessage("PreferredFormat must not be empty.")
                .MaximumLength(200).WithMessage("PreferredFormat must not exceed 200 characters.")
                .When(x => x.CommunicationPreferences!.PreferredFormats is not null);
        });
    }
}
