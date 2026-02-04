using FluentValidation;
using WodStrat.Api.ViewModels.TimeEstimate;

namespace WodStrat.Api.Validators.TimeEstimate;

/// <summary>
/// Validator for CalculateTimeEstimateRequest.
/// </summary>
public class CalculateTimeEstimateRequestValidator : AbstractValidator<CalculateTimeEstimateRequest>
{
    public CalculateTimeEstimateRequestValidator()
    {
        RuleFor(x => x.AthleteId)
            .GreaterThan(0)
            .WithMessage("Athlete ID must be a positive integer.");

        RuleFor(x => x.WorkoutId)
            .GreaterThan(0)
            .WithMessage("Workout ID must be a positive integer.");
    }
}
