using FluentAssertions;
using WodStrat.Services.Parsing;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing;

/// <summary>
/// Unit tests for InputValidator class.
/// Tests cover input validation, sanitization, and error generation.
/// </summary>
public class InputValidatorTests
{
    #region Empty/Null Input Tests

    [Fact]
    public void Validate_NullInput_ReturnsEmptyInputError()
    {
        // Act
        var result = InputValidator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(ParsingErrorCode.EmptyInput);
        result.SanitizedText.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyString_ReturnsEmptyInputError()
    {
        // Act
        var result = InputValidator.Validate("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.EmptyInput);
    }

    [Fact]
    public void Validate_WhitespaceOnly_ReturnsEmptyInputError()
    {
        // Act
        var result = InputValidator.Validate("   \t\n\r  ");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.EmptyInput);
    }

    #endregion

    #region Input Length Tests

    [Fact]
    public void Validate_InputTooLong_ReturnsInputTooLongError()
    {
        // Arrange
        var longInput = new string('a', ParsingErrorMessages.MaxInputLength + 1);

        // Act
        var result = InputValidator.Validate(longInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.InputTooLong);
    }

    [Fact]
    public void Validate_InputExactlyAtMaxLength_IsValid()
    {
        // Arrange - Create valid workout text at max length
        var baseText = "10 Push-ups\n";
        var iterations = (ParsingErrorMessages.MaxInputLength / baseText.Length) + 1;
        var input = string.Concat(Enumerable.Repeat(baseText, iterations));

        // Trim to exact max length
        input = input[..ParsingErrorMessages.MaxInputLength];

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.Errors.Should().NotContain(e => e.Code == ParsingErrorCode.InputTooLong);
    }

    [Fact]
    public void Validate_InputTooShort_ReturnsInputTooShortError()
    {
        // Arrange - Less than MinInputLength (5) characters
        var shortInput = "ab";

        // Act
        var result = InputValidator.Validate(shortInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.InputTooShort);
    }

    [Fact]
    public void Validate_InputExactlyAtMinLength_IsValid()
    {
        // Arrange - Exactly MinInputLength characters with a number
        var input = "5 run";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.Errors.Should().NotContain(e => e.Code == ParsingErrorCode.InputTooShort);
    }

    #endregion

    #region Binary Content Tests

    [Fact]
    public void Validate_BinaryContent_ReturnsBinaryContentError()
    {
        // Arrange - Include null character (binary)
        var binaryInput = "Hello\x00World 10 reps";

        // Act
        var result = InputValidator.Validate(binaryInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.BinaryContent);
    }

    [Theory]
    [InlineData("\x01")] // SOH
    [InlineData("\x02")] // STX
    [InlineData("\x03")] // ETX
    [InlineData("\x04")] // EOT
    [InlineData("\x05")] // ENQ
    [InlineData("\x06")] // ACK
    [InlineData("\x07")] // BEL
    [InlineData("\x08")] // BS
    // \x09 is TAB (allowed)
    // \x0A is LF (allowed)
    [InlineData("\x0B")] // VT
    [InlineData("\x0C")] // FF
    // \x0D is CR (allowed)
    [InlineData("\x0E")] // SO
    [InlineData("\x0F")] // SI
    public void Validate_ControlCharacters_ReturnsBinaryContentError(string controlChar)
    {
        // Arrange
        var input = $"10 Push-ups{controlChar}valid text";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.BinaryContent);
    }

    #endregion

    #region Dangerous Content Tests

    [Fact]
    public void Validate_ScriptTag_ReturnsInvalidCharactersError()
    {
        // Arrange
        var xssInput = "10 Push-ups <script>alert('xss')</script>";

        // Act
        var result = InputValidator.Validate(xssInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.InvalidCharacters);
    }

    [Fact]
    public void Validate_JavascriptProtocol_ReturnsInvalidCharactersError()
    {
        // Arrange
        var jsInput = "10 Push-ups javascript:alert('xss')";

        // Act
        var result = InputValidator.Validate(jsInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.InvalidCharacters);
    }

    [Fact]
    public void Validate_DataProtocol_ReturnsInvalidCharactersError()
    {
        // Arrange
        var dataInput = "10 Push-ups data:text/html,<script>alert('xss')</script>";

        // Act
        var result = InputValidator.Validate(dataInput);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == ParsingErrorCode.InvalidCharacters);
    }

    [Fact]
    public void Validate_OnEventHandler_DoesNotTriggerError()
    {
        // Arrange - "onclick" in text should be OK if not in HTML context
        var input = "10 Push-ups onclick style";

        // Act
        var result = InputValidator.Validate(input);

        // Assert - Only dangerous patterns in specific contexts are blocked
        result.Errors.Should().NotContain(e => e.Code == ParsingErrorCode.InvalidCharacters);
    }

    #endregion

    #region No Numbers Warning Tests

    [Fact]
    public void Validate_NoNumbers_ReturnsNoWorkoutStructureWarning()
    {
        // Arrange - Valid text but no numbers
        var input = "Push-ups, Pull-ups, Squats";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue(); // Warnings don't make it invalid
        result.Warnings.Should().ContainSingle(w => w.Code == ParsingErrorCode.NoWorkoutStructure);
    }

    [Fact]
    public void Validate_WithNumbers_NoWarning()
    {
        // Arrange
        var input = "10 Push-ups, 20 Pull-ups";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.Warnings.Should().NotContain(w => w.Code == ParsingErrorCode.NoWorkoutStructure);
    }

    [Theory]
    [InlineData("10")]
    [InlineData("5 minutes")]
    [InlineData("3 rounds")]
    [InlineData("AMRAP 20")]
    public void Validate_WithAnyNumber_NoWarning(string input)
    {
        // Need to make it long enough (min 5 chars)
        var paddedInput = input.Length < 5 ? input + " run" : input;

        // Act
        var result = InputValidator.Validate(paddedInput);

        // Assert
        result.Warnings.Should().NotContain(w => w.Code == ParsingErrorCode.NoWorkoutStructure);
    }

    #endregion

    #region Sanitization Tests

    [Fact]
    public void Validate_LeadingWhitespace_IsTrimmed()
    {
        // Arrange
        var input = "   10 Push-ups";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.SanitizedText.Should().StartWith("10");
    }

    [Fact]
    public void Validate_TrailingWhitespace_IsTrimmed()
    {
        // Arrange
        var input = "10 Push-ups   ";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.SanitizedText.Should().EndWith("Push-ups");
    }

    [Fact]
    public void Validate_MultipleSpaces_AreNormalized()
    {
        // Arrange
        var input = "10    Push-ups     for     time";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.SanitizedText.Should().NotContain("  "); // No double spaces
    }

    [Fact]
    public void Validate_TabsAndNewlines_AreNormalized()
    {
        // Arrange
        var input = "10\tPush-ups\n\n20 Pull-ups";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.SanitizedText.Should().NotContain("\t");
        result.SanitizedText.Should().NotContain("\n\n");
    }

    #endregion

    #region Valid Input Tests

    [Fact]
    public void Validate_ValidWorkoutText_ReturnsValidResult()
    {
        // Arrange
        var input = @"AMRAP 20:
10 Push-ups
15 Air Squats
20 Sit-ups";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.SanitizedText.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ValidSimpleWorkout_ReturnsValidResult()
    {
        // Arrange
        var input = "21-15-9 Thrusters and Pull-ups";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidWithSpecialCharacters_ReturnsValidResult()
    {
        // Arrange - Common CrossFit notation
        var input = "5 Rounds:\n10 Push-ups @ 45#\n400m Run";

        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("For Time:\n21-15-9\nThrusters (95/65)\nPull-ups")]
    [InlineData("EMOM x 10:\n5 Power Cleans (185/125)")]
    [InlineData("Tabata Hollow Rocks")]
    [InlineData("5x5 Back Squat @ 80%")]
    public void Validate_RealWorldWorkouts_ReturnsValidResult(string input)
    {
        // Act
        var result = InputValidator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region InputValidationResult Tests

    [Fact]
    public void InputValidationResult_Defaults_AreCorrect()
    {
        // Arrange
        var result = new InputValidationResult();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        result.SanitizedText.Should().BeEmpty();
    }

    [Fact]
    public void InputValidationResult_ErrorsAndWarnings_AreReadOnly()
    {
        // Arrange
        var result = InputValidator.Validate("10 Push-ups, 20 Pull-ups");

        // Assert - These should be read-only lists
        result.Errors.Should().BeAssignableTo<IReadOnlyList<ParsingError>>();
        result.Warnings.Should().BeAssignableTo<IReadOnlyList<ParsingError>>();
    }

    #endregion
}
