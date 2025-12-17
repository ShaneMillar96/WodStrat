using FluentValidation;
using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Dal.Enums;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for UpdateWorkoutRequest.
/// </summary>
public class UpdateWorkoutRequestValidator : AbstractValidator<UpdateWorkoutRequest>
{
    private static readonly string[] ValidWorkoutTypes = Enum.GetNames<WorkoutType>();

    public UpdateWorkoutRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.WorkoutType)
            .NotEmpty()
            .WithMessage("Workout type is required.")
            .Must(BeValidWorkoutType)
            .WithMessage($"Workout type must be one of: {string.Join(", ", ValidWorkoutTypes)}.");

        RuleFor(x => x.TimeCapSeconds)
            .GreaterThan(0)
            .When(x => x.TimeCapSeconds.HasValue)
            .WithMessage("Time cap must be greater than 0 seconds.");

        RuleFor(x => x.RoundCount)
            .GreaterThan(0)
            .When(x => x.RoundCount.HasValue)
            .WithMessage("Round count must be greater than 0.");

        RuleFor(x => x.IntervalDurationSeconds)
            .GreaterThan(0)
            .When(x => x.IntervalDurationSeconds.HasValue)
            .WithMessage("Interval duration must be greater than 0 seconds.");

        RuleForEach(x => x.Movements)
            .SetValidator(new CreateWorkoutMovementRequestValidator())
            .When(x => x.Movements != null && x.Movements.Any());
    }

    private static bool BeValidWorkoutType(string workoutType)
    {
        return ValidWorkoutTypes.Contains(workoutType, StringComparer.OrdinalIgnoreCase);
    }
}
