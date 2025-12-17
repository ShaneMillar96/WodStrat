using FluentValidation;
using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Dal.Enums;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for CreateWorkoutMovementRequest.
/// </summary>
public class CreateWorkoutMovementRequestValidator : AbstractValidator<CreateWorkoutMovementRequest>
{
    private static readonly string[] ValidLoadUnits = Enum.GetNames<LoadUnit>();
    private static readonly string[] ValidDistanceUnits = Enum.GetNames<DistanceUnit>();

    public CreateWorkoutMovementRequestValidator()
    {
        RuleFor(x => x.MovementDefinitionId)
            .GreaterThan(0)
            .WithMessage("Movement definition ID must be a positive integer.");

        RuleFor(x => x.SequenceOrder)
            .GreaterThan(0)
            .WithMessage("Sequence order must be a positive integer.");

        RuleFor(x => x.RepCount)
            .GreaterThan(0)
            .When(x => x.RepCount.HasValue)
            .WithMessage("Rep count must be greater than 0.");

        RuleFor(x => x.LoadValue)
            .GreaterThan(0)
            .When(x => x.LoadValue.HasValue)
            .WithMessage("Load value must be greater than 0.");

        RuleFor(x => x.LoadUnit)
            .Must(BeValidLoadUnit)
            .When(x => !string.IsNullOrEmpty(x.LoadUnit))
            .WithMessage($"Load unit must be one of: {string.Join(", ", ValidLoadUnits)}.");

        RuleFor(x => x.DistanceValue)
            .GreaterThan(0)
            .When(x => x.DistanceValue.HasValue)
            .WithMessage("Distance value must be greater than 0.");

        RuleFor(x => x.DistanceUnit)
            .Must(BeValidDistanceUnit)
            .When(x => !string.IsNullOrEmpty(x.DistanceUnit))
            .WithMessage($"Distance unit must be one of: {string.Join(", ", ValidDistanceUnits)}.");

        RuleFor(x => x.Calories)
            .GreaterThan(0)
            .When(x => x.Calories.HasValue)
            .WithMessage("Calories must be greater than 0.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .When(x => x.DurationSeconds.HasValue)
            .WithMessage("Duration must be greater than 0 seconds.");

        RuleFor(x => x.Notes)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 200 characters.");
    }

    private static bool BeValidLoadUnit(string? unit)
    {
        return string.IsNullOrEmpty(unit) || ValidLoadUnits.Contains(unit, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidDistanceUnit(string? unit)
    {
        return string.IsNullOrEmpty(unit) || ValidDistanceUnits.Contains(unit, StringComparer.OrdinalIgnoreCase);
    }
}
