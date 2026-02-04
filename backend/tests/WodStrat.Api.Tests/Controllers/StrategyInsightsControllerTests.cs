using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.StrategyInsights;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for StrategyInsightsController.
/// </summary>
public class StrategyInsightsControllerTests
{
    private readonly IFixture _fixture;
    private readonly IStrategyInsightsService _strategyInsightsService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;
    private readonly StrategyInsightsController _sut;

    public StrategyInsightsControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new StrategyInsightsDtoCustomization());

        _strategyInsightsService = Substitute.For<IStrategyInsightsService>();
        _benchmarkService = Substitute.For<IBenchmarkService>();
        _workoutService = Substitute.For<IWorkoutService>();

        _sut = new StrategyInsightsController(_strategyInsightsService, _benchmarkService, _workoutService);
    }

    #region GetStrategyInsights Tests

    [Fact]
    public async Task GetStrategyInsights_ValidRequest_ReturnsOkWithStrategyInsightsResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;
        response.WorkoutId.Should().Be(insightsResult.WorkoutId);
        response.WorkoutName.Should().Be(insightsResult.WorkoutName);
        response.DifficultyScore.Should().NotBeNull();
        response.DifficultyScore.Score.Should().Be(insightsResult.DifficultyScore.Score);
        response.StrategyConfidence.Should().NotBeNull();
        response.KeyFocusMovements.Should().HaveCount(insightsResult.KeyFocusMovements.Count);
        response.RiskAlerts.Should().HaveCount(insightsResult.RiskAlerts.Count);
    }

    [Fact]
    public async Task GetStrategyInsights_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetStrategyInsights_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetStrategyInsights_ServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((StrategyInsightsResultDto?)null);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetStrategyInsights_ValidatesAthleteOwnershipFirst()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>());
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _strategyInsightsService.DidNotReceive().CalculateStrategyInsightsAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStrategyInsights_ValidatesWorkoutOwnershipAfterAthlete()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>());
        await _workoutService.Received(1).ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
        await _strategyInsightsService.DidNotReceive().CalculateStrategyInsightsAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStrategyInsights_ResponseMapsAllDifficultyScoreFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;

        response.DifficultyScore.Score.Should().Be(insightsResult.DifficultyScore.Score);
        response.DifficultyScore.Label.Should().Be(insightsResult.DifficultyScore.Label);
        response.DifficultyScore.Description.Should().Be(insightsResult.DifficultyScore.Description);
        response.DifficultyScore.Breakdown.PacingFactor.Should().Be(insightsResult.DifficultyScore.Breakdown.PacingFactor);
        response.DifficultyScore.Breakdown.VolumeFactor.Should().Be(insightsResult.DifficultyScore.Breakdown.VolumeFactor);
        response.DifficultyScore.Breakdown.TimeFactor.Should().Be(insightsResult.DifficultyScore.Breakdown.TimeFactor);
        response.DifficultyScore.Breakdown.ExperienceModifier.Should().Be(insightsResult.DifficultyScore.Breakdown.ExperienceModifier);
    }

    [Fact]
    public async Task GetStrategyInsights_ResponseMapsAllConfidenceFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;

        response.StrategyConfidence.Level.Should().Be(insightsResult.StrategyConfidence.Level);
        response.StrategyConfidence.Percentage.Should().Be(insightsResult.StrategyConfidence.Percentage);
        response.StrategyConfidence.Explanation.Should().Be(insightsResult.StrategyConfidence.Explanation);
        response.StrategyConfidence.MissingBenchmarks.Should().BeEquivalentTo(insightsResult.StrategyConfidence.MissingBenchmarks);
    }

    [Fact]
    public async Task GetStrategyInsights_ResponseMapsKeyFocusMovements()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;

        response.KeyFocusMovements.Should().HaveCount(insightsResult.KeyFocusMovements.Count);
        for (var i = 0; i < response.KeyFocusMovements.Count; i++)
        {
            response.KeyFocusMovements[i].MovementName.Should().Be(insightsResult.KeyFocusMovements[i].MovementName);
            response.KeyFocusMovements[i].Reason.Should().Be(insightsResult.KeyFocusMovements[i].Reason);
            response.KeyFocusMovements[i].Recommendation.Should().Be(insightsResult.KeyFocusMovements[i].Recommendation);
            response.KeyFocusMovements[i].Priority.Should().Be(insightsResult.KeyFocusMovements[i].Priority);
        }
    }

    [Fact]
    public async Task GetStrategyInsights_ResponseMapsRiskAlerts()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;

        response.RiskAlerts.Should().HaveCount(insightsResult.RiskAlerts.Count);
        for (var i = 0; i < response.RiskAlerts.Count; i++)
        {
            response.RiskAlerts[i].AlertType.Should().Be(insightsResult.RiskAlerts[i].AlertType);
            response.RiskAlerts[i].Severity.Should().Be(insightsResult.RiskAlerts[i].Severity);
            response.RiskAlerts[i].Title.Should().Be(insightsResult.RiskAlerts[i].Title);
            response.RiskAlerts[i].Message.Should().Be(insightsResult.RiskAlerts[i].Message);
            response.RiskAlerts[i].AffectedMovements.Should().BeEquivalentTo(insightsResult.RiskAlerts[i].AffectedMovements);
            response.RiskAlerts[i].SuggestedAction.Should().Be(insightsResult.RiskAlerts[i].SuggestedAction);
        }
    }

    [Fact]
    public async Task GetStrategyInsights_ResponseMapsCalculatedAt()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();
        insightsResult.CalculatedAt = DateTime.UtcNow;

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;
        response.CalculatedAt.Should().Be(insightsResult.CalculatedAt);
    }

    [Fact]
    public async Task GetStrategyInsights_EmptyKeyFocusMovements_ReturnsEmptyList()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();
        insightsResult.KeyFocusMovements = new List<KeyFocusMovementDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;
        response.KeyFocusMovements.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStrategyInsights_EmptyRiskAlerts_ReturnsEmptyList()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var insightsResult = _fixture.Create<StrategyInsightsResultDto>();
        insightsResult.RiskAlerts = new List<RiskAlertDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(insightsResult);

        // Act
        var result = await _sut.GetStrategyInsights(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StrategyInsightsResponse>().Subject;
        response.RiskAlerts.Should().BeEmpty();
    }

    #endregion
}
