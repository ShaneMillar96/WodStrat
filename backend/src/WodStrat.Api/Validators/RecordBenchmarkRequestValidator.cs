using FluentValidation;
using WodStrat.Api.ViewModels.Benchmarks;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for RecordBenchmarkRequest.
/// </summary>
public class RecordBenchmarkRequestValidator : AbstractValidator<RecordBenchmarkRequest>
{
    public RecordBenchmarkRequestValidator()
    {
        RuleFor(x => x.BenchmarkDefinitionId)
            .NotEmpty()
            .WithMessage("Benchmark definition ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Benchmark definition ID must be a valid GUID.");

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
