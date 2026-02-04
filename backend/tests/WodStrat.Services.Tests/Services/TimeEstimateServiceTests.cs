using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Services;
using WodStrat.Services.Tests.Customizations;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for TimeEstimateService.
/// </summary>
public class TimeEstimateServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly IPacingService _pacingService;
    private readonly TimeEstimateService _sut;

    public TimeEstimateServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new TimeEstimateCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _athleteService = Substitute.For<IAthleteService>();
        _pacingService = Substitute.For<IPacingService>();

        _sut = new TimeEstimateService(_database, _athleteService, _pacingService);
    }

    #region CalculateTimeRange Tests

    [Fact]
    public void CalculateTimeRange_BaseTime600s_50thPercentile_Intermediate_ReturnsExpectedRange()
    {
        // Arrange
        var baseTimeSeconds = 600;
        var athletePercentile = 50m; // Falls in 40-60 range, adjustment factor ~1.10 (slightly slower)
        var experience = ExperienceLevel.Intermediate; // +/- 15% base
        var benchmarkCoverage = 80; // High coverage, no additional adjustment

        // Act
        var (minSeconds, maxSeconds) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, benchmarkCoverage);

        // Assert - The actual formula applies adjustment factor first, then range
        // 50th percentile is in 40-60 range = 1.10 adjustment (10% slower)
        // Adjusted time = 600 * 1.10 = 660
        // Range: 660 * 0.85 = 561 to 660 * 1.15 = 759
        minSeconds.Should().BeGreaterThan(0);
        maxSeconds.Should().BeGreaterThan(minSeconds);
        minSeconds.Should().BeInRange(500, 650);
        maxSeconds.Should().BeInRange(700, 850);
    }

    [Fact]
    public void CalculateTimeRange_HighPercentile_ReturnsFasterTimes()
    {
        // Arrange
        var baseTimeSeconds = 600;
        var athletePercentile = 95m;
        var experience = ExperienceLevel.Intermediate;
        var benchmarkCoverage = 80;

        // Act
        var (minSeconds, maxSeconds) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, benchmarkCoverage);

        // Assert - 95th percentile = faster (0.85 adjustment factor)
        minSeconds.Should().BeLessThan(600);
        maxSeconds.Should().BeLessThan(700);
    }

    [Fact]
    public void CalculateTimeRange_LowPercentile_ReturnsSlowerTimes()
    {
        // Arrange
        var baseTimeSeconds = 600;
        var athletePercentile = 20m;
        var experience = ExperienceLevel.Intermediate;
        var benchmarkCoverage = 80;

        // Act
        var (minSeconds, maxSeconds) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, benchmarkCoverage);

        // Assert - 20th percentile = slower (1.25 adjustment factor)
        minSeconds.Should().BeGreaterThan(500);
        maxSeconds.Should().BeGreaterThan(700);
    }

    [Fact]
    public void CalculateTimeRange_BeginnerExperience_ReturnsWiderRange()
    {
        // Arrange
        var baseTimeSeconds = 600;
        var athletePercentile = 50m;
        var experience = ExperienceLevel.Beginner;
        var benchmarkCoverage = 80;

        // Act
        var (minSecondsBegin, maxSecondsBegin) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, benchmarkCoverage);
        var (minSecondsAdv, maxSecondsAdv) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, ExperienceLevel.Advanced, benchmarkCoverage);

        // Assert - Beginner range should be wider than Advanced
        var beginnerRange = maxSecondsBegin - minSecondsBegin;
        var advancedRange = maxSecondsAdv - minSecondsAdv;
        beginnerRange.Should().BeGreaterThan(advancedRange);
    }

    [Fact]
    public void CalculateTimeRange_LowBenchmarkCoverage_ReturnsWiderRange()
    {
        // Arrange
        var baseTimeSeconds = 600;
        var athletePercentile = 50m;
        var experience = ExperienceLevel.Intermediate;

        // Act
        var (minSecondsLow, maxSecondsLow) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, 30);
        var (minSecondsHigh, maxSecondsHigh) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, 100);

        // Assert - Low coverage range should be wider than high coverage
        var lowCoverageRange = maxSecondsLow - minSecondsLow;
        var highCoverageRange = maxSecondsHigh - minSecondsHigh;
        lowCoverageRange.Should().BeGreaterThan(highCoverageRange);
    }

    [Fact]
    public void CalculateTimeRange_VerySmallBaseTime_EnsuresMinimumValues()
    {
        // Arrange
        var baseTimeSeconds = 5;
        var athletePercentile = 50m;
        var experience = ExperienceLevel.Advanced;
        var benchmarkCoverage = 100;

        // Act
        var (minSeconds, maxSeconds) = _sut.CalculateTimeRange(
            baseTimeSeconds, athletePercentile, experience, benchmarkCoverage);

        // Assert - Should have minimum reasonable values
        minSeconds.Should().BeGreaterThanOrEqualTo(1);
        maxSeconds.Should().BeGreaterThan(minSeconds);
    }

    #endregion

    #region CalculateRestRecommendations Tests

    [Fact]
    public void CalculateRestRecommendations_LightPacing_ReturnsLongerRest()
    {
        // Arrange
        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.DisplayName, "Thruster")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .Create();
        var movements = new List<WorkoutMovement> { movement };
        var overallPacing = PacingLevel.Light;
        var movementPacingLevels = new Dictionary<int, PacingLevel> { { 1, PacingLevel.Light } };

        // Act
        var result = _sut.CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Assert - Light pacing = weakness = longer rest (15-20s)
        result.Should().HaveCount(1);
        result[0].SuggestedRestSeconds.Should().BeInRange(15, 20);
        result[0].PacingLevel.Should().Be("Light");
        result[0].Reasoning.Should().Contain("weakness");
    }

    [Fact]
    public void CalculateRestRecommendations_HeavyPacing_ReturnsShorterRest()
    {
        // Arrange
        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.DisplayName, "Thruster")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .Create();
        var movements = new List<WorkoutMovement> { movement };
        var overallPacing = PacingLevel.Heavy;
        var movementPacingLevels = new Dictionary<int, PacingLevel> { { 1, PacingLevel.Heavy } };

        // Act
        var result = _sut.CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Assert - Heavy pacing = strength = shorter rest (3-5s)
        result.Should().HaveCount(1);
        result[0].SuggestedRestSeconds.Should().BeInRange(3, 5);
        result[0].PacingLevel.Should().Be("Heavy");
        result[0].Reasoning.Should().Contain("strength");
    }

    [Fact]
    public void CalculateRestRecommendations_ModeratePacing_ReturnsMediumRest()
    {
        // Arrange
        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.DisplayName, "Thruster")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .Create();
        var movements = new List<WorkoutMovement> { movement };
        var overallPacing = PacingLevel.Moderate;
        var movementPacingLevels = new Dictionary<int, PacingLevel> { { 1, PacingLevel.Moderate } };

        // Act
        var result = _sut.CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Assert - Moderate pacing = average = medium rest (8-12s)
        result.Should().HaveCount(1);
        result[0].SuggestedRestSeconds.Should().BeInRange(8, 12);
        result[0].PacingLevel.Should().Be("Moderate");
    }

    [Fact]
    public void CalculateRestRecommendations_MultipleMovements_ReturnsRecommendationForEach()
    {
        // Arrange
        var movementDef1 = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.DisplayName, "Thruster")
            .Create();
        var movementDef2 = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 2)
            .With(x => x.DisplayName, "Pull-up")
            .Create();
        var movement1 = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef1)
            .Create();
        var movement2 = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 2)
            .With(x => x.MovementDefinition, movementDef2)
            .Create();
        var movements = new List<WorkoutMovement> { movement1, movement2 };
        var overallPacing = PacingLevel.Moderate;
        var movementPacingLevels = new Dictionary<int, PacingLevel>
        {
            { 1, PacingLevel.Heavy },
            { 2, PacingLevel.Light }
        };

        // Act
        var result = _sut.CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Assert
        result.Should().HaveCount(2);
        result[0].AfterMovement.Should().Be("Thruster");
        result[0].PacingLevel.Should().Be("Heavy");
        result[1].AfterMovement.Should().Be("Pull-up");
        result[1].PacingLevel.Should().Be("Light");
    }

    [Fact]
    public void CalculateRestRecommendations_MovementNotInDictionary_UsesOverallPacing()
    {
        // Arrange
        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.DisplayName, "Thruster")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .Create();
        var movements = new List<WorkoutMovement> { movement };
        var overallPacing = PacingLevel.Heavy;
        var movementPacingLevels = new Dictionary<int, PacingLevel>(); // Empty - no specific levels

        // Act
        var result = _sut.CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Assert - Should use overall pacing (Heavy)
        result.Should().HaveCount(1);
        result[0].PacingLevel.Should().Be("Heavy");
    }

    #endregion

    #region FormatTimeRange Tests

    [Fact]
    public void FormatTimeRange_SecondsOnly_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatTimeRange(45, 55);

        // Assert
        result.Should().Be("0:45 - 0:55");
    }

    [Fact]
    public void FormatTimeRange_MinutesAndSeconds_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatTimeRange(510, 615);

        // Assert
        result.Should().Be("8:30 - 10:15");
    }

    [Fact]
    public void FormatTimeRange_Over10Minutes_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatTimeRange(720, 900);

        // Assert
        result.Should().Be("12:00 - 15:00");
    }

    #endregion

    #region FormatAmrapRange Tests

    [Fact]
    public void FormatAmrapRange_SameRoundsNoReps_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatAmrapRange(5, 0, 5, 0, 30);

        // Assert
        result.Should().Be("5 to 5 rounds");
    }

    [Fact]
    public void FormatAmrapRange_DifferentRoundsWithReps_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatAmrapRange(4, 12, 5, 8, 30);

        // Assert
        result.Should().Be("4+12 to 5+8 rounds");
    }

    [Fact]
    public void FormatAmrapRange_ZeroExtraReps_OmitsReps()
    {
        // Act
        var result = _sut.FormatAmrapRange(4, 0, 6, 0, 30);

        // Assert
        result.Should().Be("4 to 6 rounds");
    }

    #endregion

    #region EstimateWorkoutTimeAsync Tests

    [Fact]
    public async Task EstimateWorkoutTimeAsync_WorkoutNotFound_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        var emptyWorkouts = new List<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(emptyWorkouts);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EstimateWorkoutTimeAsync_DeletedWorkout_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.IsDeleted, true)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workouts);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EstimateWorkoutTimeAsync_ForTimeWorkout_ReturnsTimeEstimate()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Weightlifting)
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 21)
            .With(x => x.SequenceOrder, 1)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.Name, "Test Workout")
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RoundCount, 1)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, athleteId)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.IsDeleted, false)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        var athletes = new List<Athlete> { athlete }.AsQueryable().BuildMock();

        _database.Get<Workout>().Returns(workouts);
        _database.Get<Athlete>().Returns(athletes);

        var pacingDto = _fixture.Build<MovementPacingDto>()
            .With(x => x.AthletePercentile, 65m)
            .With(x => x.PacingLevel, "Moderate")
            .Create();
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(pacingDto);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutId.Should().Be(workoutId);
        result.WorkoutName.Should().Be("Test Workout");
        result.WorkoutType.Should().Be("ForTime");
        result.EstimateType.Should().Be("Time");
        result.MinEstimate.Should().BeGreaterThan(0);
        result.MaxEstimate.Should().BeGreaterThan(result.MinEstimate);
        result.FormattedRange.Should().NotBeNullOrEmpty();
        result.ConfidenceLevel.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EstimateWorkoutTimeAsync_AmrapWorkout_ReturnsRoundsEstimate()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Gymnastics)
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 10)
            .With(x => x.SequenceOrder, 1)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.Name, "AMRAP Test")
            .With(x => x.WorkoutType, WorkoutType.Amrap)
            .With(x => x.TimeCapSeconds, 720) // 12 minutes
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, athleteId)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.IsDeleted, false)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        var athletes = new List<Athlete> { athlete }.AsQueryable().BuildMock();

        _database.Get<Workout>().Returns(workouts);
        _database.Get<Athlete>().Returns(athletes);

        var pacingDto = _fixture.Build<MovementPacingDto>()
            .With(x => x.AthletePercentile, 70m)
            .With(x => x.PacingLevel, "Moderate")
            .Create();
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(pacingDto);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutType.Should().Be("Amrap");
        result.EstimateType.Should().Be("RoundsReps");
        result.MinEstimate.Should().BeGreaterThanOrEqualTo(0); // Rounds
        result.MaxEstimate.Should().BeGreaterThanOrEqualTo(result.MinEstimate);
        result.FormattedRange.Should().Contain("rounds");
    }

    [Fact]
    public async Task EstimateWorkoutTimeAsync_EmomWorkout_ReturnsFeasibilityData()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Weightlifting)
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 10)
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MinuteStart, 1)
            .With(x => x.MinuteEnd, 10)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.Name, "EMOM Test")
            .With(x => x.WorkoutType, WorkoutType.Emom)
            .With(x => x.TimeCapSeconds, 600) // 10 minutes
            .With(x => x.IntervalDurationSeconds, 60)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, athleteId)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.IsDeleted, false)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        var athletes = new List<Athlete> { athlete }.AsQueryable().BuildMock();

        _database.Get<Workout>().Returns(workouts);
        _database.Get<Athlete>().Returns(athletes);

        var pacingDto = _fixture.Build<MovementPacingDto>()
            .With(x => x.AthletePercentile, 65m)
            .With(x => x.PacingLevel, "Moderate")
            .Create();
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(pacingDto);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutType.Should().Be("Emom");
        result.EmomFeasibility.Should().NotBeNull();
        result.EmomFeasibility.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region EstimateCurrentUserWorkoutTimeAsync Tests

    [Fact]
    public async Task EstimateCurrentUserWorkoutTimeAsync_NoAthleteProfile_ReturnsNull()
    {
        // Arrange
        var workoutId = _fixture.Create<int>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.EstimateCurrentUserWorkoutTimeAsync(workoutId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EstimateCurrentUserWorkoutTimeAsync_HasAthleteProfile_CallsEstimateWithAthleteId()
    {
        // Arrange
        var workoutId = 123;
        var athleteId = 456;

        var athleteDto = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Return null from EstimateWorkoutTimeAsync since we're just testing the routing
        var emptyWorkouts = new List<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(emptyWorkouts);

        // Act
        var result = await _sut.EstimateCurrentUserWorkoutTimeAsync(workoutId);

        // Assert
        result.Should().BeNull(); // Because workout not found
        await _athleteService.Received(1).GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region CheckEmomFeasibilityAsync Tests

    [Fact]
    public async Task CheckEmomFeasibilityAsync_FastMovements_AllMinutesFeasible()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Gymnastics) // 2.5s per rep
            .With(x => x.DisplayName, "Air Squat")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 10) // 10 * 2.5 = 25s, well under 60s
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MinuteStart, 1)
            .With(x => x.MinuteEnd, 5)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.WorkoutType, WorkoutType.Emom)
            .With(x => x.TimeCapSeconds, 300) // 5 minutes
            .With(x => x.IntervalDurationSeconds, 60)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        // No pacing data - uses base times
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((MovementPacingDto?)null);

        // Act
        var result = await _sut.CheckEmomFeasibilityAsync(athleteId, workout);

        // Assert
        result.Should().HaveCount(5);
        result.All(f => f.IsFeasible).Should().BeTrue();
        result.All(f => f.BufferSeconds > 0).Should().BeTrue();
    }

    [Fact]
    public async Task CheckEmomFeasibilityAsync_SlowMovements_SomeMinutesInfeasible()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Strongman) // 5s per rep
            .With(x => x.DisplayName, "Tire Flip")
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 15) // 15 * 5 = 75s, over 60s
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MinuteStart, 1)
            .With(x => x.MinuteEnd, 3)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.WorkoutType, WorkoutType.Emom)
            .With(x => x.TimeCapSeconds, 180) // 3 minutes
            .With(x => x.IntervalDurationSeconds, 60)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((MovementPacingDto?)null);

        // Act
        var result = await _sut.CheckEmomFeasibilityAsync(athleteId, workout);

        // Assert
        result.Should().HaveCount(3);
        result.Any(f => !f.IsFeasible).Should().BeTrue();
        result.Where(f => !f.IsFeasible).All(f => f.Recommendation.Contains("scaling")).Should().BeTrue();
    }

    #endregion

    #region Confidence Level Tests

    [Fact]
    public async Task EstimateWorkoutTimeAsync_HighBenchmarkCoverage_ReturnsHighConfidence()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Weightlifting)
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 21)
            .With(x => x.SequenceOrder, 1)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RoundCount, 1)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, athleteId)
            .With(x => x.IsDeleted, false)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        var athletes = new List<Athlete> { athlete }.AsQueryable().BuildMock();

        _database.Get<Workout>().Returns(workouts);
        _database.Get<Athlete>().Returns(athletes);

        // Return pacing data for all movements (100% coverage)
        var pacingDto = _fixture.Build<MovementPacingDto>()
            .With(x => x.AthletePercentile, 65m)
            .Create();
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(pacingDto);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().NotBeNull();
        result!.ConfidenceLevel.Should().Be(TimeEstimateConfidenceLevel.High);
        result.BenchmarkCoverageCount.Should().Be(1);
        result.TotalMovementCount.Should().Be(1);
    }

    [Fact]
    public async Task EstimateWorkoutTimeAsync_NoBenchmarkData_ReturnsLowConfidence()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = 123;

        var movementDef = _fixture.Build<MovementDefinition>()
            .With(x => x.Id, 1)
            .With(x => x.Category, MovementCategory.Weightlifting)
            .Create();
        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementDefinition, movementDef)
            .With(x => x.RepCount, 21)
            .With(x => x.SequenceOrder, 1)
            .Create();
        var movements = new List<WorkoutMovement> { movement };

        var workout = _fixture.Build<Workout>()
            .With(x => x.Id, workoutId)
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RoundCount, 1)
            .With(x => x.Movements, movements)
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, athleteId)
            .With(x => x.IsDeleted, false)
            .Create();

        var workouts = new List<Workout> { workout }.AsQueryable().BuildMock();
        var athletes = new List<Athlete> { athlete }.AsQueryable().BuildMock();

        _database.Get<Workout>().Returns(workouts);
        _database.Get<Athlete>().Returns(athletes);

        // No pacing data available (0% coverage)
        _pacingService.CalculateMovementPacingAsync(
            athleteId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((MovementPacingDto?)null);

        // Act
        var result = await _sut.EstimateWorkoutTimeAsync(athleteId, workoutId);

        // Assert
        result.Should().NotBeNull();
        result!.ConfidenceLevel.Should().Be(TimeEstimateConfidenceLevel.Low);
        result.BenchmarkCoverageCount.Should().Be(0);
    }

    #endregion
}
