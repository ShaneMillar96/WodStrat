using FluentValidation;
using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Dal.Enums;

namespace WodStrat.Api.Validators;

/// <summary>
/// Validator for UpdateAthleteRequest.
/// </summary>
public class UpdateAthleteRequestValidator : AbstractValidator<UpdateAthleteRequest>
{
    private static readonly string[] ValidGenders = { "Male", "Female", "Other", "PreferNotToSay" };

    public UpdateAthleteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.")
            .Must(name => name == null || name == name.Trim())
            .WithMessage("Name must not have leading or trailing whitespace.");

        RuleFor(x => x.DateOfBirth)
            .Must(BeInThePast)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.")
            .Must(ResultInValidAge)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Age must be between 13 and 120 years.");

        RuleFor(x => x.Gender)
            .Must(BeValidGender)
            .When(x => !string.IsNullOrEmpty(x.Gender))
            .WithMessage($"Gender must be one of: {string.Join(", ", ValidGenders)}.");

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 300)
            .When(x => x.HeightCm.HasValue)
            .WithMessage("Height must be between 50 and 300 cm.");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(20, 500)
            .When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be between 20 and 500 kg.");

        RuleFor(x => x.ExperienceLevel)
            .NotEmpty()
            .WithMessage("Experience level is required.")
            .Must(BeValidExperienceLevel)
            .WithMessage($"Experience level must be one of: {string.Join(", ", Enum.GetNames<ExperienceLevel>())}.");

        RuleFor(x => x.PrimaryGoal)
            .NotEmpty()
            .WithMessage("Primary goal is required.")
            .Must(BeValidAthleteGoal)
            .WithMessage($"Primary goal must be one of: {string.Join(", ", Enum.GetNames<AthleteGoal>())}.");
    }

    private static bool BeInThePast(DateOnly? dateOfBirth)
    {
        return dateOfBirth < DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private static bool ResultInValidAge(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value > today.AddYears(-age)) age--;

        return age >= 13 && age <= 120;
    }

    private static bool BeValidGender(string? gender)
    {
        return string.IsNullOrEmpty(gender) || ValidGenders.Contains(gender);
    }

    private static bool BeValidExperienceLevel(string experienceLevel)
    {
        return Enum.TryParse<ExperienceLevel>(experienceLevel, ignoreCase: true, out _);
    }

    private static bool BeValidAthleteGoal(string primaryGoal)
    {
        return Enum.TryParse<AthleteGoal>(primaryGoal, ignoreCase: true, out _);
    }
}
