using FluentAssertions;
using WodStrat.Services.Dtos;
using WodStrat.Services.Parsing;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing;

/// <summary>
/// Unit tests for LineParseResult class.
/// Tests cover factory methods and property initialization.
/// </summary>
public class LineParseResultTests
{
    #region CreateSuccess Tests

    [Fact]
    public void CreateSuccess_WithMinimalParams_SetsCorrectDefaults()
    {
        // Arrange
        var movement = new ParsedMovementDto
        {
            MovementName = "Push-ups",
            RepCount = 10
        };

        // Act
        var result = LineParseResult.CreateSuccess(
            lineNumber: 1,
            originalText: "10 Push-ups",
            movement: movement,
            confidence: 95);

        // Assert
        result.LineNumber.Should().Be(1);
        result.OriginalText.Should().Be("10 Push-ups");
        result.Success.Should().BeTrue();
        result.ParsedMovement.Should().Be(movement);
        result.Confidence.Should().Be(95);
        result.Error.Should().BeNull();
        result.Warnings.Should().BeEmpty();
        result.Skipped.Should().BeFalse();
        result.SkipReason.Should().BeNull();
    }

    [Fact]
    public void CreateSuccess_WithWarnings_SetsWarnings()
    {
        // Arrange
        var movement = new ParsedMovementDto
        {
            MovementName = "Push-ups",
            RepCount = 10
        };
        var warnings = new List<ParsingError>
        {
            ParsingError.CreateWarning(ParsingErrorCode.ValueOutOfRange, "High rep count")
        };

        // Act
        var result = LineParseResult.CreateSuccess(
            lineNumber: 1,
            originalText: "1000 Push-ups",
            movement: movement,
            confidence: 60,
            warnings: warnings);

        // Assert
        result.Success.Should().BeTrue();
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Code.Should().Be(ParsingErrorCode.ValueOutOfRange);
    }

    [Fact]
    public void CreateSuccess_NullWarnings_DefaultsToEmptyList()
    {
        // Arrange
        var movement = new ParsedMovementDto { MovementName = "Push-ups" };

        // Act
        var result = LineParseResult.CreateSuccess(
            lineNumber: 1,
            originalText: "10 Push-ups",
            movement: movement,
            confidence: 100,
            warnings: null);

        // Assert
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().BeEmpty();
    }

    #endregion

    #region CreateFailure Tests

    [Fact]
    public void CreateFailure_SetsCorrectProperties()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.UnrecognizedMovementFormat,
            "Could not parse movement");

        // Act
        var result = LineParseResult.CreateFailure(
            lineNumber: 5,
            originalText: "??? unknown ???",
            error: error);

        // Assert
        result.LineNumber.Should().Be(5);
        result.OriginalText.Should().Be("??? unknown ???");
        result.Success.Should().BeFalse();
        result.ParsedMovement.Should().BeNull();
        result.Error.Should().Be(error);
        result.Confidence.Should().Be(0);
        result.Skipped.Should().BeFalse();
    }

    [Fact]
    public void CreateFailure_ErrorContainsLineInfo()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count '-5'",
            lineNumber: 3);

        // Act
        var result = LineParseResult.CreateFailure(
            lineNumber: 3,
            originalText: "-5 Push-ups",
            error: error);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.LineNumber.Should().Be(3);
        result.Error!.Code.Should().Be(ParsingErrorCode.InvalidRepCount);
    }

    #endregion

    #region CreateSkipped Tests

    [Fact]
    public void CreateSkipped_SetsSkippedFlag()
    {
        // Act
        var result = LineParseResult.CreateSkipped(
            lineNumber: 1,
            originalText: "AMRAP 20 minutes:",
            reason: "Header line");

        // Assert
        result.Skipped.Should().BeTrue();
        result.SkipReason.Should().Be("Header line");
        result.Success.Should().BeFalse();
        result.Confidence.Should().Be(0);
    }

    [Fact]
    public void CreateSkipped_PreservesOriginalText()
    {
        // Act
        var result = LineParseResult.CreateSkipped(
            lineNumber: 3,
            originalText: "",
            reason: "Empty line");

        // Assert
        result.LineNumber.Should().Be(3);
        result.OriginalText.Should().BeEmpty();
        result.SkipReason.Should().Be("Empty line");
    }

    [Theory]
    [InlineData("Header line")]
    [InlineData("Empty line")]
    [InlineData("Comment")]
    [InlineData("Separator")]
    public void CreateSkipped_AcceptsVariousReasons(string reason)
    {
        // Act
        var result = LineParseResult.CreateSkipped(
            lineNumber: 1,
            originalText: "---",
            reason: reason);

        // Assert
        result.SkipReason.Should().Be(reason);
        result.Skipped.Should().BeTrue();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void LineNumber_CanBeAnyPositiveInteger()
    {
        // Arrange
        var movement = new ParsedMovementDto { MovementName = "Test" };

        // Act
        var result1 = LineParseResult.CreateSuccess(1, "text", movement, 100);
        var result2 = LineParseResult.CreateSuccess(100, "text", movement, 100);
        var result3 = LineParseResult.CreateSuccess(int.MaxValue, "text", movement, 100);

        // Assert
        result1.LineNumber.Should().Be(1);
        result2.LineNumber.Should().Be(100);
        result3.LineNumber.Should().Be(int.MaxValue);
    }

    [Fact]
    public void OriginalText_CanBeEmpty()
    {
        // Act
        var result = LineParseResult.CreateSkipped(1, "", "Empty line");

        // Assert
        result.OriginalText.Should().BeEmpty();
    }

    [Fact]
    public void Confidence_RangesFrom0To100()
    {
        // Arrange
        var movement = new ParsedMovementDto { MovementName = "Test" };

        // Act
        var lowConfidence = LineParseResult.CreateSuccess(1, "text", movement, 0);
        var highConfidence = LineParseResult.CreateSuccess(1, "text", movement, 100);
        var midConfidence = LineParseResult.CreateSuccess(1, "text", movement, 50);

        // Assert
        lowConfidence.Confidence.Should().Be(0);
        highConfidence.Confidence.Should().Be(100);
        midConfidence.Confidence.Should().Be(50);
    }

    #endregion

    #region Default Initialization Tests

    [Fact]
    public void LineParseResult_DefaultOriginalText_IsEmpty()
    {
        // Arrange
        var result = new LineParseResult();

        // Assert
        result.OriginalText.Should().BeEmpty();
    }

    [Fact]
    public void LineParseResult_DefaultWarnings_IsEmptyList()
    {
        // Arrange
        var result = new LineParseResult();

        // Assert
        result.Warnings.Should().BeEmpty();
    }

    #endregion
}
