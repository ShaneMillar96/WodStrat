using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for WorkoutsController.
/// </summary>
public class WorkoutsControllerTests
{
    private readonly IFixture _fixture;
    private readonly IWorkoutService _workoutService;
    private readonly IWorkoutParsingService _workoutParsingService;
    private readonly WorkoutsController _sut;

    public WorkoutsControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new WorkoutDtoCustomization());

        _workoutService = Substitute.For<IWorkoutService>();
        _workoutParsingService = Substitute.For<IWorkoutParsingService>();
        _sut = new WorkoutsController(_workoutService, _workoutParsingService);
    }

    #region ParseWorkoutText Tests

    [Fact]
    public async Task ParseWorkoutText_ValidRequest_ReturnsOkWithParsedWorkout()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP\n10 Pull-ups" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();

        _workoutParsingService.ParseWorkoutTextAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedDto);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResponse>().Subject;
        response.OriginalText.Should().Be(parsedDto.OriginalText);
        response.WorkoutType.Should().Be(parsedDto.WorkoutType.ToString());
    }

    [Fact]
    public async Task ParseWorkoutText_AMRAPWorkout_ReturnsCorrectType()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.Amrap)
            .With(x => x.TimeCapSeconds, 1200)
            .Create();

        _workoutParsingService.ParseWorkoutTextAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedDto);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResponse>().Subject;
        response.WorkoutType.Should().Be("Amrap");
        response.TimeCapSeconds.Should().Be(1200);
    }

    [Fact]
    public async Task ParseWorkoutText_WithErrors_ReturnsErrorsInResponse()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "Invalid workout" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.Movements, new List<ParsedMovementDto>())
            .With(x => x.Errors, new List<ParsingErrorDto>
            {
                new() { LineNumber = 1, Message = "Unknown movement", ErrorType = "UnknownMovement" }
            })
            .Create();

        _workoutParsingService.ParseWorkoutTextAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedDto);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResponse>().Subject;
        response.IsValid.Should().BeFalse();
        response.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseWorkoutText_WithMovements_ReturnsParsedMovements()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "21-15-9\nThrusters\nPull-ups" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.Movements, new List<ParsedMovementDto>
            {
                new() { MovementName = "Thruster", SequenceOrder = 1, RepCount = 21, MovementDefinitionId = 1 },
                new() { MovementName = "Pull-up", SequenceOrder = 2, RepCount = 21, MovementDefinitionId = 2 }
            })
            .Create();

        _workoutParsingService.ParseWorkoutTextAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedDto);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResponse>().Subject;
        response.Movements.Should().HaveCount(2);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateWorkoutRequest
        {
            Name = "Test WOD",
            WorkoutType = "ForTime",
            OriginalText = "21-15-9\nThrusters\nPull-ups",
            TimeCapSeconds = 1200,
            Movements = new List<CreateWorkoutMovementRequest>()
        };

        var createdDto = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, 1)
            .With(x => x.Name, request.Name)
            .With(x => x.WorkoutType, request.WorkoutType)
            .Create();

        _workoutService.CreateWorkoutAsync(Arg.Any<CreateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(WorkoutsController.GetById));
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Name.Should().Be(request.Name);
        response.WorkoutType.Should().Be(request.WorkoutType);
    }

    [Fact]
    public async Task Create_NotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateWorkoutRequest
        {
            Name = "Test WOD",
            WorkoutType = "ForTime",
            OriginalText = "21-15-9",
            Movements = new List<CreateWorkoutMovementRequest>()
        };

        _workoutService.CreateWorkoutAsync(Arg.Any<CreateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns((WorkoutDto?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Create_WithMovements_CreatesWithMovements()
    {
        // Arrange
        var request = new CreateWorkoutRequest
        {
            WorkoutType = "ForTime",
            OriginalText = "21-15-9\nThrusters",
            Movements = new List<CreateWorkoutMovementRequest>
            {
                new() { MovementDefinitionId = 1, SequenceOrder = 1, RepCount = 21, LoadValue = 95, LoadUnit = "Lb" }
            }
        };

        var createdDto = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, 1)
            .With(x => x.Movements, new List<WorkoutMovementDto>
            {
                new() { MovementDefinitionId = 1, MovementName = "Thruster", RepCount = 21 }
            })
            .Create();

        _workoutService.CreateWorkoutAsync(Arg.Any<CreateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Movements.Should().HaveCount(1);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_HasWorkouts_ReturnsOkWithWorkouts()
    {
        // Arrange
        var workouts = new List<WorkoutDto>
        {
            _fixture.Build<WorkoutDto>().With(x => x.Id, 1).Create(),
            _fixture.Build<WorkoutDto>().With(x => x.Id, 2).Create()
        };

        _workoutService.GetCurrentUserWorkoutsAsync(Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<WorkoutResponse>>().Subject;
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_NoWorkouts_ReturnsOkWithEmptyList()
    {
        // Arrange
        _workoutService.GetCurrentUserWorkoutsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<WorkoutDto>());

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<WorkoutResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_MapsAllPropertiesToResponse()
    {
        // Arrange
        var workout = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, 1)
            .With(x => x.Name, "Cindy")
            .With(x => x.WorkoutType, "Amrap")
            .With(x => x.TimeCapSeconds, 1200)
            .With(x => x.TimeCapFormatted, "20:00")
            .Create();

        _workoutService.GetCurrentUserWorkoutsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<WorkoutDto> { workout });

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<WorkoutResponse>>().Subject.First();
        response.Id.Should().Be(workout.Id);
        response.Name.Should().Be(workout.Name);
        response.WorkoutType.Should().Be(workout.WorkoutType);
        response.TimeCapSeconds.Should().Be(workout.TimeCapSeconds);
        response.TimeCapFormatted.Should().Be(workout.TimeCapFormatted);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ValidIdAndOwner_ReturnsOkWithWorkout()
    {
        // Arrange
        var workoutId = 1;
        var workout = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, workoutId)
            .Create();

        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.GetWorkoutByIdAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(workout);

        // Act
        var result = await _sut.GetById(workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Id.Should().Be(workoutId);
    }

    [Fact]
    public async Task GetById_NotOwner_ReturnsNotFound()
    {
        // Arrange
        var workoutId = 1;
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.GetById(workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_WorkoutNotFound_ReturnsNotFound()
    {
        // Arrange
        var workoutId = 999;
        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.GetWorkoutByIdAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns((WorkoutDto?)null);

        // Act
        var result = await _sut.GetById(workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_MapsMovementsCorrectly()
    {
        // Arrange
        var workoutId = 1;
        var workout = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, workoutId)
            .With(x => x.Movements, new List<WorkoutMovementDto>
            {
                new()
                {
                    Id = 1,
                    MovementDefinitionId = 10,
                    MovementName = "Thruster",
                    MovementCategory = "Weightlifting",
                    SequenceOrder = 1,
                    RepCount = 21,
                    LoadValue = 95,
                    LoadUnit = "Lb",
                    LoadFormatted = "95 lb"
                }
            })
            .Create();

        _workoutService.ValidateOwnershipAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);
        _workoutService.GetWorkoutByIdAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(workout);

        // Act
        var result = await _sut.GetById(workoutId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Movements.Should().HaveCount(1);
        response.Movements[0].MovementName.Should().Be("Thruster");
        response.Movements[0].LoadFormatted.Should().Be("95 lb");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidIdAndOwner_ReturnsOkWithUpdatedWorkout()
    {
        // Arrange
        var workoutId = 1;
        var request = new UpdateWorkoutRequest
        {
            Name = "Updated WOD",
            WorkoutType = "Amrap",
            TimeCapSeconds = 900
        };

        var updatedDto = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, workoutId)
            .With(x => x.Name, request.Name)
            .With(x => x.WorkoutType, request.WorkoutType)
            .Create();

        _workoutService.UpdateWorkoutAsync(workoutId, Arg.Any<UpdateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        var result = await _sut.Update(workoutId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Id.Should().Be(workoutId);
        response.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Update_NotOwnerOrNotFound_ReturnsNotFound()
    {
        // Arrange
        var workoutId = 1;
        var request = new UpdateWorkoutRequest
        {
            Name = "Updated",
            WorkoutType = "ForTime"
        };

        _workoutService.UpdateWorkoutAsync(workoutId, Arg.Any<UpdateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns((WorkoutDto?)null);

        // Act
        var result = await _sut.Update(workoutId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Update_WithMovements_ReplacesMovements()
    {
        // Arrange
        var workoutId = 1;
        var request = new UpdateWorkoutRequest
        {
            Name = "Updated",
            WorkoutType = "ForTime",
            Movements = new List<CreateWorkoutMovementRequest>
            {
                new() { MovementDefinitionId = 5, SequenceOrder = 1, RepCount = 15 }
            }
        };

        var updatedDto = _fixture.Build<WorkoutDto>()
            .With(x => x.Id, workoutId)
            .With(x => x.Movements, new List<WorkoutMovementDto>
            {
                new() { MovementDefinitionId = 5, RepCount = 15 }
            })
            .Create();

        _workoutService.UpdateWorkoutAsync(workoutId, Arg.Any<UpdateWorkoutDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        var result = await _sut.Update(workoutId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WorkoutResponse>().Subject;
        response.Movements.Should().HaveCount(1);
        response.Movements[0].MovementDefinitionId.Should().Be(5);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ValidIdAndOwner_ReturnsNoContent()
    {
        // Arrange
        var workoutId = 1;
        _workoutService.DeleteWorkoutAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Delete(workoutId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NotOwnerOrNotFound_ReturnsNotFound()
    {
        // Arrange
        var workoutId = 1;
        _workoutService.DeleteWorkoutAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Delete(workoutId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Delete_CallsDeleteWorkoutAsync()
    {
        // Arrange
        var workoutId = 42;
        _workoutService.DeleteWorkoutAsync(workoutId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.Delete(workoutId, CancellationToken.None);

        // Assert
        await _workoutService.Received(1).DeleteWorkoutAsync(workoutId, Arg.Any<CancellationToken>());
    }

    #endregion
}
