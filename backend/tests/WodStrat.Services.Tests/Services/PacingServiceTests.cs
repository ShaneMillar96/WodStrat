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
/// Unit tests for PacingService.
/// </summary>
public class PacingServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly PacingService _sut;

    public PacingServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new PacingCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _athleteService = Substitute.For<IAthleteService>();
        _sut = new PacingService(_database, _athleteService);
    }

    #region CalculateAthletePercentile Tests - Higher is Better (Weight/Reps)

    [Fact]
    public void CalculateAthletePercentile_WeightMetric_AtPercentile20_Returns20()
    {
        // Arrange
        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        var athleteValue = 70m; // Exactly at 20th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);

        // Assert
        result.Should().BeApproximately(20m, 0.1m);
    }

    [Fact]
    public void CalculateAthletePercentile_WeightMetric_AtPercentile80_Returns80()
    {
        // Arrange
        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        var athleteValue = 135m; // Exactly at 80th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);

        // Assert
        result.Should().BeApproximately(80m, 0.1m);
    }

    [Fact]
    public void CalculateAthletePercentile_WeightMetric_BetweenPercentiles_InterpolatesCorrectly()
    {
        // Arrange
        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        var athleteValue = 100m; // Midpoint between 40th (90) and 60th (110) percentiles

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);

        // Assert
        result.Should().BeApproximately(50m, 0.1m);
    }

    [Fact]
    public void CalculateAthletePercentile_WeightMetric_AbovePercentile95_ReturnsAbove95()
    {
        // Arrange
        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        var athleteValue = 185m; // Above 95th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);

        // Assert
        result.Should().BeGreaterThan(95m);
    }

    [Fact]
    public void CalculateAthletePercentile_WeightMetric_BelowPercentile20_ReturnsBelow20()
    {
        // Arrange
        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        var athleteValue = 50m; // Below 20th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);

        // Assert
        result.Should().BeLessThan(20m);
    }

    [Fact]
    public void CalculateAthletePercentile_RepsMetric_HigherIsBetter()
    {
        // Arrange
        var populationData = CreatePopulationData(5m, 12m, 20m, 30m, 45m);
        var athleteValue = 25m; // Between 60th (20) and 80th (30) percentiles

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Reps);

        // Assert
        result.Should().BeApproximately(70m, 0.1m);
    }

    #endregion

    #region CalculateAthletePercentile Tests - Lower is Better (Time/Pace)

    [Fact]
    public void CalculateAthletePercentile_TimeMetric_AtPercentile95_Returns95()
    {
        // Arrange - For time metrics, lower values are at higher percentiles
        var populationData = CreatePopulationData(115m, 105m, 95m, 87m, 78m);
        var athleteValue = 78m; // At 95th percentile (fastest)

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Time);

        // Assert
        result.Should().BeApproximately(95m, 0.1m);
    }

    [Fact]
    public void CalculateAthletePercentile_TimeMetric_AtPercentile20_Returns20()
    {
        // Arrange - For time metrics, higher values are at lower percentiles
        var populationData = CreatePopulationData(115m, 105m, 95m, 87m, 78m);
        var athleteValue = 115m; // At 20th percentile (slowest)

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Time);

        // Assert
        result.Should().BeApproximately(20m, 0.1m);
    }

    [Fact]
    public void CalculateAthletePercentile_TimeMetric_FasterThanPercentile95_ReturnsAbove95()
    {
        // Arrange
        var populationData = CreatePopulationData(115m, 105m, 95m, 87m, 78m);
        var athleteValue = 70m; // Faster than 95th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Time);

        // Assert
        result.Should().BeGreaterThan(95m);
    }

    [Fact]
    public void CalculateAthletePercentile_TimeMetric_SlowerThanPercentile20_ReturnsBelow20()
    {
        // Arrange
        var populationData = CreatePopulationData(115m, 105m, 95m, 87m, 78m);
        var athleteValue = 130m; // Slower than 20th percentile

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Time);

        // Assert
        result.Should().BeLessThan(20m);
    }

    [Fact]
    public void CalculateAthletePercentile_PaceMetric_LowerIsBetter()
    {
        // Arrange - Pace is also lower-is-better
        var populationData = CreatePopulationData(120m, 110m, 100m, 92m, 83m);
        var athleteValue = 96m; // Between 60th (100) and 80th (92) percentiles

        // Act
        var result = _sut.CalculateAthletePercentile(athleteValue, populationData, BenchmarkMetricType.Pace);

        // Assert
        result.Should().BeApproximately(70m, 1m);
    }

    #endregion

    #region CalculateSetBreakdown Tests - Heavy Pacing

    [Theory]
    [InlineData(9)]
    [InlineData(12)]
    [InlineData(15)]
    public void CalculateSetBreakdown_Heavy_SmallReps_ReturnsUnbroken(int totalReps)
    {
        // Act
        var result = _sut.CalculateSetBreakdown(totalReps, PacingLevel.Heavy);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(totalReps);
    }

    [Fact]
    public void CalculateSetBreakdown_Heavy_21Reps_ReturnsLargeSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(21, PacingLevel.Heavy);

        // Assert
        result.Should().HaveCount(2);
        result.Sum().Should().Be(21);
        result[0].Should().BeGreaterThan(result[1]); // First set should be larger
    }

    [Fact]
    public void CalculateSetBreakdown_Heavy_LargeReps_CapsAtMax15PerSet()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(45, PacingLevel.Heavy);

        // Assert
        result.Sum().Should().Be(45);
        result.Max().Should().BeLessOrEqualTo(15);
    }

    [Fact]
    public void CalculateSetBreakdown_Heavy_ZeroReps_ReturnsEmptyArray()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(0, PacingLevel.Heavy);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CalculateSetBreakdown Tests - Moderate Pacing

    [Fact]
    public void CalculateSetBreakdown_Moderate_SmallReps_ReturnsUnbroken()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(6, PacingLevel.Moderate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(6);
    }

    [Fact]
    public void CalculateSetBreakdown_Moderate_21Reps_ReturnsBalancedSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(21, PacingLevel.Moderate);

        // Assert
        result.Sum().Should().Be(21);
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Max().Should().BeLessOrEqualTo(12);
    }

    [Fact]
    public void CalculateSetBreakdown_Moderate_15Reps_ReturnsTwoSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(15, PacingLevel.Moderate);

        // Assert
        result.Sum().Should().Be(15);
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    #endregion

    #region CalculateSetBreakdown Tests - Light Pacing

    [Fact]
    public void CalculateSetBreakdown_Light_21Reps_ReturnsSmallSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(21, PacingLevel.Light);

        // Assert
        result.Sum().Should().Be(21);
        result.Should().HaveCountGreaterOrEqualTo(3);
        result.Max().Should().BeLessOrEqualTo(7);
    }

    [Fact]
    public void CalculateSetBreakdown_Light_21Reps_PrefersEqualDistribution()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(21, PacingLevel.Light);

        // Assert - 21 should ideally be 7-7-7
        result.Should().Equal(7, 7, 7);
    }

    [Fact]
    public void CalculateSetBreakdown_Light_15Reps_ReturnsSmallSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(15, PacingLevel.Light);

        // Assert
        result.Sum().Should().Be(15);
        result.Max().Should().BeLessOrEqualTo(7);
    }

    [Fact]
    public void CalculateSetBreakdown_Light_9Reps_ReturnsSmallSets()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(9, PacingLevel.Light);

        // Assert
        result.Sum().Should().Be(9);
        result.Max().Should().BeLessOrEqualTo(7);
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public void CalculateSetBreakdown_Light_VerySmallReps_ReturnsUnbroken()
    {
        // Act
        var result = _sut.CalculateSetBreakdown(3, PacingLevel.Light);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(3);
    }

    #endregion

    #region GenerateGuidanceText Tests

    [Fact]
    public void GenerateGuidanceText_Heavy_Unbroken_ContainsStrengthMessage()
    {
        // Arrange
        var setBreakdown = new[] { 21 };

        // Act
        var result = _sut.GenerateGuidanceText(21, PacingLevel.Heavy, setBreakdown, "Pull-Up");

        // Assert
        result.Should().Contain("unbroken");
        result.Should().Contain("strength");
    }

    [Fact]
    public void GenerateGuidanceText_Heavy_MultipleSets_ContainsPushHardMessage()
    {
        // Arrange
        var setBreakdown = new[] { 13, 8 };

        // Act
        var result = _sut.GenerateGuidanceText(21, PacingLevel.Heavy, setBreakdown, "Thruster");

        // Assert
        result.Should().Contain("Push hard");
        result.Should().Contain("13-8");
    }

    [Fact]
    public void GenerateGuidanceText_Moderate_ContainsControlledPace()
    {
        // Arrange
        var setBreakdown = new[] { 11, 10 };

        // Act
        var result = _sut.GenerateGuidanceText(21, PacingLevel.Moderate, setBreakdown, "Wall Ball");

        // Assert
        result.Should().Contain("Controlled pace");
        result.Should().Contain("11-10");
    }

    [Fact]
    public void GenerateGuidanceText_Light_ContainsConservativeMessage()
    {
        // Arrange
        var setBreakdown = new[] { 7, 7, 7 };

        // Act
        var result = _sut.GenerateGuidanceText(21, PacingLevel.Light, setBreakdown, "Thruster");

        // Assert
        result.Should().Contain("Conservative");
        result.Should().Contain("7-7-7");
    }

    [Fact]
    public void GenerateGuidanceText_ZeroReps_ReturnsDefaultMessage()
    {
        // Act
        var result = _sut.GenerateGuidanceText(0, PacingLevel.Moderate, Array.Empty<int>(), "Movement");

        // Assert
        result.Should().Contain("Complete");
        result.Should().Contain("Movement");
    }

    #endregion

    #region DetermineAthletePacingLevelAsync Tests

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_AthleteAbove80thPercentile_ReturnsHeavy()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .With(x => x.IsActive, true)
            .Create();

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Value, 140m) // Above 80th percentile (135)
            .With(x => x.IsDeleted, false)
            .Create();

        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        populationData.BenchmarkDefinitionId = benchmarkDefinition.Id;

        SetupDatabaseMocks(benchmarkDefinition, athleteBenchmark, populationData);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(
            athleteBenchmark.AthleteId,
            benchmarkDefinition.Id,
            CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Heavy);
    }

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_AthleteBetween60And80Percentile_ReturnsModerate()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .With(x => x.IsActive, true)
            .Create();

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Value, 120m) // Between 60th (110) and 80th (135) percentile
            .With(x => x.IsDeleted, false)
            .Create();

        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        populationData.BenchmarkDefinitionId = benchmarkDefinition.Id;

        SetupDatabaseMocks(benchmarkDefinition, athleteBenchmark, populationData);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(
            athleteBenchmark.AthleteId,
            benchmarkDefinition.Id,
            CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Moderate);
    }

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_AthleteBelow60thPercentile_ReturnsLight()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .With(x => x.IsActive, true)
            .Create();

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Value, 80m) // Below 60th percentile (110)
            .With(x => x.IsDeleted, false)
            .Create();

        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        populationData.BenchmarkDefinitionId = benchmarkDefinition.Id;

        SetupDatabaseMocks(benchmarkDefinition, athleteBenchmark, populationData);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(
            athleteBenchmark.AthleteId,
            benchmarkDefinition.Id,
            CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Light);
    }

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_NoBenchmarkDefinition_ReturnsModerate()
    {
        // Arrange
        var queryable = Array.Empty<BenchmarkDefinition>().AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(1, 999, CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Moderate);
    }

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_NoAthleteBenchmark_ReturnsModerate()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();

        var benchmarkQueryable = new[] { benchmarkDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(benchmarkQueryable);

        var athleteBenchmarkQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(1, benchmarkDefinition.Id, CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Moderate);
    }

    [Fact]
    public async Task DetermineAthletePacingLevelAsync_NoPopulationData_ReturnsModerate()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.BenchmarkDefinitionId, benchmarkDefinition.Id)
            .With(x => x.IsDeleted, false)
            .Create();

        var benchmarkQueryable = new[] { benchmarkDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(benchmarkQueryable);

        var athleteBenchmarkQueryable = new[] { athleteBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        var populationQueryable = Array.Empty<PopulationBenchmarkPercentile>().AsQueryable().BuildMock();
        _database.Get<PopulationBenchmarkPercentile>().Returns(populationQueryable);

        // Act
        var result = await _sut.DetermineAthletePacingLevelAsync(
            athleteBenchmark.AthleteId,
            benchmarkDefinition.Id,
            CancellationToken.None);

        // Assert
        result.Should().Be(PacingLevel.Moderate);
    }

    #endregion

    #region CalculateWorkoutPacingAsync Tests

    [Fact]
    public async Task CalculateWorkoutPacingAsync_WorkoutNotFound_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.CalculateWorkoutPacingAsync(1, 999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateWorkoutPacingAsync_DeletedWorkout_ReturnsNull()
    {
        // Arrange
        var workout = _fixture.Build<Workout>()
            .With(x => x.IsDeleted, true)
            .Create();

        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.CalculateWorkoutPacingAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateWorkoutPacingAsync_ValidWorkout_ReturnsWorkoutPacingResult()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsDeleted, false)
            .Create();

        var workoutMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 21)
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.Name, "Fran")
            .With(x => x.IsDeleted, false)
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.Movements, new List<WorkoutMovement> { workoutMovement })
            .Create();

        workoutMovement.WorkoutId = workout.Id;

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        // No benchmark mappings - will use default pacing
        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutPacingAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutId.Should().Be(workout.Id);
        result.WorkoutName.Should().Be("Fran");
        result.MovementPacing.Should().HaveCount(1);
        result.MovementPacing[0].MovementName.Should().Be("Thruster");
        result.OverallStrategyNotes.Should().NotBeNullOrEmpty();
        result.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region CalculateMovementPacingAsync Tests

    [Fact]
    public async Task CalculateMovementPacingAsync_MovementNotFound_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<MovementDefinition>().AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.CalculateMovementPacingAsync(1, 999, 21, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateMovementPacingAsync_DeletedMovement_ReturnsNull()
    {
        // Arrange
        var movement = _fixture.Build<MovementDefinition>()
            .With(x => x.IsDeleted, true)
            .Create();

        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.CalculateMovementPacingAsync(1, movement.Id, 21, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateMovementPacingAsync_NoBenchmarkMapping_ReturnsModeratePacing()
    {
        // Arrange
        var movement = _fixture.Build<MovementDefinition>()
            .With(x => x.IsDeleted, false)
            .With(x => x.DisplayName, "Box Jump")
            .Create();

        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateMovementPacingAsync(1, movement.Id, 21, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PacingLevel.Should().Be("Moderate");
        result.HasPopulationData.Should().BeFalse();
        result.HasAthleteBenchmark.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateMovementPacingAsync_ValidMapping_ReturnsCorrectPacing()
    {
        // Arrange
        var benchmarkDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Name, "Back Squat 1RM")
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .With(x => x.IsActive, true)
            .Create();

        var movement = _fixture.Build<MovementDefinition>()
            .With(x => x.IsDeleted, false)
            .With(x => x.DisplayName, "Thruster")
            .Create();

        var mapping = _fixture.Build<BenchmarkMovementMapping>()
            .With(x => x.BenchmarkDefinitionId, benchmarkDefinition.Id)
            .With(x => x.MovementDefinitionId, movement.Id)
            .With(x => x.RelevanceFactor, 1.0m)
            .With(x => x.BenchmarkDefinition, benchmarkDefinition)
            .Create();

        var populationData = CreatePopulationData(70m, 90m, 110m, 135m, 170m);
        populationData.BenchmarkDefinitionId = benchmarkDefinition.Id;

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, 1)
            .With(x => x.BenchmarkDefinitionId, benchmarkDefinition.Id)
            .With(x => x.Value, 140m) // Above 80th percentile
            .With(x => x.IsDeleted, false)
            .Create();

        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        var mappingQueryable = new[] { mapping }.AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        var athleteBenchmarkQueryable = new[] { athleteBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        var populationQueryable = new[] { populationData }.AsQueryable().BuildMock();
        _database.Get<PopulationBenchmarkPercentile>().Returns(populationQueryable);

        // Act
        var result = await _sut.CalculateMovementPacingAsync(1, movement.Id, 21, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MovementName.Should().Be("Thruster");
        result.PacingLevel.Should().Be("Heavy");
        result.AthletePercentile.Should().BeGreaterThan(80);
        result.HasPopulationData.Should().BeTrue();
        result.HasAthleteBenchmark.Should().BeTrue();
        result.BenchmarkUsed.Should().Be("Back Squat 1RM");
        result.RecommendedSets.Should().NotBeEmpty();
        result.GuidanceText.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region CalculateCurrentUserWorkoutPacingAsync Tests

    [Fact]
    public async Task CalculateCurrentUserWorkoutPacingAsync_NoAthleteProfile_ReturnsNull()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.CalculateCurrentUserWorkoutPacingAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateCurrentUserWorkoutPacingAsync_ValidAthlete_CallsCalculateWorkoutPacing()
    {
        // Arrange
        var athleteDto = new AthleteDto { Id = 5 };
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        var workoutQueryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        // Act
        var result = await _sut.CalculateCurrentUserWorkoutPacingAsync(1, CancellationToken.None);

        // Assert - Returns null because workout not found, but verifies flow
        result.Should().BeNull();
        await _athleteService.Received(1).GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Helper Methods

    private PopulationBenchmarkPercentile CreatePopulationData(
        decimal p20, decimal p40, decimal p60, decimal p80, decimal p95)
    {
        return new PopulationBenchmarkPercentile
        {
            Id = _fixture.Create<int>(),
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Percentile20 = p20,
            Percentile40 = p40,
            Percentile60 = p60,
            Percentile80 = p80,
            Percentile95 = p95,
            Gender = "Male",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    private void SetupDatabaseMocks(
        BenchmarkDefinition benchmarkDefinition,
        AthleteBenchmark athleteBenchmark,
        PopulationBenchmarkPercentile populationData)
    {
        athleteBenchmark.BenchmarkDefinitionId = benchmarkDefinition.Id;

        var benchmarkQueryable = new[] { benchmarkDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(benchmarkQueryable);

        var athleteBenchmarkQueryable = new[] { athleteBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        var populationQueryable = new[] { populationData }.AsQueryable().BuildMock();
        _database.Get<PopulationBenchmarkPercentile>().Returns(populationQueryable);
    }

    #endregion
}
