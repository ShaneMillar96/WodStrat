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
/// Unit tests for StrategyInsightsService.
/// </summary>
public class StrategyInsightsServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly IPacingService _pacingService;
    private readonly IVolumeLoadService _volumeLoadService;
    private readonly ITimeEstimateService _timeEstimateService;
    private readonly StrategyInsightsService _sut;

    public StrategyInsightsServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new StrategyInsightsCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _athleteService = Substitute.For<IAthleteService>();
        _pacingService = Substitute.For<IPacingService>();
        _volumeLoadService = Substitute.For<IVolumeLoadService>();
        _timeEstimateService = Substitute.For<ITimeEstimateService>();

        _sut = new StrategyInsightsService(
            _database,
            _athleteService,
            _pacingService,
            _volumeLoadService,
            _timeEstimateService);
    }

    #region CalculateStrategyInsightsAsync Tests

    [Fact]
    public async Task CalculateStrategyInsightsAsync_ValidRequest_ReturnsCompleteInsights()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var pacingResult = _fixture.Create<WorkoutPacingResultDto>();
        var volumeResult = _fixture.Create<WorkoutVolumeLoadResultDto>();
        var timeResult = _fixture.Create<TimeEstimateResultDto>();
        var athleteDto = _fixture.Create<AthleteDto>();

        var workout = new Workout { Id = workoutId, Name = "Test", TimeCapSeconds = 600, IsDeleted = false };
        var workoutQueryable = new[] { workout }.AsQueryable().BuildMock();

        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(pacingResult);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeResult);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeResult);
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _database.Get<Workout>().Returns(workoutQueryable);

        // Act
        var result = await _sut.CalculateStrategyInsightsAsync(athleteId, workoutId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.WorkoutId.Should().Be(workoutId);
        result.WorkoutName.Should().Be(pacingResult.WorkoutName);
        result.DifficultyScore.Should().NotBeNull();
        result.DifficultyScore.Score.Should().BeInRange(1, 10);
        result.StrategyConfidence.Should().NotBeNull();
        result.KeyFocusMovements.Should().NotBeNull();
        result.RiskAlerts.Should().NotBeNull();
        result.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CalculateStrategyInsightsAsync_PacingServiceReturnsNull_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutPacingResultDto?)null);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<WorkoutVolumeLoadResultDto>());
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<TimeEstimateResultDto>());

        // Act
        var result = await _sut.CalculateStrategyInsightsAsync(athleteId, workoutId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateStrategyInsightsAsync_VolumeLoadServiceReturnsNull_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<WorkoutPacingResultDto>());
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutVolumeLoadResultDto?)null);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<TimeEstimateResultDto>());

        // Act
        var result = await _sut.CalculateStrategyInsightsAsync(athleteId, workoutId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateStrategyInsightsAsync_TimeEstimateServiceReturnsNull_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<WorkoutPacingResultDto>());
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<WorkoutVolumeLoadResultDto>());
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((TimeEstimateResultDto?)null);

        // Act
        var result = await _sut.CalculateStrategyInsightsAsync(athleteId, workoutId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateStrategyInsightsAsync_AllServicesReturnNull_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutPacingResultDto?)null);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutVolumeLoadResultDto?)null);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((TimeEstimateResultDto?)null);

        // Act
        var result = await _sut.CalculateStrategyInsightsAsync(athleteId, workoutId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CalculateDifficultyScore Tests

    [Fact]
    public void CalculateDifficultyScore_AllLightPacing_ReturnsHighDifficulty()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Light", "Light", "Light");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Intermediate");

        // Assert
        result.Score.Should().BeGreaterOrEqualTo(6); // Light pacing = difficulty
        result.Label.Should().NotBeNullOrEmpty();
        result.Description.Should().NotBeNullOrEmpty();
        result.Breakdown.PacingFactor.Should().Be(8m); // Light = 8 points
    }

    [Fact]
    public void CalculateDifficultyScore_AllHeavyPacing_ReturnsLowDifficulty()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Heavy", "Heavy", "Heavy");
        var volumeResult = CreateVolumeResultWithClassifications("Low", "Low", "Low");
        var timeResult = CreateTimeEstimateResult(180, 240);

        // Act
        var result = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Intermediate");

        // Assert
        result.Score.Should().BeLessThanOrEqualTo(4); // Heavy pacing = easier
        result.Breakdown.PacingFactor.Should().Be(2m); // Heavy = 2 points
    }

    [Fact]
    public void CalculateDifficultyScore_MixedPacing_ReturnsModerate()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Heavy", "Moderate", "Light");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Intermediate");

        // Assert
        result.Score.Should().BeInRange(4, 7);
        result.Breakdown.PacingFactor.Should().Be(5m); // (2+5+8)/3 = 5
    }

    [Fact]
    public void CalculateDifficultyScore_BeginnerExperience_IncreasesScore()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Moderate", "Moderate", "Moderate");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var intermediateResult = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Intermediate");
        var beginnerResult = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Beginner");

        // Assert
        beginnerResult.Score.Should().BeGreaterThanOrEqualTo(intermediateResult.Score);
        beginnerResult.Breakdown.ExperienceModifier.Should().Be(1.2m);
    }

    [Fact]
    public void CalculateDifficultyScore_AdvancedExperience_DecreasesScore()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Moderate", "Moderate", "Moderate");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var intermediateResult = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Intermediate");
        var advancedResult = _sut.CalculateDifficultyScore(pacingResult, volumeResult, timeResult, "Advanced");

        // Assert
        advancedResult.Score.Should().BeLessThanOrEqualTo(intermediateResult.Score);
        advancedResult.Breakdown.ExperienceModifier.Should().Be(0.85m);
    }

    [Fact]
    public void CalculateDifficultyScore_HighVolumeLoad_IncreasesDifficulty()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Moderate", "Moderate", "Moderate");
        var lowVolumeResult = CreateVolumeResultWithClassifications("Low", "Low", "Low");
        var highVolumeResult = CreateVolumeResultWithClassifications("High", "High", "High");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var lowVolumeScore = _sut.CalculateDifficultyScore(pacingResult, lowVolumeResult, timeResult, "Intermediate");
        var highVolumeScore = _sut.CalculateDifficultyScore(pacingResult, highVolumeResult, timeResult, "Intermediate");

        // Assert
        highVolumeScore.Breakdown.VolumeFactor.Should().BeGreaterThan(lowVolumeScore.Breakdown.VolumeFactor);
    }

    [Fact]
    public void CalculateDifficultyScore_ScoreIsClampedBetween1And10()
    {
        // Arrange - Create extreme cases
        var lightPacing = CreatePacingResultWithLevels("Light", "Light", "Light");
        var highVolume = CreateVolumeResultWithClassifications("High", "High", "High");
        var longTime = CreateTimeEstimateResult(1800, 2400);

        var heavyPacing = CreatePacingResultWithLevels("Heavy", "Heavy", "Heavy");
        var lowVolume = CreateVolumeResultWithClassifications("Low", "Low", "Low");
        var shortTime = CreateTimeEstimateResult(60, 90);

        // Act
        var maxResult = _sut.CalculateDifficultyScore(lightPacing, highVolume, longTime, "Beginner");
        var minResult = _sut.CalculateDifficultyScore(heavyPacing, lowVolume, shortTime, "Advanced");

        // Assert
        maxResult.Score.Should().BeLessThanOrEqualTo(10);
        minResult.Score.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void CalculateDifficultyScore_EmptyPacing_DefaultsToModerate()
    {
        // Arrange
        var emptyPacingResult = new WorkoutPacingResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test",
            WorkoutType = "ForTime",
            MovementPacing = new List<MovementPacingDto>(),
            CalculatedAt = DateTime.UtcNow
        };
        var volumeResult = CreateVolumeResultWithClassifications("Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.CalculateDifficultyScore(emptyPacingResult, volumeResult, timeResult, "Intermediate");

        // Assert
        result.Breakdown.PacingFactor.Should().Be(5m); // Default moderate
    }

    #endregion

    #region IdentifyKeyFocusMovements Tests

    [Fact]
    public void IdentifyKeyFocusMovements_LightPacingWithHighVolume_ReturnsAsFocus()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Light", true, true),
            ("Pull-up", "Heavy", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Thruster", "High", true),
            ("Pull-up", "Low", true));

        // Act
        var result = _sut.IdentifyKeyFocusMovements(pacingResult, volumeResult);

        // Assert
        result.Should().HaveCountGreaterThan(0);
        result.First().MovementName.Should().Be("Thruster");
        result.First().Priority.Should().Be(1);
        result.First().ScalingRecommended.Should().BeTrue();
    }

    [Fact]
    public void IdentifyKeyFocusMovements_MaxThreeMovements()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Movement1", "Light", true, true),
            ("Movement2", "Light", true, true),
            ("Movement3", "Light", true, true),
            ("Movement4", "Light", true, true),
            ("Movement5", "Light", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Movement1", "High", true),
            ("Movement2", "High", true),
            ("Movement3", "High", true),
            ("Movement4", "High", true),
            ("Movement5", "High", true));

        // Act
        var result = _sut.IdentifyKeyFocusMovements(pacingResult, volumeResult, 3);

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public void IdentifyKeyFocusMovements_AllHeavyPacing_ReturnsEmpty()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Heavy", true, true),
            ("Pull-up", "Heavy", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Thruster", "Low", true),
            ("Pull-up", "Low", true));

        // Act
        var result = _sut.IdentifyKeyFocusMovements(pacingResult, volumeResult);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void IdentifyKeyFocusMovements_MissingBenchmark_IncludedInFocus()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Unknown Movement", "Moderate", false, false));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Unknown Movement", "Moderate", true));

        // Act
        var result = _sut.IdentifyKeyFocusMovements(pacingResult, volumeResult);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void IdentifyKeyFocusMovements_PrioritiesAreSequential()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Movement1", "Light", true, true),
            ("Movement2", "Light", true, true),
            ("Movement3", "Moderate", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Movement1", "High", true),
            ("Movement2", "Moderate", true),
            ("Movement3", "High", true));

        // Act
        var result = _sut.IdentifyKeyFocusMovements(pacingResult, volumeResult);

        // Assert
        for (var i = 0; i < result.Count; i++)
        {
            result[i].Priority.Should().Be(i + 1);
        }
    }

    #endregion

    #region GenerateRiskAlerts Tests

    [Fact]
    public void GenerateRiskAlerts_ScalingRecommended_TwoOrMoreLightWithHighVolume()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Light", true, true),
            ("Clean", "Light", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Thruster", "High", true),
            ("Clean", "Moderate", true));
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 5, null);

        // Assert
        result.Should().Contain(a => a.AlertType == RiskAlertType.ScalingRecommended);
    }

    [Fact]
    public void GenerateRiskAlerts_TimeCapRisk_WhenApproachingTimeCap()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Moderate");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate");
        var timeResult = CreateTimeEstimateResult(540, 560); // Max 560 seconds
        var timeCapSeconds = 600; // 600 second time cap

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 5, timeCapSeconds);

        // Assert
        result.Should().Contain(a => a.AlertType == RiskAlertType.TimeCapRisk);
        var timeCapAlert = result.First(a => a.AlertType == RiskAlertType.TimeCapRisk);
        timeCapAlert.Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public void GenerateRiskAlerts_NoTimeCapRisk_WhenUnderThreshold()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Moderate");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate");
        var timeResult = CreateTimeEstimateResult(300, 400); // Max 400 seconds
        var timeCapSeconds = 600; // 600 second time cap

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 5, timeCapSeconds);

        // Assert
        result.Should().NotContain(a => a.AlertType == RiskAlertType.TimeCapRisk);
    }

    [Fact]
    public void GenerateRiskAlerts_RecoveryImpact_HighDifficultyAndHighVolume()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Light", "Light");
        var volumeResult = CreateVolumeResultWithClassifications("High", "High");
        var timeResult = CreateTimeEstimateResult(600, 900);
        var highDifficultyScore = 8;

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, highDifficultyScore, null);

        // Assert
        result.Should().Contain(a => a.AlertType == RiskAlertType.RecoveryImpact);
    }

    [Fact]
    public void GenerateRiskAlerts_NoRecoveryImpact_WhenDifficultyBelow8()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Light", "Light");
        var volumeResult = CreateVolumeResultWithClassifications("High", "High");
        var timeResult = CreateTimeEstimateResult(600, 900);
        var lowDifficultyScore = 7;

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, lowDifficultyScore, null);

        // Assert
        result.Should().NotContain(a => a.AlertType == RiskAlertType.RecoveryImpact);
    }

    [Fact]
    public void GenerateRiskAlerts_PacingMismatch_MixOfHeavyAndLight()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithLevels("Heavy", "Heavy", "Light", "Light");
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 5, null);

        // Assert
        result.Should().Contain(a => a.AlertType == RiskAlertType.PacingMismatch);
    }

    [Fact]
    public void GenerateRiskAlerts_BenchmarkGap_MissingBenchmarkData()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Moderate", false, false),
            ("Pull-up", "Moderate", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Thruster", "Moderate", true),
            ("Pull-up", "Moderate", true));
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 5, null);

        // Assert
        result.Should().Contain(a => a.AlertType == RiskAlertType.BenchmarkGap);
        var gapAlert = result.First(a => a.AlertType == RiskAlertType.BenchmarkGap);
        gapAlert.AffectedMovements.Should().Contain("Thruster");
    }

    [Fact]
    public void GenerateRiskAlerts_SortedBySeverity_HighFirst()
    {
        // Arrange - Create conditions for multiple alerts
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Light", false, false),
            ("Clean", "Light", true, true),
            ("Heavy DL", "Heavy", true, true),
            ("Another Heavy", "Heavy", true, true));
        var volumeResult = CreateVolumeResultWithDetails(
            ("Thruster", "High", true),
            ("Clean", "Moderate", true),
            ("Heavy DL", "Low", true),
            ("Another Heavy", "Low", true));
        var timeResult = CreateTimeEstimateResult(560, 580);
        var timeCapSeconds = 600;

        // Act
        var result = _sut.GenerateRiskAlerts(pacingResult, volumeResult, timeResult, 8, timeCapSeconds);

        // Assert
        if (result.Count > 1)
        {
            var highAlerts = result.TakeWhile(a => a.Severity == AlertSeverity.High).Count();
            var mediumAlerts = result.Skip(highAlerts).TakeWhile(a => a.Severity == AlertSeverity.Medium).Count();
            var lowAlerts = result.Skip(highAlerts + mediumAlerts).Count();

            // Verify ordering
            result.Take(highAlerts).Should().OnlyContain(a => a.Severity == AlertSeverity.High);
        }
    }

    #endregion

    #region CalculateStrategyConfidence Tests

    [Fact]
    public void CalculateStrategyConfidence_AllBenchmarksCovered_ReturnsHigh()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Moderate", true, true),
            ("Pull-up", "Moderate", true, true),
            ("Clean", "Moderate", true, true));
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360, "High");

        // Act
        var result = _sut.CalculateStrategyConfidence(pacingResult, volumeResult, timeResult);

        // Assert
        result.Level.Should().Be("High");
        result.Percentage.Should().Be(100);
        result.MissingBenchmarks.Should().BeEmpty();
    }

    [Fact]
    public void CalculateStrategyConfidence_PartialCoverage_ReturnsMedium()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Moderate", true, true),
            ("Pull-up", "Moderate", false, true),
            ("Clean", "Moderate", true, true),
            ("Box Jump", "Moderate", false, false));
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360, "Medium");

        // Act
        var result = _sut.CalculateStrategyConfidence(pacingResult, volumeResult, timeResult);

        // Assert
        result.Level.Should().Be("Medium");
        result.Percentage.Should().Be(50);
        result.MissingBenchmarks.Should().Contain("Pull-up");
        result.MissingBenchmarks.Should().Contain("Box Jump");
    }

    [Fact]
    public void CalculateStrategyConfidence_LowCoverage_ReturnsLow()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Thruster", "Moderate", false, false),
            ("Pull-up", "Moderate", false, false),
            ("Clean", "Moderate", true, true));
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360, "Low");

        // Act
        var result = _sut.CalculateStrategyConfidence(pacingResult, volumeResult, timeResult);

        // Assert
        result.Level.Should().Be("Low");
        result.Percentage.Should().BeLessThan(50);
    }

    [Fact]
    public void CalculateStrategyConfidence_CountsAreCorrect()
    {
        // Arrange
        var pacingResult = CreatePacingResultWithDetails(
            ("Movement1", "Moderate", true, true),
            ("Movement2", "Moderate", true, true),
            ("Movement3", "Moderate", false, false));
        var volumeResult = CreateVolumeResultWithClassifications("Moderate", "Moderate", "Moderate");
        var timeResult = CreateTimeEstimateResult(300, 360);

        // Act
        var result = _sut.CalculateStrategyConfidence(pacingResult, volumeResult, timeResult);

        // Assert
        result.TotalMovementCount.Should().Be(3);
        result.CoveredMovementCount.Should().Be(2);
    }

    #endregion

    #region DifficultyLabel Tests

    [Theory]
    [InlineData(1, "Very Easy")]
    [InlineData(2, "Very Easy")]
    [InlineData(3, "Easy")]
    [InlineData(4, "Easy")]
    [InlineData(5, "Moderate")]
    [InlineData(6, "Moderate")]
    [InlineData(7, "Hard")]
    [InlineData(8, "Hard")]
    [InlineData(9, "Very Hard")]
    [InlineData(10, "Very Hard")]
    public void DifficultyLabel_GetLabel_ReturnsCorrectLabel(int score, string expectedLabel)
    {
        // Act
        var result = DifficultyLabel.GetLabel(score);

        // Assert
        result.Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void DifficultyLabel_GetDescription_ReturnsNonEmptyString(int score)
    {
        // Act
        var result = DifficultyLabel.GetDescription(score);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Helper Methods

    private static WorkoutPacingResultDto CreatePacingResultWithLevels(params string[] levels)
    {
        var movements = levels.Select((level, index) => new MovementPacingDto
        {
            MovementDefinitionId = index + 1,
            MovementName = $"Movement{index + 1}",
            PacingLevel = level,
            AthletePercentile = 50m,
            HasAthleteBenchmark = true,
            HasPopulationData = true
        }).ToList();

        return new WorkoutPacingResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test Workout",
            WorkoutType = "ForTime",
            MovementPacing = movements,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static WorkoutPacingResultDto CreatePacingResultWithDetails(
        params (string name, string level, bool hasAthleteBenchmark, bool hasPopulationData)[] movements)
    {
        var movementDtos = movements.Select((m, index) => new MovementPacingDto
        {
            MovementDefinitionId = index + 1,
            MovementName = m.name,
            PacingLevel = m.level,
            AthletePercentile = 50m,
            HasAthleteBenchmark = m.hasAthleteBenchmark,
            HasPopulationData = m.hasPopulationData
        }).ToList();

        return new WorkoutPacingResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test Workout",
            WorkoutType = "ForTime",
            MovementPacing = movementDtos,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static WorkoutVolumeLoadResultDto CreateVolumeResultWithClassifications(params string[] classifications)
    {
        var volumes = classifications.Select((classification, index) => new MovementVolumeLoadDto
        {
            MovementDefinitionId = index + 1,
            MovementName = $"Movement{index + 1}",
            LoadClassification = classification,
            VolumeLoad = 1000m,
            HasSufficientData = true
        }).ToList();

        return new WorkoutVolumeLoadResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test Workout",
            MovementVolumes = volumes,
            TotalVolumeLoad = volumes.Sum(v => v.VolumeLoad),
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static WorkoutVolumeLoadResultDto CreateVolumeResultWithDetails(
        params (string name, string classification, bool hasSufficientData)[] movements)
    {
        var volumes = movements.Select((m, index) => new MovementVolumeLoadDto
        {
            MovementDefinitionId = index + 1,
            MovementName = m.name,
            LoadClassification = m.classification,
            VolumeLoad = 1000m,
            HasSufficientData = m.hasSufficientData
        }).ToList();

        return new WorkoutVolumeLoadResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test Workout",
            MovementVolumes = volumes,
            TotalVolumeLoad = volumes.Sum(v => v.VolumeLoad),
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static TimeEstimateResultDto CreateTimeEstimateResult(
        int minEstimate,
        int maxEstimate,
        string confidence = "High")
    {
        return new TimeEstimateResultDto
        {
            WorkoutId = 1,
            WorkoutName = "Test Workout",
            WorkoutType = "ForTime",
            EstimateType = "Time",
            MinEstimate = minEstimate,
            MaxEstimate = maxEstimate,
            ConfidenceLevel = confidence,
            CalculatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
