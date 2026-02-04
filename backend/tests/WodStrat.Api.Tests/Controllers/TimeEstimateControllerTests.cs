using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.TimeEstimate;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for TimeEstimateController.
/// </summary>
public class TimeEstimateControllerTests
{
    private readonly IFixture _fixture;
    private readonly ITimeEstimateService _timeEstimateService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;
    private readonly TimeEstimateController _sut;

    public TimeEstimateControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new TimeEstimateDtoCustomization());

        _timeEstimateService = Substitute.For<ITimeEstimateService>();
        _benchmarkService = Substitute.For<IBenchmarkService>();
        _workoutService = Substitute.For<IWorkoutService>();

        _sut = new TimeEstimateController(_timeEstimateService, _benchmarkService, _workoutService);
    }

    #region GetTimeEstimate Tests

    [Fact]
    public async Task GetTimeEstimate_ValidRequest_ReturnsOkWithTimeEstimateResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var timeEstimateResult = _fixture.Create<TimeEstimateResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TimeEstimateResponse>().Subject;
        response.WorkoutId.Should().Be(timeEstimateResult.WorkoutId);
        response.WorkoutName.Should().Be(timeEstimateResult.WorkoutName);
        response.FormattedRange.Should().Be(timeEstimateResult.FormattedRange);
    }

    [Fact]
    public async Task GetTimeEstimate_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetTimeEstimate_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetTimeEstimate_ServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((TimeEstimateResultDto?)null);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetTimeEstimate_MapsAllResponseFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var restRecommendations = _fixture.CreateMany<RestRecommendationDto>(2).ToList();
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.WorkoutId, workoutId)
            .With(x => x.WorkoutName, "Fran")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.EstimateType, "Time")
            .With(x => x.MinEstimate, 300)
            .With(x => x.MaxEstimate, 420)
            .With(x => x.FormattedRange, "5:00 - 7:00")
            .With(x => x.ConfidenceLevel, TimeEstimateConfidenceLevel.High)
            .With(x => x.FactorsSummary, "Test factors summary")
            .With(x => x.RestRecommendations, restRecommendations)
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .With(x => x.BenchmarkCoverageCount, 2)
            .With(x => x.TotalMovementCount, 2)
            .With(x => x.AveragePercentile, 70m)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TimeEstimateResponse>().Subject;
        response.WorkoutId.Should().Be(workoutId);
        response.WorkoutName.Should().Be("Fran");
        response.WorkoutType.Should().Be("ForTime");
        response.EstimateType.Should().Be("Time");
        response.MinEstimate.Should().Be(300);
        response.MaxEstimate.Should().Be(420);
        response.FormattedRange.Should().Be("5:00 - 7:00");
        response.ConfidenceLevel.Should().Be("High");
        response.FactorsSummary.Should().Be("Test factors summary");
        response.RestRecommendations.Should().HaveCount(2);
        response.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region CalculateTimeEstimate (POST) Tests

    [Fact]
    public async Task CalculateTimeEstimate_ValidRequest_ReturnsOkWithTimeEstimateResponse()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };
        var timeEstimateResult = _fixture.Create<TimeEstimateResultDto>();

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TimeEstimateResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculateTimeEstimate_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculateTimeEstimate_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculateTimeEstimate_ServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns((TimeEstimateResultDto?)null);

        // Act
        var result = await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetEmomFeasibility Tests

    [Fact]
    public async Task GetEmomFeasibility_ValidEmomWorkout_ReturnsOkWithFeasibilityResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var emomFeasibility = _fixture.CreateMany<EmomFeasibilityDto>(10).ToList();
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.WorkoutId, workoutId)
            .With(x => x.WorkoutName, "EMOM Test")
            .With(x => x.WorkoutType, "Emom")
            .With(x => x.EmomFeasibility, emomFeasibility)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<EmomFeasibilityResponse>().Subject;
        response.WorkoutId.Should().Be(workoutId);
        response.WorkoutName.Should().Be("EMOM Test");
        response.MinuteBreakdown.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetEmomFeasibility_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetEmomFeasibility_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetEmomFeasibility_NonEmomWorkout_ReturnsBadRequest()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.WorkoutType, "ForTime") // Not an EMOM workout
            .With(x => x.EmomFeasibility, (IReadOnlyList<EmomFeasibilityDto>?)null)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetEmomFeasibility_ServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((TimeEstimateResultDto?)null);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetEmomFeasibility_EmomWithEmptyFeasibility_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.WorkoutType, "Emom")
            .With(x => x.EmomFeasibility, Array.Empty<EmomFeasibilityDto>())
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetEmomFeasibility_MapsAllResponseFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var emomFeasibility = new List<EmomFeasibilityDto>
        {
            new EmomFeasibilityDto
            {
                Minute = 1,
                PrescribedWork = "10 Thrusters",
                EstimatedCompletionSeconds = 40,
                IsFeasible = true,
                BufferSeconds = 20,
                Recommendation = "On pace - comfortable buffer",
                MovementNames = new List<string> { "Thruster" }
            },
            new EmomFeasibilityDto
            {
                Minute = 2,
                PrescribedWork = "15 Pull-ups",
                EstimatedCompletionSeconds = 55,
                IsFeasible = false,
                BufferSeconds = 5,
                Recommendation = "Tight timing - maintain focus",
                MovementNames = new List<string> { "Pull-up" }
            }
        };
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.WorkoutId, workoutId)
            .With(x => x.WorkoutName, "EMOM Test")
            .With(x => x.WorkoutType, "Emom")
            .With(x => x.EmomFeasibility, emomFeasibility)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<EmomFeasibilityResponse>().Subject;

        response.WorkoutId.Should().Be(workoutId);
        response.WorkoutName.Should().Be("EMOM Test");
        response.MinuteBreakdown.Should().HaveCount(2);
        response.TotalMinutes.Should().Be(2);

        var minute1 = response.MinuteBreakdown[0];
        minute1.Minute.Should().Be(1);
        minute1.PrescribedWork.Should().Be("10 Thrusters");
        minute1.EstimatedCompletionSeconds.Should().Be(40);
        minute1.IsFeasible.Should().BeTrue();
        minute1.BufferSeconds.Should().Be(20);
        minute1.Recommendation.Should().Be("On pace - comfortable buffer");

        var minute2 = response.MinuteBreakdown[1];
        minute2.IsFeasible.Should().BeFalse();
    }

    #endregion

    #region Authorization Verification Tests

    [Fact]
    public async Task GetTimeEstimate_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify workout ownership was not checked when athlete ownership failed
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTimeEstimate_ValidatesWorkoutOwnershipAfterAthlete()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify both were called
        await _benchmarkService.Received(1).ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>());
        await _workoutService.Received(1).ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateTimeEstimate_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEmomFeasibility_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetEmomFeasibility(athleteId, workoutId, CancellationToken.None);

        // Assert
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Service Interaction Tests

    [Fact]
    public async Task GetTimeEstimate_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var athleteId = 123;
        var workoutId = 456;
        var timeEstimateResult = _fixture.Create<TimeEstimateResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        await _timeEstimateService.Received(1).EstimateWorkoutTimeAsync(123, 456, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateTimeEstimate_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = new CalculateTimeEstimateRequest
        {
            AthleteId = 789,
            WorkoutId = 101
        };
        var timeEstimateResult = _fixture.Create<TimeEstimateResultDto>();

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        await _sut.CalculateTimeEstimate(request, CancellationToken.None);

        // Assert
        await _timeEstimateService.Received(1).EstimateWorkoutTimeAsync(789, 101, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Rest Recommendation Response Mapping Tests

    [Fact]
    public async Task GetTimeEstimate_RestRecommendations_MapsCorrectly()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var restRecommendations = new List<RestRecommendationDto>
        {
            new RestRecommendationDto
            {
                AfterMovement = "Thruster",
                MovementDefinitionId = 1,
                PacingLevel = "Heavy",
                SuggestedRestSeconds = 4,
                RestRange = "3-5 seconds",
                Reasoning = "This is a strength - quick transitions"
            },
            new RestRecommendationDto
            {
                AfterMovement = "Pull-up",
                MovementDefinitionId = 2,
                PacingLevel = "Light",
                SuggestedRestSeconds = 17,
                RestRange = "15-20 seconds",
                Reasoning = "This is a weakness - longer recovery needed"
            }
        };
        var timeEstimateResult = _fixture.Build<TimeEstimateResultDto>()
            .With(x => x.RestRecommendations, restRecommendations)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(timeEstimateResult);

        // Act
        var result = await _sut.GetTimeEstimate(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TimeEstimateResponse>().Subject;

        response.RestRecommendations.Should().HaveCount(2);

        var rec1 = response.RestRecommendations[0];
        rec1.AfterMovement.Should().Be("Thruster");
        rec1.PacingLevel.Should().Be("Heavy");
        rec1.SuggestedRestSeconds.Should().Be(4);
        rec1.RestRange.Should().Be("3-5 seconds");

        var rec2 = response.RestRecommendations[1];
        rec2.AfterMovement.Should().Be("Pull-up");
        rec2.PacingLevel.Should().Be("Light");
        rec2.SuggestedRestSeconds.Should().Be(17);
    }

    #endregion
}
