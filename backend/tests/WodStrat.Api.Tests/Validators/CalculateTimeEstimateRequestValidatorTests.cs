using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators.TimeEstimate;
using WodStrat.Api.ViewModels.TimeEstimate;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for CalculateTimeEstimateRequestValidator.
/// </summary>
public class CalculateTimeEstimateRequestValidatorTests
{
    private readonly CalculateTimeEstimateRequestValidator _sut;

    public CalculateTimeEstimateRequestValidatorTests()
    {
        _sut = new CalculateTimeEstimateRequestValidator();
    }

    #region AthleteId Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void AthleteId_PositiveValue_ShouldNotHaveError(int athleteId)
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = athleteId,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AthleteId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AthleteId_NonPositiveValue_ShouldHaveError(int athleteId)
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = athleteId,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId)
            .WithErrorMessage("Athlete ID must be a positive integer.");
    }

    #endregion

    #region WorkoutId Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void WorkoutId_PositiveValue_ShouldNotHaveError(int workoutId)
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = 1,
            WorkoutId = workoutId
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void WorkoutId_NonPositiveValue_ShouldHaveError(int workoutId)
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = 1,
            WorkoutId = workoutId
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId)
            .WithErrorMessage("Workout ID must be a positive integer.");
    }

    #endregion

    #region Complete Request Tests

    [Fact]
    public void ValidRequest_ShouldBeValid()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = 123,
            WorkoutId = 456
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void InvalidRequest_BothFieldsZero_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = 0,
            WorkoutId = 0
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.ShouldHaveValidationErrorFor(x => x.AthleteId);
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Fact]
    public void InvalidRequest_BothFieldsNegative_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = -1,
            WorkoutId = -1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    #endregion
}
