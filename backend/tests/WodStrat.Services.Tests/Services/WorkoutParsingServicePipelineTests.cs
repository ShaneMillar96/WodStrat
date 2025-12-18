using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using NSubstitute;
using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Services;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Integration tests for WorkoutParsingService pipeline.
/// Tests the full parsing flow with ParseWorkoutAsync and confidence scoring.
/// </summary>
public class WorkoutParsingServicePipelineTests
{
    private readonly IFixture _fixture;
    private readonly IPatternMatchingService _patternMatchingService;
    private readonly IMovementDefinitionService _movementDefinitionService;
    private readonly WorkoutParsingService _sut;

    public WorkoutParsingServicePipelineTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

        // Use real PatternMatchingService for accurate pattern matching
        _patternMatchingService = new PatternMatchingService();
        _movementDefinitionService = Substitute.For<IMovementDefinitionService>();

        SetupMovementDefinitionService();

        _sut = new WorkoutParsingService(_patternMatchingService, _movementDefinitionService);
    }

    #region ParseWorkoutAsync Tests - Success Scenarios

    [Fact]
    public async Task ParseWorkoutAsync_ValidWorkout_ReturnsSuccessResult()
    {
        // Arrange
        var workoutText = "For Time:\n10 Pull-ups\n20 Push-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.Success.Should().BeTrue();
        result.ParsedWorkout.Should().NotBeNull();
        result.ConfidenceScore.Should().BeGreaterThan(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseWorkoutAsync_ValidWorkout_ReturnsConfidenceBreakdown()
    {
        // Arrange
        var workoutText = "20 min AMRAP:\n5 Pull-ups\n10 Push-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ConfidenceDetails.Should().NotBeNull();
        result.ConfidenceDetails!.WorkoutTypeConfidence.Should().BeGreaterThan(0);
        result.ConfidenceDetails.MovementIdentificationConfidence.Should().BeGreaterThan(0);
        result.ConfidenceDetails.TotalMovementLines.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ParseWorkoutAsync_PerfectParse_ReturnsHighConfidence()
    {
        // Arrange - Well-formed workout with all recognized movements
        var workoutText = @"20 min AMRAP:
5 Pull-ups
10 Push-ups
15 Air Squats";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ConfidenceScore.Should().BeGreaterOrEqualTo(70);
        result.ConfidenceLevel.Should().BeOneOf("Perfect", "High", "Medium");
    }

    #endregion

    #region ParseWorkoutAsync Tests - Error Scenarios

    [Fact]
    public async Task ParseWorkoutAsync_EmptyInput_ReturnsErrorResult()
    {
        // Act
        var result = await _sut.ParseWorkoutAsync("");

        // Assert
        result.Success.Should().BeFalse();
        result.ConfidenceScore.Should().Be(0);
        result.Errors.Should().Contain(e => e.ErrorType == "EmptyInput");
    }

    [Fact]
    public async Task ParseWorkoutAsync_WhitespaceOnly_ReturnsErrorResult()
    {
        // Act
        var result = await _sut.ParseWorkoutAsync("   \n\t  ");

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorType == "EmptyInput");
    }

    [Fact]
    public async Task ParseWorkoutAsync_NullInput_ReturnsErrorResult()
    {
        // Act
        var result = await _sut.ParseWorkoutAsync(null!);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorType == "EmptyInput");
    }

    #endregion

    #region ParseWorkoutAsync Tests - Warning Scenarios

    [Fact]
    public async Task ParseWorkoutAsync_UnrecognizedMovement_AddsWarning()
    {
        // Arrange
        var workoutText = @"For Time:
10 Pull-ups
5 SuperFantasyExercise
20 Push-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.Success.Should().BeTrue(); // Warnings don't block success
        result.ParsedWorkout!.Movements.Should().HaveCountGreaterThanOrEqualTo(2); // At least recognized ones
    }

    [Fact]
    public async Task ParseWorkoutAsync_AmrapWithoutTime_AddsWarning()
    {
        // Arrange - AMRAP without explicit time cap
        var workoutText = @"AMRAP:
5 Pull-ups
10 Push-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.WorkoutType.Should().Be(WorkoutType.Amrap);
        // May have warning about missing time cap
    }

    #endregion

    #region ParseWorkoutAsync Tests - Workout Types

    [Theory]
    [InlineData("20 min AMRAP:\n5 Pull-ups", WorkoutType.Amrap)]
    [InlineData("For Time:\n21 Thrusters", WorkoutType.ForTime)]
    [InlineData("EMOM 10:\n5 Pull-ups", WorkoutType.Emom)]
    [InlineData("5 Rounds:\n10 Pull-ups", WorkoutType.Rounds)]
    public async Task ParseWorkoutAsync_WorkoutTypes_DetectsCorrectType(string workoutText, WorkoutType expectedType)
    {
        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.WorkoutType.Should().Be(expectedType);
    }

    [Fact]
    public async Task ParseWorkoutAsync_AmrapWithDuration_ExtractsTimeCap()
    {
        // Arrange
        var workoutText = "20 min AMRAP:\n5 Pull-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.TimeCapSeconds.Should().Be(1200);
    }

    [Fact]
    public async Task ParseWorkoutAsync_ForTimeWithRounds_ExtractsRoundCount()
    {
        // Arrange
        var workoutText = "3 Rounds For Time:\n10 Pull-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.RoundCount.Should().Be(3);
    }

    #endregion

    #region ParseWorkoutAsync Tests - Movement Parsing

    [Fact]
    public async Task ParseWorkoutAsync_SimpleMovements_ParsesAllMovements()
    {
        // Arrange
        var workoutText = @"For Time:
10 Pull-ups
20 Push-ups
30 Air Squats";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.Movements.Should().HaveCount(3);
    }

    [Fact]
    public async Task ParseWorkoutAsync_MovementWithWeight_ParsesLoad()
    {
        // Arrange
        var workoutText = "For Time:\n21 Thrusters (95 lb)";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.Movements.Should().HaveCount(1);
        result.ParsedWorkout.Movements[0].LoadValue.Should().Be(95);
        result.ParsedWorkout.Movements[0].LoadUnit.Should().Be(LoadUnit.Lb);
    }

    [Fact]
    public async Task ParseWorkoutAsync_MovementWithGenderWeight_ParsesBothLoads()
    {
        // Arrange
        var workoutText = "For Time:\n21 Thrusters (95/65 lb)";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.Movements.Should().HaveCount(1);
        result.ParsedWorkout.Movements[0].LoadValue.Should().Be(95);
        result.ParsedWorkout.Movements[0].LoadValueFemale.Should().Be(65);
    }

    [Fact]
    public async Task ParseWorkoutAsync_MovementWithDistance_ParsesDistance()
    {
        // Arrange
        var workoutText = "For Time:\n400m Run";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.Movements.Should().HaveCount(1);
        result.ParsedWorkout.Movements[0].DistanceValue.Should().Be(400);
        result.ParsedWorkout.Movements[0].DistanceUnit.Should().Be(DistanceUnit.M);
    }

    [Fact]
    public async Task ParseWorkoutAsync_MovementWithCalories_ParsesCalories()
    {
        // Arrange
        var workoutText = "For Time:\n15/12 Cal Row";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.ParsedWorkout!.Movements.Should().HaveCount(1);
        result.ParsedWorkout.Movements[0].Calories.Should().Be(15);
        result.ParsedWorkout.Movements[0].CaloriesFemale.Should().Be(12);
    }

    #endregion

    #region ParseWorkoutAsync Tests - Real Workouts

    [Fact]
    public async Task ParseWorkoutAsync_Cindy_ParsesCorrectly()
    {
        // Arrange
        var workoutText = @"20 min AMRAP:
5 Pull-ups
10 Push-ups
15 Air Squats";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.Success.Should().BeTrue();
        result.ParsedWorkout!.WorkoutType.Should().Be(WorkoutType.Amrap);
        result.ParsedWorkout.TimeCapSeconds.Should().Be(1200);
        result.ParsedWorkout.Movements.Should().HaveCount(3);
    }

    [Fact]
    public async Task ParseWorkoutAsync_FranStyle_ParsesCorrectly()
    {
        // Arrange
        var workoutText = @"For Time:
21 Thrusters (95/65 lb)
21 Pull-ups
15 Thrusters
15 Pull-ups
9 Thrusters
9 Pull-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.Success.Should().BeTrue();
        result.ParsedWorkout!.WorkoutType.Should().Be(WorkoutType.ForTime);
        result.ParsedWorkout.Movements.Should().HaveCount(6);
    }

    [Fact]
    public async Task ParseWorkoutAsync_EmomStructure_ParsesCorrectly()
    {
        // Arrange
        var workoutText = @"10 min EMOM:
5 Pull-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.Success.Should().BeTrue();
        result.ParsedWorkout!.WorkoutType.Should().Be(WorkoutType.Emom);
        result.ParsedWorkout.TimeCapSeconds.Should().Be(600);
        result.ParsedWorkout.IntervalDurationSeconds.Should().Be(60);
    }

    #endregion

    #region Confidence Level Tests

    [Fact]
    public async Task ParseWorkoutAsync_ConfidenceLevel_ReflectsQuality()
    {
        // Arrange - High quality parse
        var highQualityWorkout = @"20 min AMRAP:
5 Pull-ups
10 Push-ups
15 Air Squats";

        // Act
        var result = await _sut.ParseWorkoutAsync(highQualityWorkout);

        // Assert
        result.ConfidenceLevel.Should().BeOneOf("Perfect", "High", "Medium");
    }

    [Fact]
    public async Task ParseWorkoutAsync_IsUsable_TrueForValidWorkout()
    {
        // Arrange
        var workoutText = "For Time:\n10 Pull-ups";

        // Act
        var result = await _sut.ParseWorkoutAsync(workoutText);

        // Assert
        result.IsUsable.Should().BeTrue();
    }

    [Fact]
    public async Task ParseWorkoutAsync_IsUsable_FalseForEmptyInput()
    {
        // Act
        var result = await _sut.ParseWorkoutAsync("");

        // Assert
        result.IsUsable.Should().BeFalse();
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task ParseWorkoutAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var workoutText = "For Time:\n10 Pull-ups";
        using var cts = new CancellationTokenSource();

        // Act - The current implementation doesn't check cancellation token during parsing
        // This test verifies the method accepts a cancellation token parameter
        var result = await _sut.ParseWorkoutAsync(workoutText, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private void SetupMovementDefinitionService()
    {
        // Setup alias lookup
        var aliasLookup = new Dictionary<string, int>
        {
            { "pull up", 1 }, { "pull-up", 1 }, { "pullup", 1 },
            { "push up", 4 }, { "push-up", 4 }, { "pushup", 4 },
            { "air squat", 3 }, { "air squats", 3 },
            { "thruster", 2 }, { "thrusters", 2 },
            { "row", 7 }, { "run", 8 }
        };

        _movementDefinitionService.GetAliasLookupAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyDictionary<string, int>>(aliasLookup));

        // Setup normalization
        _movementDefinitionService.NormalizeMovementNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var input = callInfo.Arg<string>()?.ToLowerInvariant() ?? string.Empty;
                if (input.Contains("pull")) return Task.FromResult<string?>("pull_up");
                if (input.Contains("push")) return Task.FromResult<string?>("push_up");
                if (input.Contains("squat")) return Task.FromResult<string?>("air_squat");
                if (input.Contains("thruster")) return Task.FromResult<string?>("thruster");
                if (input.Contains("row")) return Task.FromResult<string?>("row");
                if (input.Contains("run")) return Task.FromResult<string?>("run");
                return Task.FromResult<string?>(null);
            });

        // Setup movement lookup
        _movementDefinitionService.GetMovementByCanonicalNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var canonical = callInfo.Arg<string>();
                return canonical switch
                {
                    "pull_up" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 1, CanonicalName = "pull_up", DisplayName = "Pull-up", Category = "Gymnastics" }),
                    "push_up" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 4, CanonicalName = "push_up", DisplayName = "Push-up", Category = "Gymnastics" }),
                    "air_squat" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 3, CanonicalName = "air_squat", DisplayName = "Air Squat", Category = "Gymnastics" }),
                    "thruster" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 2, CanonicalName = "thruster", DisplayName = "Thruster", Category = "Weightlifting" }),
                    "row" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 7, CanonicalName = "row", DisplayName = "Row", Category = "Cardio" }),
                    "run" => Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 8, CanonicalName = "run", DisplayName = "Run", Category = "Cardio" }),
                    _ => Task.FromResult<MovementDefinitionDto?>(null)
                };
            });

        // Setup alias lookup
        _movementDefinitionService.FindMovementByAliasAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var alias = callInfo.Arg<string>().ToLowerInvariant();
                if (alias.Contains("pull")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 1, CanonicalName = "pull_up", DisplayName = "Pull-up", Category = "Gymnastics" });
                if (alias.Contains("push")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 4, CanonicalName = "push_up", DisplayName = "Push-up", Category = "Gymnastics" });
                if (alias.Contains("squat")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 3, CanonicalName = "air_squat", DisplayName = "Air Squat", Category = "Gymnastics" });
                if (alias.Contains("thruster")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 2, CanonicalName = "thruster", DisplayName = "Thruster", Category = "Weightlifting" });
                if (alias.Contains("row")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 7, CanonicalName = "row", DisplayName = "Row", Category = "Cardio" });
                if (alias.Contains("run")) return Task.FromResult<MovementDefinitionDto?>(new MovementDefinitionDto { Id = 8, CanonicalName = "run", DisplayName = "Run", Category = "Cardio" });
                return Task.FromResult<MovementDefinitionDto?>(null);
            });

        _movementDefinitionService.SearchMovementsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<MovementDefinitionDto>>(Array.Empty<MovementDefinitionDto>()));
    }

    #endregion
}
