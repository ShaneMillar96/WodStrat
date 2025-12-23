using FluentAssertions;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for ParsingErrorMessages static class.
/// Tests cover message retrieval, interpolation, and suggestion generation.
/// </summary>
public class ParsingErrorMessagesTests
{
    #region Constant Tests

    [Fact]
    public void MaxInputLength_ShouldBe10000()
    {
        // Assert
        ParsingErrorMessages.MaxInputLength.Should().Be(10000);
    }

    [Fact]
    public void MinInputLength_ShouldBe5()
    {
        // Assert
        ParsingErrorMessages.MinInputLength.Should().Be(5);
    }

    [Fact]
    public void MaxErrorCount_ShouldBe20()
    {
        // Assert
        ParsingErrorMessages.MaxErrorCount.Should().Be(20);
    }

    [Fact]
    public void SimilarNameSuggestionCount_ShouldBe3()
    {
        // Assert
        ParsingErrorMessages.SimilarNameSuggestionCount.Should().Be(3);
    }

    #endregion

    #region GetMessage Tests

    [Fact]
    public void GetMessage_EmptyInput_ReturnsExpectedMessage()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(ParsingErrorCode.EmptyInput);

        // Assert
        message.Should().Be("Workout text cannot be empty.");
    }

    [Fact]
    public void GetMessage_InputTooLong_InterpolatesMaxLength()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(
            ParsingErrorCode.InputTooLong,
            ParsingErrorMessages.MaxInputLength);

        // Assert
        message.Should().Contain("10,000");
        message.Should().Contain("exceeds maximum length");
    }

    [Fact]
    public void GetMessage_UnknownMovement_InterpolatesMovementName()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(
            ParsingErrorCode.UnknownMovement,
            "burpies");

        // Assert
        message.Should().Be("Movement 'burpies' not recognized.");
    }

    [Fact]
    public void GetMessage_InvalidWorkoutType_InterpolatesTypeName()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(
            ParsingErrorCode.InvalidWorkoutType,
            "SUPERAMRAP");

        // Assert
        message.Should().Be("'SUPERAMRAP' is not a recognized workout type.");
    }

    [Fact]
    public void GetMessage_AmbiguousMovement_InterpolatesMultipleValues()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(
            ParsingErrorCode.AmbiguousMovement,
            "DL",
            "Deadlift, Dumbbell Lunge");

        // Assert
        message.Should().Be("'DL' could match multiple movements: Deadlift, Dumbbell Lunge.");
    }

    [Fact]
    public void GetMessage_UnknownErrorCode_ReturnsGenericMessage()
    {
        // Act
        var message = ParsingErrorMessages.GetMessage((ParsingErrorCode)9999);

        // Assert
        message.Should().Contain("Unknown error");
        message.Should().Contain("9999");
    }

    [Fact]
    public void GetMessage_WithNoArgsWhenArgsExpected_ReturnsTemplateWithoutInterpolation()
    {
        // Act - UnknownMovement expects {0} but we don't provide it
        var message = ParsingErrorMessages.GetMessage(ParsingErrorCode.UnknownMovement);

        // Assert - Should return template as-is (with {0} placeholder)
        message.Should().Contain("Movement '{0}' not recognized.");
    }

    [Fact]
    public void GetMessage_WithWrongNumberOfArgs_HandlesGracefully()
    {
        // Act - Provide extra args that aren't expected
        var message = ParsingErrorMessages.GetMessage(
            ParsingErrorCode.EmptyInput,
            "extra", "args", "here");

        // Assert - Should still return the message (ignores extra args)
        message.Should().Be("Workout text cannot be empty.");
    }

    #endregion

    #region GetSuggestion Tests

    [Fact]
    public void GetSuggestion_EmptyInput_ReturnsHelpfulSuggestion()
    {
        // Act
        var suggestion = ParsingErrorMessages.GetSuggestion(ParsingErrorCode.EmptyInput);

        // Assert
        suggestion.Should().NotBeNullOrEmpty();
        suggestion.Should().Contain("Enter a workout description");
    }

    [Fact]
    public void GetSuggestion_UnknownMovement_ReturnsHelpfulSuggestion()
    {
        // Act
        var suggestion = ParsingErrorMessages.GetSuggestion(ParsingErrorCode.UnknownMovement);

        // Assert
        suggestion.Should().NotBeNullOrEmpty();
        suggestion.Should().Contain("Check spelling");
    }

    [Fact]
    public void GetSuggestion_NoMovementsDetected_ReturnsHelpfulSuggestion()
    {
        // Act
        var suggestion = ParsingErrorMessages.GetSuggestion(ParsingErrorCode.NoMovementsDetected);

        // Assert
        suggestion.Should().NotBeNullOrEmpty();
        suggestion.Should().Contain("movements with quantities");
    }

    [Fact]
    public void GetSuggestion_InvalidWeight_ReturnsFormatExample()
    {
        // Act
        var suggestion = ParsingErrorMessages.GetSuggestion(ParsingErrorCode.InvalidWeight);

        // Assert
        suggestion.Should().NotBeNullOrEmpty();
        suggestion.Should().ContainAny("lbs", "kg", "format");
    }

    [Fact]
    public void GetSuggestion_UnknownErrorCode_ReturnsEmptyString()
    {
        // Act
        var suggestion = ParsingErrorMessages.GetSuggestion((ParsingErrorCode)9999);

        // Assert
        suggestion.Should().BeEmpty();
    }

    #endregion

    #region Create Factory Tests

    [Fact]
    public void Create_EmptyInput_CreatesCompleteParsingError()
    {
        // Act
        var error = ParsingErrorMessages.Create(
            ParsingErrorCode.EmptyInput,
            ParsingErrorSeverity.Error);

        // Assert
        error.Code.Should().Be(ParsingErrorCode.EmptyInput);
        error.Severity.Should().Be(ParsingErrorSeverity.Error);
        error.Message.Should().Be("Workout text cannot be empty.");
        error.Suggestion.Should().NotBeNullOrEmpty();
        error.LineNumber.Should().BeNull();
        error.Context.Should().BeNull();
        error.SimilarNames.Should().BeNull();
    }

    [Fact]
    public void Create_WithLineNumber_SetsLineNumber()
    {
        // Act
        var error = ParsingErrorMessages.Create(
            ParsingErrorCode.InvalidRepCount,
            ParsingErrorSeverity.Warning,
            lineNumber: 5,
            messageArgs: "-5");

        // Assert
        error.LineNumber.Should().Be(5);
        error.Message.Should().Contain("-5");
    }

    [Fact]
    public void Create_WithContext_SetsContext()
    {
        // Act
        var error = ParsingErrorMessages.Create(
            ParsingErrorCode.UnknownMovement,
            ParsingErrorSeverity.Warning,
            context: "10 burpies",
            messageArgs: "burpies");

        // Assert
        error.Context.Should().Be("10 burpies");
    }

    [Fact]
    public void Create_WithSimilarNames_SetsSimilarNames()
    {
        // Arrange
        var similarNames = new List<string> { "Burpees", "Bar-facing Burpees" };

        // Act
        var error = ParsingErrorMessages.Create(
            ParsingErrorCode.UnknownMovement,
            ParsingErrorSeverity.Warning,
            similarNames: similarNames,
            messageArgs: "burpies");

        // Assert
        error.SimilarNames.Should().NotBeNull();
        error.SimilarNames.Should().BeEquivalentTo(similarNames);
    }

    [Fact]
    public void Create_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var similarNames = new List<string> { "Deadlift" };

        // Act
        var error = ParsingErrorMessages.Create(
            ParsingErrorCode.AmbiguousMovement,
            ParsingErrorSeverity.Warning,
            lineNumber: 3,
            context: "5 DL @ 315",
            similarNames: similarNames,
            messageArgs: new object[] { "DL", "Deadlift, Dumbbell Lunge" });

        // Assert
        error.Code.Should().Be(ParsingErrorCode.AmbiguousMovement);
        error.Severity.Should().Be(ParsingErrorSeverity.Warning);
        error.Message.Should().Contain("DL");
        error.Message.Should().Contain("Deadlift, Dumbbell Lunge");
        error.Suggestion.Should().NotBeNullOrEmpty();
        error.LineNumber.Should().Be(3);
        error.Context.Should().Be("5 DL @ 315");
        error.SimilarNames.Should().Contain("Deadlift");
    }

    #endregion

    #region Coverage Tests for All Error Codes

    [Theory]
    [InlineData(ParsingErrorCode.EmptyInput)]
    [InlineData(ParsingErrorCode.InputTooLong)]
    [InlineData(ParsingErrorCode.InputTooShort)]
    [InlineData(ParsingErrorCode.BinaryContent)]
    [InlineData(ParsingErrorCode.InvalidCharacters)]
    [InlineData(ParsingErrorCode.NoWorkoutStructure)]
    [InlineData(ParsingErrorCode.NoMovementsDetected)]
    [InlineData(ParsingErrorCode.InvalidWorkoutType)]
    [InlineData(ParsingErrorCode.AmbiguousWorkoutType)]
    [InlineData(ParsingErrorCode.MissingDuration)]
    [InlineData(ParsingErrorCode.MissingRoundCount)]
    [InlineData(ParsingErrorCode.ContradictoryMetadata)]
    [InlineData(ParsingErrorCode.UnknownMovement)]
    [InlineData(ParsingErrorCode.AmbiguousMovement)]
    [InlineData(ParsingErrorCode.InvalidRepCount)]
    [InlineData(ParsingErrorCode.InvalidWeight)]
    [InlineData(ParsingErrorCode.InvalidDistance)]
    [InlineData(ParsingErrorCode.InvalidTime)]
    [InlineData(ParsingErrorCode.InvalidCalories)]
    [InlineData(ParsingErrorCode.EmptyMovementLine)]
    [InlineData(ParsingErrorCode.UnrecognizedMovementFormat)]
    [InlineData(ParsingErrorCode.DuplicateMovement)]
    [InlineData(ParsingErrorCode.InconsistentUnits)]
    [InlineData(ParsingErrorCode.ValueOutOfRange)]
    [InlineData(ParsingErrorCode.InternalError)]
    [InlineData(ParsingErrorCode.Timeout)]
    public void AllErrorCodes_ShouldHaveMessageAndSuggestion(ParsingErrorCode code)
    {
        // Act
        var message = ParsingErrorMessages.GetMessage(code);
        var suggestion = ParsingErrorMessages.GetSuggestion(code);

        // Assert
        message.Should().NotBeNullOrWhiteSpace(
            $"Error code {code} should have a message template");
        suggestion.Should().NotBeNullOrWhiteSpace(
            $"Error code {code} should have a suggestion");
    }

    #endregion
}
