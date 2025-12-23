using FluentAssertions;
using WodStrat.Services.Extensions;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Extensions;

/// <summary>
/// Unit tests for ParsingErrorExtensions class.
/// Tests cover DTO conversion for errors, warnings, and info messages.
/// </summary>
public class ParsingErrorExtensionsTests
{
    #region ToErrorDto Tests

    [Fact]
    public void ToErrorDto_WithAllProperties_MapsAllProperties()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.UnknownMovement,
            "Movement 'Pushup' not recognized.",
            lineNumber: 5,
            context: "10 Pushup",
            suggestion: "Check spelling or use standard movement names.");

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.ErrorType.Should().Be("UnknownMovement");
        dto.ErrorCode.Should().Be((int)ParsingErrorCode.UnknownMovement);
        dto.Message.Should().Be("Movement 'Pushup' not recognized.");
        dto.LineNumber.Should().Be(5);
        dto.OriginalText.Should().Be("10 Pushup");
        dto.Suggestion.Should().Be("Check spelling or use standard movement names.");
    }

    [Fact]
    public void ToErrorDto_WithNullLineNumber_DefaultsToZero()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Workout text cannot be empty.");

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.LineNumber.Should().Be(0);
    }

    [Fact]
    public void ToErrorDto_WithNullContext_PreservesNull()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Workout text cannot be empty.");

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.OriginalText.Should().BeNull();
    }

    [Fact]
    public void ToErrorDto_WithNullSimilarNames_PreservesNull()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: 3);

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.SimilarNames.Should().BeNull();
    }

    [Theory]
    [InlineData(ParsingErrorCode.EmptyInput)]
    [InlineData(ParsingErrorCode.InputTooLong)]
    [InlineData(ParsingErrorCode.InvalidRepCount)]
    [InlineData(ParsingErrorCode.UnknownMovement)]
    public void ToErrorDto_PreservesErrorCode(ParsingErrorCode code)
    {
        // Arrange
        var error = ParsingError.CreateError(code, "Test message");

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.ErrorType.Should().Be(code.ToString());
        dto.ErrorCode.Should().Be((int)code);
    }

    #endregion

    #region ToWarningDto Tests

    [Fact]
    public void ToWarningDto_WithAllProperties_MapsAllProperties()
    {
        // Arrange
        var similarNames = new List<string> { "Burpees" };
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Movement 'Burpies' not recognized.",
            lineNumber: 3,
            context: "15 Burpies",
            suggestion: "Did you mean 'Burpees'?",
            similarNames: similarNames);

        // Act
        var dto = warning.ToWarningDto();

        // Assert
        dto.WarningType.Should().Be("UnknownMovement");
        dto.WarningCode.Should().Be((int)ParsingErrorCode.UnknownMovement);
        dto.Message.Should().Be("Movement 'Burpies' not recognized.");
        dto.LineNumber.Should().Be(3);
        dto.OriginalText.Should().Be("15 Burpies");
        dto.Suggestion.Should().Be("Did you mean 'Burpees'?");
        dto.SimilarNames.Should().BeEquivalentTo(similarNames);
    }

    [Fact]
    public void ToWarningDto_WithNullLineNumber_DefaultsToZero()
    {
        // Arrange
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.NoWorkoutStructure,
            "No workout structure detected.");

        // Act
        var dto = warning.ToWarningDto();

        // Assert
        dto.LineNumber.Should().Be(0);
    }

    [Fact]
    public void ToWarningDto_FromInfoSeverity_StillConverts()
    {
        // Arrange
        var info = ParsingError.CreateInfo(
            ParsingErrorCode.DuplicateMovement,
            "Duplicate movement detected.",
            lineNumber: 7);

        // Act
        var dto = info.ToWarningDto();

        // Assert
        dto.WarningType.Should().Be("DuplicateMovement");
        dto.LineNumber.Should().Be(7);
    }

    #endregion

    #region ToDtos Collection Tests

    [Fact]
    public void ToDtos_EmptyCollection_ReturnsBothEmptyLists()
    {
        // Arrange
        var errors = Enumerable.Empty<ParsingError>();

        // Act
        var (errorDtos, warningDtos) = errors.ToDtos();

        // Assert
        errorDtos.Should().BeEmpty();
        warningDtos.Should().BeEmpty();
    }

    [Fact]
    public void ToDtos_OnlyErrors_ReturnsErrorsInErrorList()
    {
        // Arrange
        var errors = new List<ParsingError>
        {
            ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"),
            ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2")
        };

        // Act
        var (errorDtos, warningDtos) = errors.ToDtos();

        // Assert
        errorDtos.Should().HaveCount(2);
        warningDtos.Should().BeEmpty();
    }

    [Fact]
    public void ToDtos_OnlyWarnings_ReturnsWarningsInWarningList()
    {
        // Arrange
        var errors = new List<ParsingError>
        {
            ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning 1"),
            ParsingError.CreateWarning(ParsingErrorCode.ValueOutOfRange, "Warning 2")
        };

        // Act
        var (errorDtos, warningDtos) = errors.ToDtos();

        // Assert
        errorDtos.Should().BeEmpty();
        warningDtos.Should().HaveCount(2);
    }

    [Fact]
    public void ToDtos_OnlyInfo_ReturnsInfoInWarningList()
    {
        // Arrange
        var errors = new List<ParsingError>
        {
            ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info 1"),
            ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info 2", lineNumber: 5)
        };

        // Act
        var (errorDtos, warningDtos) = errors.ToDtos();

        // Assert
        errorDtos.Should().BeEmpty();
        warningDtos.Should().HaveCount(2);
    }

    [Fact]
    public void ToDtos_MixedSeverities_CorrectlyPartitions()
    {
        // Arrange
        var errors = new List<ParsingError>
        {
            ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"),
            ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning 1"),
            ParsingError.CreateError(ParsingErrorCode.InvalidRepCount, "Error 2"),
            ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info 1"),
            ParsingError.CreateWarning(ParsingErrorCode.ValueOutOfRange, "Warning 2")
        };

        // Act
        var (errorDtos, warningDtos) = errors.ToDtos();

        // Assert
        errorDtos.Should().HaveCount(2);
        warningDtos.Should().HaveCount(3);

        errorDtos.Select(e => e.ErrorType).Should().Contain("EmptyInput");
        errorDtos.Select(e => e.ErrorType).Should().Contain("InvalidRepCount");

        warningDtos.Select(w => w.WarningType).Should().Contain("UnknownMovement");
        warningDtos.Select(w => w.WarningType).Should().Contain("DuplicateMovement");
        warningDtos.Select(w => w.WarningType).Should().Contain("ValueOutOfRange");
    }

    [Fact]
    public void ToDtos_PreservesOrder()
    {
        // Arrange
        var errors = new List<ParsingError>
        {
            ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1", lineNumber: 1),
            ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2", lineNumber: 2),
            ParsingError.CreateError(ParsingErrorCode.InvalidRepCount, "Error 3", lineNumber: 3)
        };

        // Act
        var (errorDtos, _) = errors.ToDtos();

        // Assert
        errorDtos[0].LineNumber.Should().Be(1);
        errorDtos[1].LineNumber.Should().Be(2);
        errorDtos[2].LineNumber.Should().Be(3);
    }

    [Fact]
    public void ToDtos_PreservesAllProperties()
    {
        // Arrange - Using CreateWarning since it supports similarNames
        var similarNames = new List<string> { "Clean", "Clean and Jerk" };
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.AmbiguousMovement,
            "'CL' could match multiple movements.",
            lineNumber: 5,
            context: "5 CL @ 135",
            suggestion: "Use full movement name.",
            similarNames: similarNames);

        var errors = new List<ParsingError> { warning };

        // Act
        var (_, warningDtos) = errors.ToDtos();

        // Assert
        var dto = warningDtos.Single();
        dto.WarningType.Should().Be("AmbiguousMovement");
        dto.WarningCode.Should().Be((int)ParsingErrorCode.AmbiguousMovement);
        dto.Message.Should().Be("'CL' could match multiple movements.");
        dto.LineNumber.Should().Be(5);
        dto.OriginalText.Should().Be("5 CL @ 135");
        dto.Suggestion.Should().Be("Use full movement name.");
        dto.SimilarNames.Should().BeEquivalentTo(similarNames);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ToErrorDto_WithEmptyMessage_PreservesEmpty()
    {
        // Arrange
        var error = ParsingError.CreateError(
            ParsingErrorCode.InternalError,
            string.Empty);

        // Act
        var dto = error.ToErrorDto();

        // Assert
        dto.Message.Should().BeEmpty();
    }

    [Fact]
    public void ToWarningDto_WithEmptySimilarNames_PreservesEmpty()
    {
        // Arrange
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Unknown movement",
            similarNames: new List<string>());

        // Act
        var dto = warning.ToWarningDto();

        // Assert
        dto.SimilarNames.Should().BeEmpty();
    }

    [Fact]
    public void ToDtos_LargeCollection_HandlesEfficiently()
    {
        // Arrange
        var errors = Enumerable.Range(1, 1000)
            .Select(i => i % 3 == 0
                ? ParsingError.CreateError(ParsingErrorCode.InvalidRepCount, $"Error {i}", lineNumber: i)
                : ParsingError.CreateWarning(ParsingErrorCode.ValueOutOfRange, $"Warning {i}", lineNumber: i))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var (errorDtos, warningDtos) = errors.ToDtos();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        errorDtos.Count.Should().Be(333); // Every 3rd item
        warningDtos.Count.Should().Be(667); // The rest
    }

    #endregion
}
