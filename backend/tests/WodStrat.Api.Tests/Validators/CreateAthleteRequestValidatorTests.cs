using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators;
using WodStrat.Api.ViewModels.Athletes;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for CreateAthleteRequestValidator.
/// </summary>
public class CreateAthleteRequestValidatorTests
{
    private readonly CreateAthleteRequestValidator _validator;

    public CreateAthleteRequestValidatorTests()
    {
        _validator = new CreateAthleteRequestValidator();
    }

    #region Name Validation Tests

    [Fact]
    public void Validate_EmptyName_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = string.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_NullName_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = null!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceeds100Characters_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_NameExactly100Characters_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = new string('A', 100);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameWithLeadingWhitespace_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "  John Doe";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not have leading or trailing whitespace.");
    }

    [Fact]
    public void Validate_NameWithTrailingWhitespace_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "John Doe  ";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not have leading or trailing whitespace.");
    }

    [Fact]
    public void Validate_ValidName_NoValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "John Doe";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region DateOfBirth Validation Tests

    [Fact]
    public void Validate_NullDateOfBirth_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_FutureDateOfBirth_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth must be in the past.");
    }

    [Fact]
    public void Validate_TodayDateOfBirth_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth must be in the past.");
    }

    [Fact]
    public void Validate_AgeTooYoung_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-12));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Age must be between 13 and 120 years.");
    }

    [Fact]
    public void Validate_AgeTooOld_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-121));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Age must be between 13 and 120 years.");
    }

    [Fact]
    public void Validate_ValidAge_NoValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_MinimumValidAge_NoValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13).AddDays(-1));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_MaximumValidAge_NoValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-120));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    #endregion

    #region Gender Validation Tests

    [Fact]
    public void Validate_NullGender_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Gender = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validate_EmptyGender_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Gender = string.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    [InlineData("PreferNotToSay")]
    public void Validate_ValidGender_NoValidationError(string gender)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Gender = gender;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validate_InvalidGender_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Gender = "InvalidGender";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    #endregion

    #region HeightCm Validation Tests

    [Fact]
    public void Validate_NullHeight_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.HeightCm = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HeightCm);
    }

    [Fact]
    public void Validate_HeightTooLow_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.HeightCm = 49;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HeightCm)
            .WithErrorMessage("Height must be between 50 and 300 cm.");
    }

    [Fact]
    public void Validate_HeightTooHigh_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.HeightCm = 301;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HeightCm)
            .WithErrorMessage("Height must be between 50 and 300 cm.");
    }

    [Theory]
    [InlineData(50)]
    [InlineData(175)]
    [InlineData(300)]
    public void Validate_ValidHeight_NoValidationError(decimal height)
    {
        // Arrange
        var request = CreateValidRequest();
        request.HeightCm = height;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HeightCm);
    }

    #endregion

    #region WeightKg Validation Tests

    [Fact]
    public void Validate_NullWeight_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WeightKg = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WeightKg);
    }

    [Fact]
    public void Validate_WeightTooLow_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WeightKg = 19;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WeightKg)
            .WithErrorMessage("Weight must be between 20 and 500 kg.");
    }

    [Fact]
    public void Validate_WeightTooHigh_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WeightKg = 501;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WeightKg)
            .WithErrorMessage("Weight must be between 20 and 500 kg.");
    }

    [Theory]
    [InlineData(20)]
    [InlineData(80)]
    [InlineData(500)]
    public void Validate_ValidWeight_NoValidationError(decimal weight)
    {
        // Arrange
        var request = CreateValidRequest();
        request.WeightKg = weight;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WeightKg);
    }

    #endregion

    #region ExperienceLevel Validation Tests

    [Fact]
    public void Validate_EmptyExperienceLevel_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExperienceLevel = string.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExperienceLevel)
            .WithErrorMessage("Experience level is required.");
    }

    [Theory]
    [InlineData("Beginner")]
    [InlineData("Intermediate")]
    [InlineData("Advanced")]
    public void Validate_ValidExperienceLevel_NoValidationError(string level)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExperienceLevel = level;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExperienceLevel);
    }

    [Theory]
    [InlineData("beginner")]
    [InlineData("INTERMEDIATE")]
    [InlineData("Advanced")]
    public void Validate_ExperienceLevelCaseInsensitive_NoValidationError(string level)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExperienceLevel = level;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExperienceLevel);
    }

    [Fact]
    public void Validate_InvalidExperienceLevel_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExperienceLevel = "InvalidLevel";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExperienceLevel);
    }

    #endregion

    #region PrimaryGoal Validation Tests

    [Fact]
    public void Validate_EmptyPrimaryGoal_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PrimaryGoal = string.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PrimaryGoal)
            .WithErrorMessage("Primary goal is required.");
    }

    [Theory]
    [InlineData("GeneralFitness")]
    [InlineData("BuildStrength")]
    [InlineData("ImprovePacing")]
    [InlineData("CompetitionPrep")]
    [InlineData("WeightManagement")]
    [InlineData("ImproveConditioning")]
    public void Validate_ValidPrimaryGoal_NoValidationError(string goal)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PrimaryGoal = goal;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PrimaryGoal);
    }

    [Theory]
    [InlineData("generalfitness")]
    [InlineData("BUILDSTRENGTH")]
    [InlineData("ImprovePacing")]
    public void Validate_PrimaryGoalCaseInsensitive_NoValidationError(string goal)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PrimaryGoal = goal;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PrimaryGoal);
    }

    [Fact]
    public void Validate_InvalidPrimaryGoal_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PrimaryGoal = "InvalidGoal";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PrimaryGoal);
    }

    #endregion

    #region Full Request Validation Tests

    [Fact]
    public void Validate_ValidRequest_NoValidationErrors()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MinimalValidRequest_NoValidationErrors()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helper Methods

    private static CreateAthleteRequest CreateValidRequest()
    {
        return new CreateAthleteRequest
        {
            Name = "Test Athlete",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            Gender = "Male",
            HeightCm = 180m,
            WeightKg = 85m,
            ExperienceLevel = "Intermediate",
            PrimaryGoal = "ImprovePacing"
        };
    }

    #endregion
}
