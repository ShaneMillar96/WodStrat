using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators.VolumeLoad;
using WodStrat.Api.ViewModels.VolumeLoad;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for CalculateVolumeLoadRequestValidator.
/// </summary>
public class CalculateVolumeLoadRequestValidatorTests
{
    private readonly CalculateVolumeLoadRequestValidator _sut;

    public CalculateVolumeLoadRequestValidatorTests()
    {
        _sut = new CalculateVolumeLoadRequestValidator();
    }

    #region AthleteId Validation

    [Fact]
    public void Validate_ValidAthleteId_NoErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 1,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AthleteId);
    }

    [Fact]
    public void Validate_ZeroAthleteId_HasError()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 0,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId);
    }

    [Fact]
    public void Validate_NegativeAthleteId_HasError()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = -1,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId);
    }

    [Fact]
    public void Validate_LargeAthleteId_NoErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = int.MaxValue,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AthleteId);
    }

    #endregion

    #region WorkoutId Validation

    [Fact]
    public void Validate_ValidWorkoutId_NoErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 1,
            WorkoutId = 1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Fact]
    public void Validate_ZeroWorkoutId_HasError()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 1,
            WorkoutId = 0
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Fact]
    public void Validate_NegativeWorkoutId_HasError()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 1,
            WorkoutId = -1
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId);
    }

    [Fact]
    public void Validate_LargeWorkoutId_NoErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 1,
            WorkoutId = int.MaxValue
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WorkoutId);
    }

    #endregion

    #region Combined Validation

    [Fact]
    public void Validate_BothIdsInvalid_HasMultipleErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 0,
            WorkoutId = -5
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AthleteId);
        result.ShouldHaveValidationErrorFor(x => x.WorkoutId);
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Validate_BothIdsValid_NoErrors()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 100,
            WorkoutId = 200
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
