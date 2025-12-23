using FluentAssertions;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for ParsingResultAggregator class.
/// Tests cover error collection, deduplication, limiting, and summarization.
/// </summary>
public class ParsingResultAggregatorTests
{
    #region Add Tests

    [Fact]
    public void Add_SingleError_AddsToErrorsList()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Empty input");

        // Act
        var result = aggregator.Add(error);

        // Assert
        result.Should().BeTrue();
        aggregator.Errors.Should().HaveCount(1);
        aggregator.Errors.Should().Contain(error);
    }

    [Fact]
    public void Add_SingleWarning_AddsToWarningsList()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var warning = ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Unknown movement");

        // Act
        var result = aggregator.Add(warning);

        // Assert
        result.Should().BeTrue();
        aggregator.Warnings.Should().HaveCount(1);
        aggregator.Warnings.Should().Contain(warning);
    }

    [Fact]
    public void Add_SingleInfo_AddsToInfoList()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var info = ParsingError.CreateInfo(
            ParsingErrorCode.DuplicateMovement,
            "Duplicate movement");

        // Act
        var result = aggregator.Add(info);

        // Assert
        result.Should().BeTrue();
        aggregator.Info.Should().HaveCount(1);
        aggregator.Info.Should().Contain(info);
    }

    [Fact]
    public void Add_MixedSeverities_RoutesToCorrectLists()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error = ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error");
        var warning = ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning");
        var info = ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info");

        // Act
        aggregator.Add(error);
        aggregator.Add(warning);
        aggregator.Add(info);

        // Assert
        aggregator.Errors.Should().HaveCount(1);
        aggregator.Warnings.Should().HaveCount(1);
        aggregator.Info.Should().HaveCount(1);
        aggregator.TotalIssueCount.Should().Be(3);
    }

    #endregion

    #region Deduplication Tests

    [Fact]
    public void Add_DuplicateErrorSameCodeAndLine_Deduplicates()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error1 = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: 5);
        var error2 = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count (duplicate)",
            lineNumber: 5);

        // Act
        var result1 = aggregator.Add(error1);
        var result2 = aggregator.Add(error2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        aggregator.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Add_SameCodeDifferentLines_DoesNotDeduplicate()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error1 = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: 5);
        var error2 = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: 10);

        // Act
        aggregator.Add(error1);
        aggregator.Add(error2);

        // Assert
        aggregator.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Add_SameCodeNoLineNumber_DeduplicatesByContext()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error1 = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Empty input",
            context: "same context");
        var error2 = ParsingError.CreateError(
            ParsingErrorCode.EmptyInput,
            "Empty input again",
            context: "same context");

        // Act
        var result1 = aggregator.Add(error1);
        var result2 = aggregator.Add(error2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        aggregator.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Add_DifferentCodesOnSameLine_DoesNotDeduplicate()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error1 = ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Invalid rep count",
            lineNumber: 5);
        var error2 = ParsingError.CreateError(
            ParsingErrorCode.UnknownMovement,
            "Unknown movement",
            lineNumber: 5);

        // Act
        aggregator.Add(error1);
        aggregator.Add(error2);

        // Assert
        aggregator.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Error Limit Tests

    [Fact]
    public void Add_AtMaxErrors_RejectsAdditionalErrors()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator { MaxErrors = 5 };

        // Add 5 errors (at limit)
        for (int i = 0; i < 5; i++)
        {
            aggregator.Add(ParsingError.CreateError(
                ParsingErrorCode.InvalidRepCount,
                $"Error {i}",
                lineNumber: i));
        }

        // Act - Try to add 6th error
        var result = aggregator.Add(ParsingError.CreateError(
            ParsingErrorCode.InvalidRepCount,
            "Error 6",
            lineNumber: 99));

        // Assert
        result.Should().BeFalse();
        aggregator.Errors.Should().HaveCount(5);
        aggregator.ErrorLimitReached.Should().BeTrue();
    }

    [Fact]
    public void Add_AtMaxErrors_StillAcceptsWarnings()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator { MaxErrors = 2 };

        // Fill up errors
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2"));

        // Act - Add warning when error limit reached
        var result = aggregator.Add(ParsingError.CreateWarning(
            ParsingErrorCode.UnknownMovement,
            "Warning"));

        // Assert
        result.Should().BeTrue();
        aggregator.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void ErrorLimitReached_InitiallyFalse()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();

        // Assert
        aggregator.ErrorLimitReached.Should().BeFalse();
    }

    [Fact]
    public void MaxErrors_DefaultsToParsingErrorMessagesMaxErrorCount()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();

        // Assert
        aggregator.MaxErrors.Should().Be(ParsingErrorMessages.MaxErrorCount);
        aggregator.MaxErrors.Should().Be(20);
    }

    #endregion

    #region AddRange Tests

    [Fact]
    public void AddRange_MultipleErrors_AddsAllThatFit()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator { MaxErrors = 3 };
        var errors = new List<ParsingError>
        {
            ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"),
            ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2"),
            ParsingError.CreateError(ParsingErrorCode.NoMovementsDetected, "Error 3"),
            ParsingError.CreateError(ParsingErrorCode.InvalidWorkoutType, "Error 4") // Should be rejected
        };

        // Act
        aggregator.AddRange(errors);

        // Assert
        aggregator.Errors.Should().HaveCount(3);
        aggregator.ErrorLimitReached.Should().BeTrue();
    }

    [Fact]
    public void AddRange_EmptyCollection_DoesNothing()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();

        // Act
        aggregator.AddRange(Array.Empty<ParsingError>());

        // Assert
        aggregator.TotalIssueCount.Should().Be(0);
    }

    #endregion

    #region GetSortedIssues Tests

    [Fact]
    public void GetSortedIssues_ReturnsErrorsFirst()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning"));
        aggregator.Add(ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error"));

        // Act
        var sorted = aggregator.GetSortedIssues();

        // Assert
        sorted.Should().HaveCount(3);
        sorted[0].Severity.Should().Be(ParsingErrorSeverity.Error);
        sorted[1].Severity.Should().Be(ParsingErrorSeverity.Warning);
        sorted[2].Severity.Should().Be(ParsingErrorSeverity.Info);
    }

    [Fact]
    public void GetSortedIssues_EmptyAggregator_ReturnsEmptyList()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();

        // Act
        var sorted = aggregator.GetSortedIssues();

        // Assert
        sorted.Should().BeEmpty();
    }

    #endregion

    #region GetSummary Tests

    [Fact]
    public void GetSummary_ReturnsCorrectCounts()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2"));
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning 1"));
        aggregator.Add(ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info 1"));
        aggregator.Add(ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info 2", lineNumber: 2));

        // Act
        var summary = aggregator.GetSummary();

        // Assert
        summary.ErrorCount.Should().Be(2);
        summary.WarningCount.Should().Be(1);
        summary.InfoCount.Should().Be(2);
        summary.TotalIssueCount.Should().Be(5);
    }

    [Fact]
    public void GetSummary_GroupsErrorsByCode()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InvalidRepCount, "Error", lineNumber: 1));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InvalidRepCount, "Error", lineNumber: 2));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InvalidWeight, "Error", lineNumber: 3));

        // Act
        var summary = aggregator.GetSummary();

        // Assert
        summary.ErrorsByCode.Should().ContainKey(ParsingErrorCode.InvalidRepCount);
        summary.ErrorsByCode[ParsingErrorCode.InvalidRepCount].Should().Be(2);
        summary.ErrorsByCode.Should().ContainKey(ParsingErrorCode.InvalidWeight);
        summary.ErrorsByCode[ParsingErrorCode.InvalidWeight].Should().Be(1);
    }

    [Fact]
    public void GetSummary_GroupsWarningsByCode()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning", lineNumber: 1));
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning", lineNumber: 2));
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.AmbiguousWorkoutType, "Warning"));

        // Act
        var summary = aggregator.GetSummary();

        // Assert
        summary.WarningsByCode.Should().ContainKey(ParsingErrorCode.UnknownMovement);
        summary.WarningsByCode[ParsingErrorCode.UnknownMovement].Should().Be(2);
        summary.WarningsByCode.Should().ContainKey(ParsingErrorCode.AmbiguousWorkoutType);
        summary.WarningsByCode[ParsingErrorCode.AmbiguousWorkoutType].Should().Be(1);
    }

    [Fact]
    public void GetSummary_ErrorLimitReached_ReflectedInSummary()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator { MaxErrors = 2 };
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error 1"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.NoMovementsDetected, "Error 3")); // Rejected

        // Act
        var summary = aggregator.GetSummary();

        // Assert
        summary.ErrorLimitReached.Should().BeTrue();
        summary.ErrorCount.Should().Be(2);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_ResetsAllCollections()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error"));
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning"));
        aggregator.Add(ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info"));

        // Act
        aggregator.Clear();

        // Assert
        aggregator.Errors.Should().BeEmpty();
        aggregator.Warnings.Should().BeEmpty();
        aggregator.Info.Should().BeEmpty();
        aggregator.TotalIssueCount.Should().Be(0);
        aggregator.ErrorLimitReached.Should().BeFalse();
    }

    [Fact]
    public void Clear_AllowsAddingPreviouslyDeduplicatedErrors()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();
        var error = ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error");
        aggregator.Add(error);
        aggregator.Add(error); // Deduplicated

        // Act
        aggregator.Clear();
        var result = aggregator.Add(error);

        // Assert
        result.Should().BeTrue();
        aggregator.Errors.Should().HaveCount(1);
    }

    #endregion

    #region TotalIssueCount Tests

    [Fact]
    public void TotalIssueCount_IncludesAllSeverities()
    {
        // Arrange
        var aggregator = new ParsingResultAggregator();

        // Act
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.EmptyInput, "Error"));
        aggregator.Add(ParsingError.CreateError(ParsingErrorCode.InputTooLong, "Error 2"));
        aggregator.Add(ParsingError.CreateWarning(ParsingErrorCode.UnknownMovement, "Warning"));
        aggregator.Add(ParsingError.CreateInfo(ParsingErrorCode.DuplicateMovement, "Info"));

        // Assert
        aggregator.TotalIssueCount.Should().Be(4);
        aggregator.Errors.Count.Should().Be(2);
        aggregator.Warnings.Count.Should().Be(1);
        aggregator.Info.Count.Should().Be(1);
    }

    #endregion
}
