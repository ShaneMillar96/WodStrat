using FluentValidation;
using WodStrat.Api.ViewModels.VolumeLoad;

namespace WodStrat.Api.Validators.VolumeLoad;

/// <summary>
/// Validator for CalculateVolumeLoadRequest.
/// </summary>
public class CalculateVolumeLoadRequestValidator : AbstractValidator<CalculateVolumeLoadRequest>
{
    public CalculateVolumeLoadRequestValidator()
    {
        RuleFor(x => x.AthleteId)
            .GreaterThan(0)
            .WithMessage("Athlete ID must be a positive integer.");

        RuleFor(x => x.WorkoutId)
            .GreaterThan(0)
            .WithMessage("Workout ID must be a positive integer.");
    }
}
