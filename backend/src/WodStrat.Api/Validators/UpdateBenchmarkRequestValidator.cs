using FluentValidation;
using WodStrat.Api.ViewModels.Benchmarks;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for UpdateBenchmarkRequest.
/// </summary>
public class UpdateBenchmarkRequestValidator : AbstractValidator<UpdateBenchmarkRequest>
{
    public UpdateBenchmarkRequestValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Value must be greater than 0.");

        RuleFor(x => x.RecordedAt)
            .Must(BeNotInTheFuture)
            .When(x => x.RecordedAt.HasValue)
            .WithMessage("Recorded date cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters.");
    }

    private static bool BeNotInTheFuture(DateOnly? date)
    {
        if (!date.HasValue) return true;
        return date.Value <= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
