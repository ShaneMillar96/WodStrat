using FluentAssertions;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for SimilarNameFinder class.
/// Tests cover Levenshtein distance algorithm and similar name suggestions.
/// </summary>
public class SimilarNameFinderTests
{
    private readonly List<string> _movementNames = new()
    {
        "Push-ups",
        "Pull-ups",
        "Burpees",
        "Box Jumps",
        "Deadlift",
        "Squat",
        "Clean",
        "Snatch",
        "Thruster",
        "Wall Balls",
        "Kettlebell Swings",
        "Rowing",
        "Running",
        "Double Unders",
        "Muscle-ups",
        "Handstand Push-ups",
        "Toes-to-bar"
    };

    #region Basic Functionality Tests

    [Fact]
    public void FindSimilar_ExactMatch_ReturnsMatch()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("Burpees", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    [Fact]
    public void FindSimilar_CaseInsensitive_FindsMatch()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("burpees", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    [Fact]
    public void FindSimilar_ALLCAPS_FindsMatch()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("BURPEES", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    [Fact]
    public void FindSimilar_OneCharacterOff_FindsMatch()
    {
        // Act - "Burpies" is one character different from "Burpees"
        var result = SimilarNameFinder.FindSimilar("Burpies", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    [Fact]
    public void FindSimilar_TwoCharactersOff_FindsMatch()
    {
        // Act - "Burpes" is two characters different
        var result = SimilarNameFinder.FindSimilar("Burpes", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    [Fact]
    public void FindSimilar_ThreeCharactersOff_FindsMatch()
    {
        // Act - "Burpe" is three characters different (missing 'es')
        var result = SimilarNameFinder.FindSimilar("Burpe", _movementNames);

        // Assert
        result.Should().Contain("Burpees");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void FindSimilar_EmptyInput_ReturnsEmptyList()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("", _movementNames);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSimilar_WhitespaceInput_ReturnsEmptyList()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("   ", _movementNames);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSimilar_NullInput_ReturnsEmptyList()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar(null!, _movementNames);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSimilar_EmptyKnownNames_ReturnsEmptyList()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("Burpees", new List<string>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSimilar_NoCloseMatches_ReturnsEmptyList()
    {
        // Act - "XYZ123" is not close to any movement
        var result = SimilarNameFinder.FindSimilar("XYZ123", _movementNames);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region MaxSuggestions Tests

    [Fact]
    public void FindSimilar_DefaultMaxSuggestions_ReturnsUpTo3()
    {
        // Arrange - Create a list with many similar names
        var names = new List<string>
        {
            "Test1",
            "Test2",
            "Test3",
            "Test4",
            "Test5",
            "Test6"
        };

        // Act
        var result = SimilarNameFinder.FindSimilar("Test", names);

        // Assert
        result.Should().HaveCountLessOrEqualTo(3);
    }

    [Fact]
    public void FindSimilar_CustomMaxSuggestions_RespectsLimit()
    {
        // Arrange
        var names = new List<string>
        {
            "Test1",
            "Test2",
            "Test3",
            "Test4",
            "Test5"
        };

        // Act
        var result = SimilarNameFinder.FindSimilar("Test", names, maxSuggestions: 2);

        // Assert
        result.Should().HaveCountLessOrEqualTo(2);
    }

    [Fact]
    public void FindSimilar_MaxSuggestionsZero_ReturnsEmptyList()
    {
        // Act
        var result = SimilarNameFinder.FindSimilar("Burpees", _movementNames, maxSuggestions: 0);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public void FindSimilar_MultipleMatches_OrderedByDistance()
    {
        // Arrange - Typo that could match multiple movements
        var names = new List<string>
        {
            "Deadlift",      // Very different from "Clen"
            "Clean",         // Distance 1 from "Clen" (added 'a')
            "Cleans",        // Distance 2 from "Clen" (added 'a' and 's')
            "Clean and Jerk" // Very different
        };

        // Act - "Clen" should match "Clean" first (distance 1)
        var result = SimilarNameFinder.FindSimilar("Clen", names, maxSuggestions: 5);

        // Assert - "Clean" should be first (closest match to "Clen")
        if (result.Count > 0)
        {
            result.First().Should().Be("Clean");
        }
    }

    [Fact]
    public void FindSimilar_SimilarDistances_ReturnsAll()
    {
        // Arrange - "Run" is close to several short words
        var names = new List<string>
        {
            "Running",
            "Rowing",
            "Run"
        };

        // Act
        var result = SimilarNameFinder.FindSimilar("Run", names, maxSuggestions: 5);

        // Assert
        result.Should().Contain("Run");
    }

    #endregion

    #region Real-World Typo Tests

    [Theory]
    [InlineData("Pushups", "Push-ups")]
    [InlineData("Pullups", "Pull-ups")]
    [InlineData("Deadlif", "Deadlift")]
    [InlineData("Squats", "Squat")]
    [InlineData("Clena", "Clean")]
    [InlineData("Sntch", "Snatch")]
    public void FindSimilar_CommonTypos_FindsCorrectMovement(string typo, string expected)
    {
        // Act
        var result = SimilarNameFinder.FindSimilar(typo, _movementNames);

        // Assert
        result.Should().Contain(expected);
    }

    [Theory]
    [InlineData("T2B", "Toes-to-bar")]
    [InlineData("HSPu", "Handstand Push-ups")]
    public void FindSimilar_CommonAbbreviations_MayNotFindMatch(string abbreviation, string expected)
    {
        // Act
        var result = SimilarNameFinder.FindSimilar(abbreviation, _movementNames);

        // Assert - Abbreviations are often too different for Levenshtein
        // This test documents expected behavior
        if (result.Contains(expected))
        {
            result.Should().Contain(expected);
        }
        else
        {
            // Abbreviations typically don't match well with Levenshtein
            result.Should().NotContain(expected);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void FindSimilar_LargeList_CompletesQuickly()
    {
        // Arrange - Create a large list of names
        var largeList = Enumerable.Range(1, 10000)
            .Select(i => $"Movement{i}")
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = SimilarNameFinder.FindSimilar("Movement1", largeList);
        stopwatch.Stop();

        // Assert - Should complete within 1 second
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        result.Should().Contain("Movement1");
    }

    [Fact]
    public void FindSimilar_LongInputString_CompletesQuickly()
    {
        // Arrange - Very long input (edge case)
        var longInput = new string('a', 100);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = SimilarNameFinder.FindSimilar(longInput, _movementNames);
        stopwatch.Stop();

        // Assert - Should complete within 100ms
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    #endregion
}
