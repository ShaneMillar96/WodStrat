using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators;
using WodStrat.Api.ViewModels.Pacing;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for CalculatePacingRequestValidator.
/// </summary>
public class CalculatePacingRequestValidatorTests
{
    private readonly CalculatePacingRequestValidator _validator;

    public CalculatePacingRequestValidatorTests()
    {
        _validator = new CalculatePacingRequestValidator();
    }

    #region AthleteId Validation Tests

    [Fact]
    public void AthleteId_PositiveValue_IsValid()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 1, WorkoutId = 1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AthleteId);
    }

    [Fact]
    public void AthleteId_Zero_HasError()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 0, WorkoutId = 1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId)
            .WithErrorMessage("Athlete ID must be a positive integer.");
    }

    [Fact]
    public void AthleteId_NegativeValue_HasError()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = -1, WorkoutId = 1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId)
            .WithErrorMessage("Athlete ID must be a positive integer.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void AthleteId_VariousPositiveValues_IsValid(int athleteId)
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = athleteId, WorkoutId = 1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AthleteId);
    }

    #endregion

    #region WorkoutId Validation Tests

    [Fact]
    public void WorkoutId_PositiveValue_IsValid()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 1, WorkoutId = 1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Fact]
    public void WorkoutId_Zero_HasError()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 1, WorkoutId = 0 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId)
            .WithErrorMessage("Workout ID must be a positive integer.");
    }

    [Fact]
    public void WorkoutId_NegativeValue_HasError()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 1, WorkoutId = -1 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId)
            .WithErrorMessage("Workout ID must be a positive integer.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void WorkoutId_VariousPositiveValues_IsValid(int workoutId)
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 1, WorkoutId = workoutId };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WorkoutId);
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public void ValidRequest_NoErrors()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 5, WorkoutId = 10 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void InvalidRequest_BothZero_HasMultipleErrors()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = 0, WorkoutId = 0 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void InvalidRequest_BothNegative_HasMultipleErrors()
    {
        // Arrange
        var request = new CalculatePacingRequest { AthleteId = -5, WorkoutId = -10 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    #endregion
}
