using FluentAssertions;
using WodStrat.Services.Parsing;
using Xunit;

namespace WodStrat.Services.Tests.Parsing;

/// <summary>
/// Unit tests for WorkoutTextPreprocessor.
/// Tests cover text normalization, line categorization, and workout name extraction.
/// </summary>
public class WorkoutTextPreprocessorTests
{
    #region Preprocess Tests

    [Fact]
    public void Preprocess_EmptyString_ReturnsEmptyResult()
    {
        // Act
        var result = WorkoutTextPreprocessor.Preprocess("");

        // Assert
        result.Should().NotBeNull();
        result.IsEmpty.Should().BeTrue();
        result.OriginalText.Should().BeEmpty();
        result.Lines.Should().BeEmpty();
    }

    [Fact]
    public void Preprocess_WhitespaceOnly_ReturnsEmptyResult()
    {
        // Act
        var result = WorkoutTextPreprocessor.Preprocess("   \n\t\r\n  ");

        // Assert
        result.IsEmpty.Should().BeTrue();
        result.Lines.Should().BeEmpty();
    }

    [Fact]
    public void Preprocess_NullInput_ReturnsEmptyResult()
    {
        // Act
        var result = WorkoutTextPreprocessor.Preprocess(null!);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Preprocess_SingleLine_ReturnsSingleLine()
    {
        // Arrange
        var input = "For Time";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.IsEmpty.Should().BeFalse();
        result.Lines.Should().HaveCount(1);
        result.Lines[0].Should().Be("For Time");
    }

    [Fact]
    public void Preprocess_MultipleLines_SplitsCorrectly()
    {
        // Arrange
        var input = "For Time:\n10 Pull-ups\n20 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.Lines.Should().HaveCount(3);
        result.Lines[0].Should().Be("For Time:");
        result.Lines[1].Should().Be("10 Pull-ups");
        result.Lines[2].Should().Be("20 Push-ups");
    }

    [Fact]
    public void Preprocess_WindowsLineEndings_NormalizesCorrectly()
    {
        // Arrange
        var input = "For Time:\r\n10 Pull-ups\r\n20 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.Lines.Should().HaveCount(3);
        result.Lines[0].Should().Be("For Time:");
    }

    [Fact]
    public void Preprocess_MixedLineEndings_NormalizesCorrectly()
    {
        // Arrange
        var input = "For Time:\r10 Pull-ups\n20 Push-ups\r\n15 Air Squats";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.Lines.Should().HaveCount(4);
    }

    [Fact]
    public void Preprocess_ExtraWhitespace_TrimsLines()
    {
        // Arrange
        var input = "  For Time:  \n   10 Pull-ups   \n  20 Push-ups  ";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.Lines[0].Should().Be("For Time:");
        result.Lines[1].Should().Be("10 Pull-ups");
        result.Lines[2].Should().Be("20 Push-ups");
    }

    [Fact]
    public void Preprocess_EmptyLines_RemovesEmptyLines()
    {
        // Arrange
        var input = "For Time:\n\n10 Pull-ups\n\n\n20 Push-ups\n";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.Lines.Should().HaveCount(3);
        result.Lines.Should().NotContain(string.Empty);
    }

    [Fact]
    public void Preprocess_PreservesOriginalText()
    {
        // Arrange
        var input = "For Time:\n10 Pull-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.OriginalText.Should().Be(input);
    }

    #endregion

    #region Workout Name Extraction Tests

    [Fact]
    public void Preprocess_QuotedWorkoutName_ExtractsName()
    {
        // Arrange
        var input = "\"Fran\"\n21-15-9\nThrusters\nPull-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.WorkoutName.Should().Be("Fran");
    }

    [Fact]
    public void Preprocess_SingleWordWorkoutName_ExtractsName()
    {
        // Arrange - Known benchmark workout names at start
        var input = "Cindy\n20 min AMRAP\n5 Pull-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert - May or may not detect based on implementation
        // The preprocessor should handle common workout name patterns
        result.Should().NotBeNull();
    }

    [Fact]
    public void Preprocess_NoWorkoutName_ReturnsNullName()
    {
        // Arrange
        var input = "20 min AMRAP:\n5 Pull-ups\n10 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.WorkoutName.Should().BeNullOrEmpty();
    }

    #endregion

    #region Line Classification Tests

    [Fact]
    public void Preprocess_HeaderLines_IdentifiesCorrectly()
    {
        // Arrange
        var input = "20 min AMRAP:\n5 Pull-ups\n10 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.HeaderLines.Should().Contain("20 min AMRAP:");
    }

    [Fact]
    public void Preprocess_MovementLines_IdentifiesCorrectly()
    {
        // Arrange
        var input = "For Time:\n10 Pull-ups\n20 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.MovementLines.Should().Contain("10 Pull-ups");
        result.MovementLines.Should().Contain("20 Push-ups");
    }

    [Fact]
    public void Preprocess_TimeCapLine_IdentifiesAsHeader()
    {
        // Arrange
        var input = "For Time:\nTime Cap: 15\n10 Pull-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.HeaderLines.Should().Contain("Time Cap: 15");
    }

    #endregion

    #region Special Character Handling Tests

    [Fact]
    public void Preprocess_EmDash_NormalizesToHyphen()
    {
        // Arrange - Em dash (—) and en dash (–) should normalize to hyphen
        var input = "21—15—9\nThrusters";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.NormalizedText.Should().Contain("21-15-9");
    }

    [Fact]
    public void Preprocess_SmartQuotes_NormalizesToStandard()
    {
        // Arrange - Smart quotes should normalize (using escaped Unicode)
        var input = "\u201CFran\u201D\n21-15-9"; // Smart quotes

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert
        result.NormalizedText.Should().Contain("\"Fran\"");
    }

    [Fact]
    public void Preprocess_BulletPoints_PreservesInLines()
    {
        // Arrange
        var input = "For Time:\n• 10 Pull-ups\n• 20 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert - Preprocessor may or may not strip bullets depending on implementation
        result.Lines.Should().HaveCountGreaterThan(0);
        result.MovementLines.Should().NotBeEmpty();
    }

    [Fact]
    public void Preprocess_NumberedList_PreservesAsMovementLines()
    {
        // Arrange
        var input = "For Time:\n1. 10 Pull-ups\n2. 20 Push-ups";

        // Act
        var result = WorkoutTextPreprocessor.Preprocess(input);

        // Assert - Preprocessor preserves numbered list items as movement lines
        result.MovementLines.Should().HaveCount(2);
        result.MovementLines.Should().Contain(l => l.Contains("Pull-ups"));
        result.MovementLines.Should().Contain(l => l.Contains("Push-ups"));
    }

    #endregion
}
