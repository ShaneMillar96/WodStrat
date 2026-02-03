using FluentValidation;
using WodStrat.Api.ViewModels.Pacing;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for CalculatePacingRequest.
/// </summary>
public class CalculatePacingRequestValidator : AbstractValidator<CalculatePacingRequest>
{
    public CalculatePacingRequestValidator()
    {
        RuleFor(x => x.AthleteId)
            .GreaterThan(0)
            .WithMessage("Athlete ID must be a positive integer.");

        RuleFor(x => x.WorkoutId)
            .GreaterThan(0)
            .WithMessage("Workout ID must be a positive integer.");
    }
}
