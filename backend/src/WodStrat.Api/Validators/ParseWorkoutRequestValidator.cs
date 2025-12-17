using FluentValidation;
using WodStrat.Api.ViewModels.Workouts;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for ParseWorkoutRequest.
/// </summary>
public class ParseWorkoutRequestValidator : AbstractValidator<ParseWorkoutRequest>
{
    public ParseWorkoutRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Workout text is required.")
            .MaximumLength(10000)
            .WithMessage("Workout text must not exceed 10,000 characters.");
    }
}
