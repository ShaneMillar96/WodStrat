using FluentAssertions;
using FluentValidation.TestHelper;
using WodStrat.Api.Validators;
using WodStrat.Api.ViewModels.Benchmarks;
using Xunit;

namespace WodStrat.Api.Tests.Validators;

/// <summary>
/// Unit tests for RecordBenchmarkRequestValidator.
/// </summary>
public class RecordBenchmarkRequestValidatorTests
{
    private readonly RecordBenchmarkRequestValidator _validator;

    public RecordBenchmarkRequestValidatorTests()
    {
        _validator = new RecordBenchmarkRequestValidator();
    }

    #region BenchmarkDefinitionId Validation Tests

    [Fact]
    public void Validate_EmptyBenchmarkDefinitionId_HasValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BenchmarkDefinitionId = 0;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BenchmarkDefinitionId);
    }

    [Fact]
    public void Validate_ValidBenchmarkDefinitionId_NoValidationError()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BenchmarkDefinitionId);
    }

    #endregion

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
    [InlineData("RX")]
    [InlineData("Scaled")]
    [InlineData("Used 45lb barbell instead of 35lb")]
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
        // Arrange
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = 1,
            Value = 195.5m
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
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = 0,
            Value = 0,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BenchmarkDefinitionId);
        result.ShouldHaveValidationErrorFor(x => x.Value);
        result.ShouldHaveValidationErrorFor(x => x.RecordedAt);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    #endregion

    #region Helper Methods

    private static RecordBenchmarkRequest CreateValidRequest()
    {
        return new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = 1,
            Value = 195.5m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            Notes = "RX"
        };
    }

    #endregion
}
