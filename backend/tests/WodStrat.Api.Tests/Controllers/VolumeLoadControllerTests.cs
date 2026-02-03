using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.VolumeLoad;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for VolumeLoadController.
/// </summary>
public class VolumeLoadControllerTests
{
    private readonly IFixture _fixture;
    private readonly IVolumeLoadService _volumeLoadService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;
    private readonly VolumeLoadController _sut;

    public VolumeLoadControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new VolumeLoadDtoCustomization());

        _volumeLoadService = Substitute.For<IVolumeLoadService>();
        _benchmarkService = Substitute.For<IBenchmarkService>();
        _workoutService = Substitute.For<IWorkoutService>();

        _sut = new VolumeLoadController(_volumeLoadService, _benchmarkService, _workoutService);
    }

    #region GetWorkoutVolumeLoad Tests

    [Fact]
    public async Task GetWorkoutVolumeLoad_ValidRequest_ReturnsOkWithVolumeLoadResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var volumeLoadResult = _fixture.Create<WorkoutVolumeLoadResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutVolumeLoadResponse>().Subject;
        response.WorkoutId.Should().Be(volumeLoadResult.WorkoutId);
        response.WorkoutName.Should().Be(volumeLoadResult.WorkoutName);
        response.MovementVolumes.Should().HaveCount(volumeLoadResult.MovementVolumes.Count);
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_VolumeLoadCalculationReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutVolumeLoadResultDto?)null);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_MapsAllResponseFields()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();
        var movementVolumes = _fixture.CreateMany<MovementVolumeLoadDto>(2).ToList();

        // Set specific values for verification
        movementVolumes[0].MovementName = "Thruster";
        movementVolumes[0].Weight = 43m;
        movementVolumes[0].VolumeLoad = 1935m;
        movementVolumes[0].LoadClassification = "Moderate";
        movementVolumes[0].Tip = "Pace yourself";
        movementVolumes[0].RecommendedWeight = null;

        var volumeLoadResult = _fixture.Build<WorkoutVolumeLoadResultDto>()
            .With(x => x.WorkoutId, workoutId)
            .With(x => x.WorkoutName, "Fran")
            .With(x => x.MovementVolumes, movementVolumes)
            .With(x => x.TotalVolumeLoad, 1935m)
            .With(x => x.TotalVolumeLoadFormatted, "1,935 kg")
            .With(x => x.OverallAssessment, "Moderate volume workout")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutVolumeLoadResponse>().Subject;

        response.WorkoutId.Should().Be(workoutId);
        response.WorkoutName.Should().Be("Fran");
        response.TotalVolumeLoad.Should().Be(1935m);
        response.TotalVolumeLoadFormatted.Should().Be("1,935 kg");
        response.OverallAssessment.Should().Be("Moderate volume workout");
        response.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.MovementVolumes.Should().HaveCount(2);

        var firstMovement = response.MovementVolumes[0];
        firstMovement.MovementName.Should().Be("Thruster");
        firstMovement.Weight.Should().Be(43m);
        firstMovement.VolumeLoad.Should().Be(1935m);
        firstMovement.LoadClassification.Should().Be("Moderate");
        firstMovement.Tip.Should().Be("Pace yourself");
        firstMovement.RecommendedWeight.Should().BeNull();
    }

    #endregion

    #region CalculateVolumeLoad (POST) Tests

    [Fact]
    public async Task CalculateVolumeLoad_ValidRequest_ReturnsOkWithVolumeLoadResponse()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };
        var volumeLoadResult = _fixture.Create<WorkoutVolumeLoadResultDto>();

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        var result = await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutVolumeLoadResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculateVolumeLoad_AthleteNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculateVolumeLoad_WorkoutNotOwned_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CalculateVolumeLoad_VolumeLoadCalculationReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutVolumeLoadResultDto?)null);

        // Act
        var result = await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Authorization Verification Tests

    [Fact]
    public async Task GetWorkoutVolumeLoad_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify workout ownership was not checked when athlete ownership failed
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_ValidatesWorkoutOwnershipAfterAthlete()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert - Verify both were called
        await _benchmarkService.Received(1).ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>());
        await _workoutService.Received(1).ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateVolumeLoad_ValidatesAthleteOwnershipBeforeWorkout()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = _fixture.Create<int>(),
            WorkoutId = _fixture.Create<int>()
        };

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        await _workoutService.DidNotReceive().ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Movement Volume Response Mapping Tests

    [Fact]
    public async Task GetWorkoutVolumeLoad_MovementWithScalingRecommendation_MapsCorrectly()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        var movementVolume = _fixture.Build<MovementVolumeLoadDto>()
            .With(x => x.MovementName, "Deadlift")
            .With(x => x.Weight, 70m)
            .With(x => x.LoadClassification, "High")
            .With(x => x.AthleteBenchmarkPercentile, 45m)
            .With(x => x.RecommendedWeight, 56m)
            .With(x => x.RecommendedWeightFormatted, "56 kg (80% of RX)")
            .With(x => x.Tip, "Consider scaling to 56 kg")
            .Create();

        var volumeLoadResult = _fixture.Build<WorkoutVolumeLoadResultDto>()
            .With(x => x.MovementVolumes, new List<MovementVolumeLoadDto> { movementVolume })
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutVolumeLoadResponse>().Subject;

        var movement = response.MovementVolumes.First();
        movement.LoadClassification.Should().Be("High");
        movement.AthleteBenchmarkPercentile.Should().Be(45m);
        movement.RecommendedWeight.Should().Be(56m);
        movement.RecommendedWeightFormatted.Should().Be("56 kg (80% of RX)");
    }

    [Fact]
    public async Task GetWorkoutVolumeLoad_BodyweightMovement_MapsCorrectly()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var workoutId = _fixture.Create<int>();

        var movementVolume = _fixture.Build<MovementVolumeLoadDto>()
            .With(x => x.MovementName, "Pull-Up")
            .With(x => x.Weight, 0m)
            .With(x => x.VolumeLoad, 0m)
            .With(x => x.VolumeLoadFormatted, "0 kg")
            .With(x => x.LoadClassification, "Bodyweight")
            .With(x => x.BenchmarkUsed, "None")
            .With(x => x.AthleteBenchmarkPercentile, (decimal?)null)
            .With(x => x.RecommendedWeight, (decimal?)null)
            .With(x => x.HasSufficientData, false)
            .Create();

        var volumeLoadResult = _fixture.Build<WorkoutVolumeLoadResultDto>()
            .With(x => x.MovementVolumes, new List<MovementVolumeLoadDto> { movementVolume })
            .Create();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        var result = await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutVolumeLoadResponse>().Subject;

        var movement = response.MovementVolumes.First();
        movement.LoadClassification.Should().Be("Bodyweight");
        movement.VolumeLoad.Should().Be(0m);
        movement.AthleteBenchmarkPercentile.Should().BeNull();
    }

    #endregion

    #region Service Interaction Tests

    [Fact]
    public async Task GetWorkoutVolumeLoad_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var athleteId = 123;
        var workoutId = 456;
        var volumeLoadResult = _fixture.Create<WorkoutVolumeLoadResultDto>();

        _benchmarkService.ValidateOwnershipAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        await _sut.GetWorkoutVolumeLoad(athleteId, workoutId, CancellationToken.None);

        // Assert
        await _volumeLoadService.Received(1).CalculateWorkoutVolumeLoadAsync(123, 456, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateVolumeLoad_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = new CalculateVolumeLoadRequest
        {
            AthleteId = 789,
            WorkoutId = 101
        };
        var volumeLoadResult = _fixture.Create<WorkoutVolumeLoadResultDto>();

        _benchmarkService.ValidateOwnershipAsync(request.AthleteId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.ValidateOwnershipAsync(request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _volumeLoadService.CalculateWorkoutVolumeLoadAsync(request.AthleteId, request.WorkoutId, Arg.Any<CancellationToken>())
            .Returns(volumeLoadResult);

        // Act
        await _sut.CalculateVolumeLoad(request, CancellationToken.None);

        // Assert
        await _volumeLoadService.Received(1).CalculateWorkoutVolumeLoadAsync(789, 101, Arg.Any<CancellationToken>());
    }

    #endregion
}
