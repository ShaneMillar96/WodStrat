using FluentAssertions;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for ParsingError class.
/// Tests cover factory methods, property initialization, and immutability.
/// </summary>
public class ParsingErrorTests
{
    #region CreateError Factory Tests

    [Fact]
    public void CreateError_WithMinimalParams_SetsCorrectDefaults()
    {
        // Act
        var error = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Test error message");

        // Assert
        error.Code.Should().Be(ParsingErrorCode.EmptyInput);
        error.Message.Should().Be("Test error message");
        error.Severity.Should().Be(ParsingErrorSeverity.Error);
        error.LineNumber.Should().BeNull();
        error.Context.Should().BeNull();
        error.Suggestion.Should().BeNull();
        error.SimilarNames.Should().BeNull();
    }

    [Fact]
    public void CreateError_WithAllParams_SetsAllProperties()
    {
        // Act
        var error = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count '-5'",
            lineNumber: 3,
            context: "-5 Push-ups",
            suggestion: "Use a positive number");

        // Assert
        error.Code.Should().Be(ParsingErrorCode.InvalidRepCount);
        error.Message.Should().Be("Invalid rep count '-5'");
        error.Severity.Should().Be(ParsingErrorSeverity.Error);
        error.LineNumber.Should().Be(3);
        error.Context.Should().Be("-5 Push-ups");
        error.Suggestion.Should().Be("Use a positive number");
    }

    [Fact]
    public void CreateError_AlwaysSetsSeverityToError()
    {
        // Act
        var error = ParsingError.CreateError(
            ParsingErrorCode.NoMovementsDetected,
            "No movements found");

        // Assert
        error.Severity.Should().Be(ParsingErrorSeverity.Error);
    }

    #endregion

    #region CreateWarning Factory Tests

    [Fact]
    public void CreateWarning_WithMinimalParams_SetsCorrectDefaults()
    {
        // Act
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Unknown movement 'burpies'");

        // Assert
        warning.Code.Should().Be(ParsingErrorCode.UnknownMovement);
        warning.Message.Should().Be("Unknown movement 'burpies'");
        warning.Severity.Should().Be(ParsingErrorSeverity.Warning);
        warning.LineNumber.Should().BeNull();
        warning.Context.Should().BeNull();
        warning.Suggestion.Should().BeNull();
        warning.SimilarNames.Should().BeNull();
    }

    [Fact]
    public void CreateWarning_WithSimilarNames_SetsSimilarNamesProperty()
    {
        // Arrange
        var similarNames = new List<string> { "Burpees", "Burpee Box Jump Overs" };

        // Act
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Unknown movement 'burpies'",
            lineNumber: 5,
            context: "10 burpies",
            suggestion: "Did you mean 'Burpees'?",
            similarNames: similarNames);

        // Assert
        warning.SimilarNames.Should().NotBeNull();
        warning.SimilarNames.Should().HaveCount(2);
        warning.SimilarNames.Should().Contain("Burpees");
        warning.SimilarNames.Should().Contain("Burpee Box Jump Overs");
    }

    [Fact]
    public void CreateWarning_AlwaysSetsSeverityToWarning()
    {
        // Act
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.AmbiguousWorkoutType,
            "Multiple workout types detected");

        // Assert
        warning.Severity.Should().Be(ParsingErrorSeverity.Warning);
    }

    #endregion

    #region CreateInfo Factory Tests

    [Fact]
    public void CreateInfo_WithMinimalParams_SetsCorrectDefaults()
    {
        // Act
        var info = ParsingError.CreateInfo(
            ParsingErrorCode.DuplicateMovement,
            "Movement appears twice");

        // Assert
        info.Code.Should().Be(ParsingErrorCode.DuplicateMovement);
        info.Message.Should().Be("Movement appears twice");
        info.Severity.Should().Be(ParsingErrorSeverity.Info);
        info.LineNumber.Should().BeNull();
        info.Context.Should().BeNull();
        info.Suggestion.Should().BeNull();
        info.SimilarNames.Should().BeNull();
    }

    [Fact]
    public void CreateInfo_WithAllParams_SetsAllProperties()
    {
        // Act
        var info = ParsingError.CreateInfo(
            ParsingErrorCode.DuplicateMovement,
            "Push-ups appears multiple times",
            lineNumber: 7,
            context: "10 Push-ups");

        // Assert
        info.Code.Should().Be(ParsingErrorCode.DuplicateMovement);
        info.LineNumber.Should().Be(7);
        info.Context.Should().Be("10 Push-ups");
        info.Severity.Should().Be(ParsingErrorSeverity.Info);
    }

    [Fact]
    public void CreateInfo_AlwaysSetsSeverityToInfo()
    {
        // Act
        var info = ParsingError.CreateInfo(
            ParsingErrorCode.DuplicateMovement,
            "Duplicate detected");

        // Assert
        info.Severity.Should().Be(ParsingErrorSeverity.Info);
    }

    #endregion

    #region Property Initialization Tests

    [Fact]
    public void ParsingError_CanBeCreatedWithInitSyntax()
    {
        // Act
        var error = new ParsingError
        {
            Code = ParsingErrorCode.InputTooLong,
            Message = "Input exceeds maximum length",
            Suggestion = "Reduce text length",
            LineNumber = null,
            Severity = ParsingErrorSeverity.Error,
            Context = "Very long text...",
            SimilarNames = null
        };

        // Assert
        error.Code.Should().Be(ParsingErrorCode.InputTooLong);
        error.Message.Should().Be("Input exceeds maximum length");
        error.Suggestion.Should().Be("Reduce text length");
        error.Severity.Should().Be(ParsingErrorSeverity.Error);
        error.Context.Should().Be("Very long text...");
    }

    [Fact]
    public void ParsingError_DefaultMessageIsEmptyString()
    {
        // Act
        var error = new ParsingError
        {
            Code = ParsingErrorCode.EmptyInput,
            Severity = ParsingErrorSeverity.Error
        };

        // Assert
        error.Message.Should().BeEmpty();
    }

    #endregion

    #region Line Number Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void CreateError_WithValidLineNumber_SetsLineNumber(int lineNumber)
    {
        // Act
        var error = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: lineNumber);

        // Assert
        error.LineNumber.Should().Be(lineNumber);
    }

    [Fact]
    public void CreateError_WithNullLineNumber_LeavesLineNumberNull()
    {
        // Act
        var error = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Empty input",
            lineNumber: null);

        // Assert
        error.LineNumber.Should().BeNull();
    }

    #endregion
}
