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
/// Unit tests for VolumeLoadService.
/// </summary>
public class VolumeLoadServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly VolumeLoadService _sut;

    public VolumeLoadServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new VolumeLoadCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _athleteService = Substitute.For<IAthleteService>();
        _sut = new VolumeLoadService(_database, _athleteService);
    }

    #region CalculateVolumeLoad Tests (Pure Calculation)

    [Fact]
    public void CalculateVolumeLoad_ValidInputs_ReturnsCorrectVolume()
    {
        // Arrange
        decimal weight = 43m;
        int reps = 45;
        int rounds = 1;

        // Act
        var result = _sut.CalculateVolumeLoad(weight, reps, rounds);

        // Assert - 43 x 45 x 1 = 1935 kg (Fran Thrusters)
        result.Should().Be(1935m);
    }

    [Fact]
    public void CalculateVolumeLoad_MultipleRounds_ReturnsCorrectVolume()
    {
        // Arrange
        decimal weight = 70m;
        int reps = 12;
        int rounds = 5;

        // Act
        var result = _sut.CalculateVolumeLoad(weight, reps, rounds);

        // Assert - 70 x 12 x 5 = 4200 kg (DT Deadlifts)
        result.Should().Be(4200m);
    }

    [Fact]
    public void CalculateVolumeLoad_ZeroWeight_ReturnsZero()
    {
        // Act
        var result = _sut.CalculateVolumeLoad(0m, 21, 3);

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateVolumeLoad_ZeroReps_ReturnsZero()
    {
        // Act
        var result = _sut.CalculateVolumeLoad(43m, 0, 3);

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateVolumeLoad_ZeroRounds_ReturnsZero()
    {
        // Act
        var result = _sut.CalculateVolumeLoad(43m, 21, 0);

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateVolumeLoad_NegativeWeight_ReturnsZero()
    {
        // Act
        var result = _sut.CalculateVolumeLoad(-10m, 21, 3);

        // Assert
        result.Should().Be(0m);
    }

    #endregion

    #region ClassifyLoad Tests

    [Fact]
    public void ClassifyLoad_Intermediate_75PercentOf1RM_ReturnsHigh()
    {
        // Act
        var result = _sut.ClassifyLoad(75m, 100m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("High");
    }

    [Fact]
    public void ClassifyLoad_Intermediate_70PercentOf1RM_ReturnsHigh()
    {
        // Act
        var result = _sut.ClassifyLoad(70m, 100m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("High");
    }

    [Fact]
    public void ClassifyLoad_Intermediate_60PercentOf1RM_ReturnsModerate()
    {
        // Act
        var result = _sut.ClassifyLoad(60m, 100m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("Moderate");
    }

    [Fact]
    public void ClassifyLoad_Intermediate_50PercentOf1RM_ReturnsModerate()
    {
        // Act
        var result = _sut.ClassifyLoad(50m, 100m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("Moderate");
    }

    [Fact]
    public void ClassifyLoad_Intermediate_40PercentOf1RM_ReturnsLow()
    {
        // Act
        var result = _sut.ClassifyLoad(40m, 100m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("Low");
    }

    [Fact]
    public void ClassifyLoad_Beginner_65PercentOf1RM_ReturnsHigh()
    {
        // Act - 65% for beginner = High (threshold 65%)
        var result = _sut.ClassifyLoad(65m, 100m, ExperienceLevel.Beginner);

        // Assert
        result.Should().Be("High");
    }

    [Fact]
    public void ClassifyLoad_Beginner_60PercentOf1RM_ReturnsModerate()
    {
        // Act
        var result = _sut.ClassifyLoad(60m, 100m, ExperienceLevel.Beginner);

        // Assert
        result.Should().Be("Moderate");
    }

    [Fact]
    public void ClassifyLoad_Beginner_44PercentOf1RM_ReturnsLow()
    {
        // Act - 44% for beginner = Low (threshold 45%)
        var result = _sut.ClassifyLoad(44m, 100m, ExperienceLevel.Beginner);

        // Assert
        result.Should().Be("Low");
    }

    [Fact]
    public void ClassifyLoad_Advanced_80PercentOf1RM_ReturnsHigh()
    {
        // Act - 80% for advanced = High (threshold 75%)
        var result = _sut.ClassifyLoad(80m, 100m, ExperienceLevel.Advanced);

        // Assert
        result.Should().Be("High");
    }

    [Fact]
    public void ClassifyLoad_Advanced_70PercentOf1RM_ReturnsModerate()
    {
        // Act
        var result = _sut.ClassifyLoad(70m, 100m, ExperienceLevel.Advanced);

        // Assert
        result.Should().Be("Moderate");
    }

    [Fact]
    public void ClassifyLoad_Advanced_54PercentOf1RM_ReturnsLow()
    {
        // Act - 54% for advanced = Low (threshold 55%)
        var result = _sut.ClassifyLoad(54m, 100m, ExperienceLevel.Advanced);

        // Assert
        result.Should().Be("Low");
    }

    [Fact]
    public void ClassifyLoad_ZeroAthlete1RM_ReturnsModerateDefault()
    {
        // Act
        var result = _sut.ClassifyLoad(43m, 0m, ExperienceLevel.Intermediate);

        // Assert
        result.Should().Be("Moderate");
    }

    #endregion

    #region CalculateRecommendedWeight Tests

    [Fact]
    public void CalculateRecommendedWeight_HighLoadLowPercentile_Returns80Percent()
    {
        // Arrange - Athlete at 50th percentile with High load
        decimal rxWeight = 43m;
        decimal percentile = 50m;

        // Act
        var result = _sut.CalculateRecommendedWeight(rxWeight, percentile, "High");

        // Assert - Should recommend 80% = 34.4
        result.Should().Be(34.4m);
    }

    [Fact]
    public void CalculateRecommendedWeight_HighLoadMediumPercentile_Returns90Percent()
    {
        // Arrange - Athlete at 70th percentile with High load
        decimal rxWeight = 43m;
        decimal percentile = 70m;

        // Act
        var result = _sut.CalculateRecommendedWeight(rxWeight, percentile, "High");

        // Assert - Should recommend 90% = 38.7
        result.Should().Be(38.7m);
    }

    [Fact]
    public void CalculateRecommendedWeight_HighLoadHighPercentile_ReturnsNull()
    {
        // Arrange - Athlete at 85th percentile with High load
        decimal rxWeight = 43m;
        decimal percentile = 85m;

        // Act
        var result = _sut.CalculateRecommendedWeight(rxWeight, percentile, "High");

        // Assert - RX is appropriate
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateRecommendedWeight_ModerateLoad_ReturnsNull()
    {
        // Arrange
        decimal rxWeight = 43m;
        decimal percentile = 40m;

        // Act
        var result = _sut.CalculateRecommendedWeight(rxWeight, percentile, "Moderate");

        // Assert - No scaling for moderate load
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateRecommendedWeight_LowLoad_ReturnsNull()
    {
        // Arrange
        decimal rxWeight = 43m;
        decimal percentile = 40m;

        // Act
        var result = _sut.CalculateRecommendedWeight(rxWeight, percentile, "Low");

        // Assert - No scaling for low load
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateRecommendedWeight_NullPercentile_ReturnsNull()
    {
        // Act
        var result = _sut.CalculateRecommendedWeight(43m, null, "High");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GenerateTip Tests

    [Fact]
    public void GenerateTip_HighLoadLowPercentile_SuggestsScaling()
    {
        // Arrange
        string classification = "High";
        decimal percentile = 50m;
        decimal rxWeight = 43m;

        // Act
        var result = _sut.GenerateTip(classification, percentile, rxWeight, "Thruster");

        // Assert
        result.Should().Contain("scaling");
        result.Should().Contain("Thruster");
        result.Should().Contain("80%");
    }

    [Fact]
    public void GenerateTip_HighLoadHighPercentile_SuggestsGoRx()
    {
        // Arrange
        string classification = "High";
        decimal percentile = 85m;
        decimal rxWeight = 43m;

        // Act
        var result = _sut.GenerateTip(classification, percentile, rxWeight, "Thruster");

        // Assert
        result.Should().Contain("Go RX");
        result.Should().Contain("80th percentile");
    }

    [Fact]
    public void GenerateTip_ModerateLoad_SuggestsConsistentPacing()
    {
        // Arrange
        string classification = "Moderate";
        decimal percentile = 60m;
        decimal rxWeight = 43m;

        // Act
        var result = _sut.GenerateTip(classification, percentile, rxWeight, "Thruster");

        // Assert
        result.Should().Contain("moderate");
        result.Should().Contain("pacing");
    }

    [Fact]
    public void GenerateTip_LowLoad_SuggestsPushPace()
    {
        // Arrange
        string classification = "Low";
        decimal percentile = 70m;
        decimal rxWeight = 30m;

        // Act
        var result = _sut.GenerateTip(classification, percentile, rxWeight, "Thruster");

        // Assert
        result.Should().Contain("light");
        result.Should().Contain("push the pace");
    }

    [Fact]
    public void GenerateTip_NullPercentile_SuggestsRecordBenchmark()
    {
        // Act
        var result = _sut.GenerateTip("Moderate", null, 43m, "Thruster");

        // Assert
        result.Should().Contain("benchmark");
        result.Should().Contain("Thruster");
    }

    #endregion

    #region FormatVolumeLoad Tests

    [Fact]
    public void FormatVolumeLoad_SmallValue_FormatsCorrectly()
    {
        // Act
        var result = _sut.FormatVolumeLoad(500m, "kg");

        // Assert
        result.Should().Be("500 kg");
    }

    [Fact]
    public void FormatVolumeLoad_LargeValue_IncludesThousandsSeparator()
    {
        // Act
        var result = _sut.FormatVolumeLoad(2150m, "kg");

        // Assert
        // Note: Culture-specific - may be "2,150 kg" or "2.150 kg"
        result.Should().Contain("150");
        result.Should().Contain("kg");
    }

    [Fact]
    public void FormatVolumeLoad_Pounds_UsesCorrectUnit()
    {
        // Act
        var result = _sut.FormatVolumeLoad(4500m, "lb");

        // Assert
        result.Should().Contain("lb");
    }

    #endregion

    #region CalculateWorkoutVolumeLoadAsync Tests

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_WorkoutNotFound_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, 999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_DeletedWorkout_ReturnsNull()
    {
        // Arrange
        var workout = _fixture.Build<Workout>()
            .With(x => x.IsDeleted, true)
            .Create();

        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_ValidWorkout_ReturnsVolumeLoadResult()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsBodyweight, false)
            .With(x => x.IsDeleted, false)
            .Create();

        var workoutMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45) // 21-15-9 = 45 total
            .With(x => x.LoadValue, 43m)
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.Name, "Fran")
            .With(x => x.IsDeleted, false)
            .With(x => x.RoundCount, 1)
            .With(x => x.Movements, new List<WorkoutMovement> { workoutMovement })
            .Create();

        workoutMovement.WorkoutId = workout.Id;

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        // No benchmark mappings - will return default classification
        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutId.Should().Be(workout.Id);
        result.WorkoutName.Should().Be("Fran");
        result.MovementVolumes.Should().HaveCount(1);
        result.MovementVolumes[0].MovementName.Should().Be("Thruster");
        result.MovementVolumes[0].VolumeLoad.Should().Be(1935m); // 43 x 45 x 1
        result.TotalVolumeLoad.Should().Be(1935m);
        result.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_MultipleMovements_SumsTotalVolume()
    {
        // Arrange
        var thrusterDef = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsBodyweight, false)
            .Create();

        var pullUpDef = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Pull-Up")
            .With(x => x.IsBodyweight, true) // Bodyweight movement
            .Create();

        var thrusterMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45)
            .With(x => x.LoadValue, 43m)
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MovementDefinition, thrusterDef)
            .Create();

        var pullUpMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45)
            .With(x => x.LoadValue, (decimal?)null) // No load for bodyweight
            .With(x => x.SequenceOrder, 2)
            .With(x => x.MovementDefinition, pullUpDef)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.Name, "Fran")
            .With(x => x.IsDeleted, false)
            .With(x => x.RoundCount, 1)
            .With(x => x.Movements, new List<WorkoutMovement> { thrusterMovement, pullUpMovement })
            .Create();

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MovementVolumes.Should().HaveCount(2);

        // Thruster has volume
        result.MovementVolumes[0].VolumeLoad.Should().Be(1935m);

        // Pull-Up is bodyweight - zero volume
        result.MovementVolumes[1].VolumeLoad.Should().Be(0m);
        result.MovementVolumes[1].LoadClassification.Should().Be("Bodyweight");

        // Total only includes weighted movements
        result.TotalVolumeLoad.Should().Be(1935m);
    }

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_WithRounds_MultipliesCorrectly()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Deadlift")
            .With(x => x.IsBodyweight, false)
            .Create();

        var workoutMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 12)
            .With(x => x.LoadValue, 70m)
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.SequenceOrder, 1)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.Name, "DT")
            .With(x => x.IsDeleted, false)
            .With(x => x.RoundCount, 5) // 5 rounds
            .With(x => x.Movements, new List<WorkoutMovement> { workoutMovement })
            .Create();

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MovementVolumes[0].VolumeLoad.Should().Be(4200m); // 70 x 12 x 5
        result.MovementVolumes[0].Rounds.Should().Be(5);
    }

    #endregion

    #region CalculateMovementVolumeLoadAsync Tests

    [Fact]
    public async Task CalculateMovementVolumeLoadAsync_BodyweightMovement_ReturnsBodyweightClassification()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Pull-Up")
            .With(x => x.IsBodyweight, true)
            .Create();

        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45)
            .With(x => x.LoadValue, (decimal?)null)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        // Act
        var result = await _sut.CalculateMovementVolumeLoadAsync(1, movement, 1, CancellationToken.None);

        // Assert
        result.LoadClassification.Should().Be("Bodyweight");
        result.VolumeLoad.Should().Be(0m);
        result.Tip.Should().Contain("movement efficiency");
    }

    [Fact]
    public async Task CalculateMovementVolumeLoadAsync_NoBenchmarkMapping_ReturnsNoMappingTip()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Uncommon Movement")
            .With(x => x.IsBodyweight, false)
            .Create();

        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 10)
            .With(x => x.LoadValue, 50m)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateMovementVolumeLoadAsync(1, movement, 1, CancellationToken.None);

        // Assert
        result.VolumeLoad.Should().Be(500m); // 50 x 10 x 1
        result.BenchmarkUsed.Should().Be("None");
        result.HasSufficientData.Should().BeFalse();
        result.Tip.Should().Contain("No benchmark mapping");
    }

    [Fact]
    public async Task CalculateMovementVolumeLoadAsync_NoAthleteBenchmark_ReturnsRecordBenchmarkTip()
    {
        // Arrange
        var benchmarkDef = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Name, "Back Squat 1RM")
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .Create();

        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsBodyweight, false)
            .Create();

        var mapping = _fixture.Build<BenchmarkMovementMapping>()
            .With(x => x.MovementDefinitionId, movementDefinition.Id)
            .With(x => x.BenchmarkDefinitionId, benchmarkDef.Id)
            .With(x => x.BenchmarkDefinition, benchmarkDef)
            .With(x => x.RelevanceFactor, 1.0m)
            .Create();

        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45)
            .With(x => x.LoadValue, 43m)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var mappingQueryable = new[] { mapping }.AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        var athleteBenchmarkQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        // Act
        var result = await _sut.CalculateMovementVolumeLoadAsync(1, movement, 1, CancellationToken.None);

        // Assert
        result.BenchmarkUsed.Should().Be("Back Squat 1RM");
        result.HasSufficientData.Should().BeFalse();
        result.Tip.Should().Contain("Record your Back Squat 1RM");
    }

    [Fact]
    public async Task CalculateMovementVolumeLoadAsync_FullData_ReturnsCompleteAnalysis()
    {
        // Arrange
        var benchmarkDef = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Name, "Back Squat 1RM")
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .Create();

        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsBodyweight, false)
            .Create();

        var mapping = _fixture.Build<BenchmarkMovementMapping>()
            .With(x => x.MovementDefinitionId, movementDefinition.Id)
            .With(x => x.BenchmarkDefinitionId, benchmarkDef.Id)
            .With(x => x.BenchmarkDefinition, benchmarkDef)
            .With(x => x.RelevanceFactor, 1.0m)
            .Create();

        var athleteBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, 1)
            .With(x => x.BenchmarkDefinitionId, benchmarkDef.Id)
            .With(x => x.Value, 80m) // 43 is ~54% of 80 = Moderate
            .With(x => x.IsDeleted, false)
            .Create();

        var athlete = _fixture.Build<Athlete>()
            .With(x => x.Id, 1)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.Gender, "Male")
            .With(x => x.IsDeleted, false)
            .Create();

        var populationData = new PopulationBenchmarkPercentile
        {
            BenchmarkDefinitionId = benchmarkDef.Id,
            Percentile20 = 60m,
            Percentile40 = 70m,
            Percentile60 = 80m,
            Percentile80 = 100m,
            Percentile95 = 130m,
            Gender = "Male"
        };

        var movement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 45)
            .With(x => x.LoadValue, 43m)
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var mappingQueryable = new[] { mapping }.AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        var athleteBenchmarkQueryable = new[] { athleteBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(athleteBenchmarkQueryable);

        var athleteQueryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(athleteQueryable);

        var populationQueryable = new[] { populationData }.AsQueryable().BuildMock();
        _database.Get<PopulationBenchmarkPercentile>().Returns(populationQueryable);

        // Act
        var result = await _sut.CalculateMovementVolumeLoadAsync(1, movement, 1, CancellationToken.None);

        // Assert
        result.VolumeLoad.Should().Be(1935m);
        result.BenchmarkUsed.Should().Be("Back Squat 1RM");
        result.HasSufficientData.Should().BeTrue();
        result.AthleteBenchmarkPercentile.Should().BeApproximately(60m, 1m); // Athlete is at 60th percentile
        result.LoadClassification.Should().Be("Moderate"); // 43/80 = 53.75%
    }

    #endregion

    #region CalculateCurrentUserWorkoutVolumeLoadAsync Tests

    [Fact]
    public async Task CalculateCurrentUserWorkoutVolumeLoadAsync_NoAthleteProfile_ReturnsNull()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.CalculateCurrentUserWorkoutVolumeLoadAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateCurrentUserWorkoutVolumeLoadAsync_ValidAthlete_CallsCalculateWorkoutVolumeLoad()
    {
        // Arrange
        var athleteDto = new AthleteDto { Id = 5 };
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        var workoutQueryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        // Act
        var result = await _sut.CalculateCurrentUserWorkoutVolumeLoadAsync(1, CancellationToken.None);

        // Assert - Returns null because workout not found, but verifies flow
        result.Should().BeNull();
        await _athleteService.Received(1).GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Edge Cases and Integration

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_NullRoundCount_DefaultsToOne()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.IsBodyweight, false)
            .Create();

        var workoutMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 21)
            .With(x => x.LoadValue, 43m)
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.IsDeleted, false)
            .With(x => x.RoundCount, (int?)null) // Null round count
            .With(x => x.Movements, new List<WorkoutMovement> { workoutMovement })
            .Create();

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MovementVolumes[0].Rounds.Should().Be(1);
        result.MovementVolumes[0].VolumeLoad.Should().Be(903m); // 43 x 21 x 1
    }

    [Fact]
    public async Task CalculateWorkoutVolumeLoadAsync_ZeroLoadValue_ReturnsZeroVolume()
    {
        // Arrange
        var movementDefinition = _fixture.Build<MovementDefinition>()
            .With(x => x.DisplayName, "Air Squat")
            .With(x => x.IsBodyweight, false) // Not marked as bodyweight but has no load
            .Create();

        var workoutMovement = _fixture.Build<WorkoutMovement>()
            .With(x => x.RepCount, 50)
            .With(x => x.LoadValue, 0m) // Zero load
            .With(x => x.MovementDefinition, movementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(x => x.IsDeleted, false)
            .With(x => x.RoundCount, 3)
            .With(x => x.Movements, new List<WorkoutMovement> { workoutMovement })
            .Create();

        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(workoutQueryable);

        var mappingQueryable = Array.Empty<BenchmarkMovementMapping>().AsQueryable().BuildMock();
        _database.Get<BenchmarkMovementMapping>().Returns(mappingQueryable);

        // Act
        var result = await _sut.CalculateWorkoutVolumeLoadAsync(1, workout.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MovementVolumes[0].VolumeLoad.Should().Be(0m);
    }

    #endregion
}
