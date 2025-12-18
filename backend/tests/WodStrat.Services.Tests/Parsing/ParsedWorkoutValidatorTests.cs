using FluentAssertions;
using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;
using WodStrat.Services.Parsing;
using Xunit;

namespace WodStrat.Services.Tests.Parsing;

/// <summary>
/// Unit tests for ParsedWorkoutValidator.
/// Tests cover validation logic and confidence score calculation.
/// </summary>
public class ParsedWorkoutValidatorTests
{
    #region QuickValidate Tests

    [Fact]
    public void QuickValidate_EmptyString_ReturnsEmptyInputError()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate("");

        // Assert
        errors.Should().HaveCount(1);
        errors[0].ErrorType.Should().Be("EmptyInput");
        errors[0].Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void QuickValidate_WhitespaceOnly_ReturnsEmptyInputError()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate("   \t\n  ");

        // Assert
        errors.Should().HaveCount(1);
        errors[0].ErrorType.Should().Be("EmptyInput");
    }

    [Fact]
    public void QuickValidate_NullInput_ReturnsEmptyInputError()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate(null!);

        // Assert
        errors.Should().HaveCount(1);
        errors[0].ErrorType.Should().Be("EmptyInput");
    }

    [Fact]
    public void QuickValidate_TooShort_ReturnsTooShortError()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate("abc");

        // Assert
        errors.Should().Contain(e => e.ErrorType == "TooShort");
    }

    [Fact]
    public void QuickValidate_NoNumbers_ReturnsNoNumbersError()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate("Pull-ups and Push-ups");

        // Assert
        errors.Should().Contain(e => e.ErrorType == "NoNumbers");
        errors.First(e => e.ErrorType == "NoNumbers").Message.Should().Contain("number");
    }

    [Fact]
    public void QuickValidate_ValidWorkout_ReturnsEmptyErrors()
    {
        // Act
        var errors = ParsedWorkoutValidator.QuickValidate("10 Pull-ups\n20 Push-ups");

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void QuickValidate_MinimalValid_ReturnsEmptyErrors()
    {
        // Act - Minimal valid workout text (has numbers, sufficient length)
        var errors = ParsedWorkoutValidator.QuickValidate("10 reps");

        // Assert
        errors.Should().BeEmpty();
    }

    #endregion

    #region Validate Tests - Basic Scenarios

    [Fact]
    public void Validate_ValidWorkout_ReturnsSuccessResult()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var typeDetection = CreateSuccessfulTypeDetection();
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ConfidenceScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validate_NoMovements_ReturnsError()
    {
        // Arrange
        var workout = CreateWorkoutWithNoMovements();
        var typeDetection = CreateSuccessfulTypeDetection();
        var movementResults = new List<MovementParseResult>();

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorType == "NoMovements");
    }

    [Fact]
    public void Validate_WithTypeDetectionError_IncludesError()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var typeDetection = CreateTypeDetectionWithError();
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorType == "TypeDetectionFailed");
    }

    [Fact]
    public void Validate_WithTypeDetectionWarning_IncludesWarning()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var typeDetection = CreateTypeDetectionWithWarning();
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Warnings.Should().NotBeEmpty();
        result.Success.Should().BeTrue(); // Warnings don't block success
    }

    #endregion

    #region Time Domain Validation Tests

    [Fact]
    public void Validate_AmrapWithoutTimeCap_AddsWarning()
    {
        // Arrange
        var workout = CreateAmrapWorkoutWithoutTimeCap();
        var typeDetection = CreateTypeDetection(WorkoutType.Amrap);
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Warnings.Should().Contain(w => w.WarningType == "MissingTimeCap");
    }

    [Fact]
    public void Validate_AmrapWithTimeCap_NoWarning()
    {
        // Arrange
        var workout = CreateAmrapWorkoutWithTimeCap();
        var typeDetection = CreateTypeDetection(WorkoutType.Amrap);
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Warnings.Should().NotContain(w => w.WarningType == "MissingTimeCap");
    }

    [Fact]
    public void Validate_EmomWithoutInterval_AddsWarning()
    {
        // Arrange
        var workout = CreateEmomWorkoutWithoutInterval();
        var typeDetection = CreateTypeDetection(WorkoutType.Emom);
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Warnings.Should().Contain(w => w.WarningType == "MissingInterval");
    }

    [Fact]
    public void Validate_RoundsWithoutRoundCount_AddsWarning()
    {
        // Arrange
        var workout = CreateRoundsWorkoutWithoutRoundCount();
        var typeDetection = CreateTypeDetection(WorkoutType.Rounds);
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.Warnings.Should().Contain(w => w.WarningType == "MissingRounds");
    }

    #endregion

    #region Confidence Scoring Tests

    [Fact]
    public void Validate_PerfectParse_ReturnsHighConfidence()
    {
        // Arrange - All movements identified, clear workout type
        var workout = CreateValidWorkout();
        var typeDetection = CreateTypeDetection(WorkoutType.ForTime, 100);
        var movementResults = CreateSuccessfulMovementResults(3, confidence: 100);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.ConfidenceScore.Should().BeGreaterOrEqualTo(90);
        result.ConfidenceLevel.Should().BeOneOf("Perfect", "High");
    }

    [Fact]
    public void Validate_WithErrors_ReturnsLowConfidence()
    {
        // Arrange
        var workout = CreateWorkoutWithNoMovements();
        var typeDetection = CreateTypeDetectionWithError();
        var movementResults = new List<MovementParseResult>();

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.ConfidenceScore.Should().BeLessThan(50);
        result.ConfidenceLevel.Should().Be("Low");
    }

    [Fact]
    public void Validate_ConfidenceBreakdown_IncludesAllFactors()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var typeDetection = CreateTypeDetection(WorkoutType.ForTime, 95);
        var movementResults = CreateSuccessfulMovementResults(2, confidence: 90);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.ConfidenceDetails.Should().NotBeNull();
        result.ConfidenceDetails!.WorkoutTypeConfidence.Should().Be(95);
        result.ConfidenceDetails.MovementIdentificationConfidence.Should().BeGreaterThan(0);
        result.ConfidenceDetails.MovementsIdentified.Should().Be(2);
        result.ConfidenceDetails.TotalMovementLines.Should().Be(2);
    }

    [Fact]
    public void Validate_PartialMovementIdentification_ReducesConfidence()
    {
        // Arrange - Some movements not identified
        var workout = CreateValidWorkout();
        var typeDetection = CreateTypeDetection(WorkoutType.ForTime, 100);
        var movementResults = CreateMixedMovementResults();

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.ConfidenceScore.Should().BeLessThan(100);
        result.ConfidenceDetails!.MovementsIdentified.Should().BeLessThan(result.ConfidenceDetails.TotalMovementLines);
    }

    [Fact]
    public void Validate_Warnings_ReduceConfidence()
    {
        // Arrange
        var workout = CreateAmrapWorkoutWithoutTimeCap();
        var typeDetection = CreateTypeDetection(WorkoutType.Amrap, 100);
        var movementResults = CreateSuccessfulMovementResults(2, confidence: 100);

        // Act
        var resultWithWarning = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Arrange - Same but with time cap
        var workoutWithTimeCap = CreateAmrapWorkoutWithTimeCap();
        var resultWithoutWarning = ParsedWorkoutValidator.Validate(workoutWithTimeCap, typeDetection, movementResults);

        // Assert - Warning should reduce confidence
        resultWithWarning.ConfidenceScore.Should().BeLessThan(resultWithoutWarning.ConfidenceScore);
    }

    #endregion

    #region ParsedWorkoutResult Properties Tests

    [Fact]
    public void ParsedWorkoutResult_IsUsable_TrueWhenSuccess()
    {
        // Arrange
        var workout = CreateValidWorkout();
        var typeDetection = CreateSuccessfulTypeDetection();
        var movementResults = CreateSuccessfulMovementResults(2);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert
        result.IsUsable.Should().BeTrue();
    }

    [Fact]
    public void ParsedWorkoutResult_IsUsable_TrueWhenRecoverableWithWarnings()
    {
        // Arrange - Has warnings but valid workout with reasonable confidence
        var workout = CreateAmrapWorkoutWithoutTimeCap();
        var typeDetection = CreateTypeDetection(WorkoutType.Amrap, 80);
        var movementResults = CreateSuccessfulMovementResults(2, confidence: 80);

        // Act
        var result = ParsedWorkoutValidator.Validate(workout, typeDetection, movementResults);

        // Assert - Should be usable despite warnings (workout has movements so IsValid is true)
        result.IsUsable.Should().BeTrue();
    }

    [Fact]
    public void ParsedWorkoutResult_ConfidenceLevel_PerfectAt100()
    {
        // Arrange
        var result = new ParsedWorkoutResult { ConfidenceScore = 100 };

        // Assert
        result.ConfidenceLevel.Should().Be("Perfect");
    }

    [Fact]
    public void ParsedWorkoutResult_ConfidenceLevel_HighAt80Plus()
    {
        // Arrange
        var result = new ParsedWorkoutResult { ConfidenceScore = 85 };

        // Assert
        result.ConfidenceLevel.Should().Be("High");
    }

    [Fact]
    public void ParsedWorkoutResult_ConfidenceLevel_MediumAt60Plus()
    {
        // Arrange
        var result = new ParsedWorkoutResult { ConfidenceScore = 65 };

        // Assert
        result.ConfidenceLevel.Should().Be("Medium");
    }

    [Fact]
    public void ParsedWorkoutResult_ConfidenceLevel_LowBelow60()
    {
        // Arrange
        var result = new ParsedWorkoutResult { ConfidenceScore = 40 };

        // Assert
        result.ConfidenceLevel.Should().Be("Low");
    }

    #endregion

    #region Helper Methods

    private static ParsedWorkoutDto CreateValidWorkout()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "For Time:\n10 Pull-ups\n20 Push-ups",
            WorkoutType = WorkoutType.ForTime,
            Movements = new List<ParsedMovementDto>
            {
                new() { RepCount = 10, MovementName = "Pull-up", MovementDefinitionId = 1 },
                new() { RepCount = 20, MovementName = "Push-up", MovementDefinitionId = 2 }
            }
        };
    }

    private static ParsedWorkoutDto CreateWorkoutWithNoMovements()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "For Time",
            WorkoutType = WorkoutType.ForTime,
            Movements = new List<ParsedMovementDto>()
        };
    }

    private static ParsedWorkoutDto CreateAmrapWorkoutWithoutTimeCap()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "AMRAP:\n5 Pull-ups",
            WorkoutType = WorkoutType.Amrap,
            TimeCapSeconds = null,
            Movements = new List<ParsedMovementDto>
            {
                new() { RepCount = 5, MovementName = "Pull-up", MovementDefinitionId = 1 }
            }
        };
    }

    private static ParsedWorkoutDto CreateAmrapWorkoutWithTimeCap()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "20 min AMRAP:\n5 Pull-ups",
            WorkoutType = WorkoutType.Amrap,
            TimeCapSeconds = 1200,
            Movements = new List<ParsedMovementDto>
            {
                new() { RepCount = 5, MovementName = "Pull-up", MovementDefinitionId = 1 }
            }
        };
    }

    private static ParsedWorkoutDto CreateEmomWorkoutWithoutInterval()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "EMOM:\n10 Pull-ups",
            WorkoutType = WorkoutType.Emom,
            IntervalDurationSeconds = null,
            Movements = new List<ParsedMovementDto>
            {
                new() { RepCount = 10, MovementName = "Pull-up", MovementDefinitionId = 1 }
            }
        };
    }

    private static ParsedWorkoutDto CreateRoundsWorkoutWithoutRoundCount()
    {
        return new ParsedWorkoutDto
        {
            OriginalText = "Rounds:\n10 Pull-ups",
            WorkoutType = WorkoutType.Rounds,
            RoundCount = null,
            Movements = new List<ParsedMovementDto>
            {
                new() { RepCount = 10, MovementName = "Pull-up", MovementDefinitionId = 1 }
            }
        };
    }

    private static WorkoutTypeDetectionResult CreateSuccessfulTypeDetection()
    {
        return CreateTypeDetection(WorkoutType.ForTime, 100);
    }

    private static WorkoutTypeDetectionResult CreateTypeDetection(WorkoutType type, int confidence = 100)
    {
        return new WorkoutTypeDetectionResult
        {
            Type = type,
            Confidence = confidence
        };
    }

    private static WorkoutTypeDetectionResult CreateTypeDetectionWithError()
    {
        return new WorkoutTypeDetectionResult
        {
            Type = WorkoutType.ForTime,
            Confidence = 0,
            Error = new ParsingErrorDto
            {
                ErrorType = "TypeDetectionFailed",
                Message = "Could not determine workout type"
            }
        };
    }

    private static WorkoutTypeDetectionResult CreateTypeDetectionWithWarning()
    {
        return new WorkoutTypeDetectionResult
        {
            Type = WorkoutType.ForTime,
            Confidence = 80,
            Warning = new ParsingWarningDto
            {
                WarningType = "AmbiguousType",
                Message = "Workout type inferred from context"
            }
        };
    }

    private static List<MovementParseResult> CreateSuccessfulMovementResults(int count, int confidence = 100)
    {
        var results = new List<MovementParseResult>();
        for (int i = 0; i < count; i++)
        {
            results.Add(new MovementParseResult
            {
                Success = true,
                Confidence = confidence,
                Movement = new ParsedMovementDto
                {
                    RepCount = 10,
                    MovementName = $"Movement{i}",
                    MovementDefinitionId = i + 1
                }
            });
        }
        return results;
    }

    private static List<MovementParseResult> CreateMixedMovementResults()
    {
        return new List<MovementParseResult>
        {
            new()
            {
                Success = true,
                Confidence = 100,
                Movement = new ParsedMovementDto { RepCount = 10, MovementName = "Pull-up", MovementDefinitionId = 1 }
            },
            new()
            {
                Success = true,
                Confidence = 50,
                Movement = new ParsedMovementDto { RepCount = 5, MovementName = "Unknown", MovementDefinitionId = null },
                Warning = new ParsingWarningDto { WarningType = "UnknownMovement", Message = "Movement not found in dictionary" }
            },
            new()
            {
                Success = true,
                Confidence = 100,
                Movement = new ParsedMovementDto { RepCount = 20, MovementName = "Push-up", MovementDefinitionId = 2 }
            }
        };
    }

    #endregion
}
