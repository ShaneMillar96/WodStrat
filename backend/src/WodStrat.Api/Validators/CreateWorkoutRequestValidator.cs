using FluentValidation;
using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Dal.Enums;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for CreateWorkoutRequest.
/// </summary>
public class CreateWorkoutRequestValidator : AbstractValidator<CreateWorkoutRequest>
{
    private static readonly string[] ValidWorkoutTypes = Enum.GetNames<WorkoutType>();

    public CreateWorkoutRequestValidator()
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

        RuleFor(x => x.OriginalText)
            .NotEmpty()
            .WithMessage("Original text is required.")
            .MaximumLength(10000)
            .WithMessage("Original text must not exceed 10,000 characters.");

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

        RuleFor(x => x.Movements)
            .NotEmpty()
            .WithMessage("At least one movement is required.");

        RuleForEach(x => x.Movements)
            .SetValidator(new CreateWorkoutMovementRequestValidator());
    }

    private static bool BeValidWorkoutType(string workoutType)
    {
        return ValidWorkoutTypes.Contains(workoutType, StringComparer.OrdinalIgnoreCase);
    }
}
