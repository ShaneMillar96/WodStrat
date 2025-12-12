using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators;
using WodStrat.Api.ViewModels.Benchmarks;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for UpdateBenchmarkRequestValidator.
/// </summary>
public class UpdateBenchmarkRequestValidatorTests
{
    private readonly UpdateBenchmarkRequestValidator _validator;

    public UpdateBenchmarkRequestValidatorTests()
    {
        _validator = new UpdateBenchmarkRequestValidator();
    }

    #region Value Validation Tests

    [Fact]
    public void Validate_ZeroValue_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Value = 0;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorMessage("Value must be greater than 0.");
    }

    [Fact]
    public void Validate_NegativeValue_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Value = -1;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorMessage("Value must be greater than 0.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000.5)]
    public void Validate_PositiveValue_NoValidationError(decimal value)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Value = value;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    #endregion

    #region RecordedAt Validation Tests

    [Fact]
    public void Validate_NullRecordedAt_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RecordedAt = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecordedAt);
    }

    [Fact]
    public void Validate_FutureRecordedAt_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecordedAt)
            .WithErrorMessage("Recorded date cannot be in the future.");
    }

    [Fact]
    public void Validate_TodayRecordedAt_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecordedAt);
    }

    [Fact]
    public void Validate_PastRecordedAt_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecordedAt);
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_NullNotes_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_EmptyNotes_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = string.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_NotesExactly500Characters_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = new string('A', 500);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_NotesExceeds500Characters_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = new string('A', 501);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 500 characters.");
    }

    [Theory]
    [InlineData("PR")]
    [InlineData("RX - Personal Best")]
    [InlineData("Improved from last week's 3:30")]
    public void Validate_ValidNotes_NoValidationError(string notes)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = notes;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
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
        // Arrange - Only value is truly required
        var request = new UpdateBenchmarkRequest
        {
            Value = 180m
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MultipleInvalidFields_HasMultipleErrors()
    {
        // Arrange
        var request = new UpdateBenchmarkRequest
        {
            Value = 0,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
        result.ShouldHaveValidationErrorFor(x => x.RecordedAt);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_VerySmallPositiveValue_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Value = 0.001m;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_VeryLargeValue_IsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Value = 999999.99m;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    #endregion

    #region Helper Methods

    private static UpdateBenchmarkRequest CreateValidRequest()
    {
        return new UpdateBenchmarkRequest
        {
            Value = 180m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            Notes = "PR"
        };
    }

    #endregion
}
