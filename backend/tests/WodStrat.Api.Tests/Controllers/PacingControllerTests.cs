using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.Pacing;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for PacingController.
/// </summary>
public class PacingControllerTests
{
    private readonly IFixture _fixture;
    private readonly IPacingService _pacingService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;
    private readonly PacingController _sut;

    public PacingControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new PacingDtoCustomization());

        _pacingService = Substitute.For<IPacingService>();
        _benchmarkService = Substitute.For<IBenchmarkService>();
        _workoutService = Substitute.For<IWorkoutService>();

        _sut = new PacingController(_pacingService, _benchmarkService, _workoutService);
    }

    #region GetWorkoutPacing Tests

    [Fact]
    public async Task GetWorkoutPacing_ValidRequest_ReturnsOkWithPacingResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var pacingResult = _fixture.Create<WorkoutPacingResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(pacingResult);

        // Act
        var result = await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutPacingResponse>().Subject;
        response.WorkoutId.Should().Be(pacingResult.WorkoutId);
        response.WorkoutName.Should().Be(pacingResult.WorkoutName);
        response.MovementPacing.Should().HaveCount(pacingResult.MovementPacing.Count);
    }

    [Fact]
    public async Task GetWorkoutPacing_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutPacing_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutPacing_PacingCalculationReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutPacingResultDto?)null);

        // Act
        var result = await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutPacing_MapsAllResponseFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var movementPacing = _fixture.CreateMany<MovementPacingDto>(2).ToList();
        var pacingResult = _fixture.Build<WorkoutPacingResultDto>()
            .With(x => x.WorkoutId, workoutId)
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.MovementPacing, movementPacing)
            .With(x => x.OverallStrategyNotes, "Test strategy notes")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(pacingResult);

        // Act
        var result = await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutPacingResponse>().Subject;
        response.WorkoutId.Should().Be(workoutId);
        response.WorkoutName.Should().Be("Test Workout");
        response.MovementPacing.Should().HaveCount(2);
        response.OverallStrategyNotes.Should().Be("Test strategy notes");
        response.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region CalculatePacing (POST) Tests

    [Fact]
    public async Task CalculatePacing_ValidRequest_ReturnsOkWithPacingResponse()
    {
        // Arrange
        var request = new CalculatePacingRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };
        var pacingResult = _fixture.Create<WorkoutPacingResultDto>();

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateWorkoutPacingAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(pacingResult);

        // Act
        var result = await _sut.CalculatePacing(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutPacingResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculatePacing_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculatePacingRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculatePacing(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculatePacing_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculatePacingRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculatePacing(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculatePacing_PacingCalculationReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculatePacingRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateWorkoutPacingAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutPacingResultDto?)null);

        // Act
        var result = await _sut.CalculatePacing(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetMovementPacing Tests

    [Fact]
    public async Task GetMovementPacing_ValidRequest_ReturnsOkWithMovementPacingResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();
        var repCount = 21;
        var movementPacing = _fixture.Create<MovementPacingDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateMovementPacingAsync(athleteId, movementId, repCount, Arg.Any<CancellationToken>())
            .Returns(movementPacing);

        // Act
        var result = await _sut.GetMovementPacing(athleteId, movementId, repCount, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementPacingResponse>().Subject;
        response.MovementName.Should().Be(movementPacing.MovementName);
        response.PacingLevel.Should().Be(movementPacing.PacingLevel);
    }

    [Fact]
    public async Task GetMovementPacing_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetMovementPacing(athleteId, movementId, 21, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetMovementPacing_MovementNotFound_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateMovementPacingAsync(athleteId, movementId, 21, Arg.Any<CancellationToken>())
            .Returns((MovementPacingDto?)null);

        // Act
        var result = await _sut.GetMovementPacing(athleteId, movementId, 21, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetMovementPacing_MapsAllResponseFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();
        var movementPacing = _fixture.Build<MovementPacingDto>()
            .With(x => x.MovementDefinitionId, movementId)
            .With(x => x.MovementName, "Pull-Up")
            .With(x => x.PacingLevel, "Moderate")
            .With(x => x.AthletePercentile, 65m)
            .With(x => x.GuidanceText, "Controlled pace on Pull-Up")
            .With(x => x.RecommendedSets, new[] { 8, 7, 6 })
            .With(x => x.BenchmarkUsed, "Max Pull-ups")
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateMovementPacingAsync(athleteId, movementId, 21, Arg.Any<CancellationToken>())
            .Returns(movementPacing);

        // Act
        var result = await _sut.GetMovementPacing(athleteId, movementId, 21, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementPacingResponse>().Subject;
        response.MovementDefinitionId.Should().Be(movementId);
        response.MovementName.Should().Be("Pull-Up");
        response.PacingLevel.Should().Be("Moderate");
        response.AthletePercentile.Should().Be(65m);
        response.GuidanceText.Should().Be("Controlled pace on Pull-Up");
        response.RecommendedSets.Should().Equal(8, 7, 6);
        response.BenchmarkUsed.Should().Be("Max Pull-ups");
    }

    [Fact]
    public async Task GetMovementPacing_EmptyRecommendedSets_ReturnsNullInResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();
        var movementPacing = _fixture.Build<MovementPacingDto>()
            .With(x => x.RecommendedSets, Array.Empty<int>())
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateMovementPacingAsync(athleteId, movementId, 0, Arg.Any<CancellationToken>())
            .Returns(movementPacing);

        // Act
        var result = await _sut.GetMovementPacing(athleteId, movementId, 0, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementPacingResponse>().Subject;
        response.RecommendedSets.Should().BeNull();
    }

    #endregion

    #region Authorization Verification Tests

    [Fact]
    public async Task GetWorkoutPacing_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify workout ownership was not checked when athlete ownership failed
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetWorkoutPacing_ValidatesWorkoutOwnershipAfterAthlete()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetWorkoutPacing(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify both were called
        await _benchmarkService.Received(1).ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>());
        await _workoutService.Received(1).ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMovementPacing_OnlyValidatesAthleteOwnership()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var movementId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _pacingService.CalculateMovementPacingAsync(athleteId, movementId, 21, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<MovementPacingDto>());

        // Act
        await _sut.GetMovementPacing(athleteId, movementId, 21, CancellationToken.None);

        // Assert - Workout ownership should not be validated
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
