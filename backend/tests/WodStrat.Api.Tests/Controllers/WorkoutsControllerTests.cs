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
    public async Task ParseWorkoutText_ValidRequest_ReturnsOkWithParsedWorkoutResult()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP\n10 Pull-ups" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 100,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Success.Should().BeTrue();
        response.ParsedWorkout.Should().NotBeNull();
        response.ParsedWorkout!.OriginalText.Should().Be(parsedDto.OriginalText);
        response.ParsedWorkout.WorkoutType.Should().Be(parsedDto.WorkoutType.ToString());
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
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 100,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.ParsedWorkout.Should().NotBeNull();
        response.ParsedWorkout!.WorkoutType.Should().Be("Amrap");
        response.ParsedWorkout.TimeCapSeconds.Should().Be(1200);
    }

    [Fact]
    public async Task ParseWorkoutText_WithErrors_ReturnsErrorsInResponse()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "Invalid workout" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.Movements, new List<ParsedMovementDto>())
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 20,
            Errors = new List<ParsingErrorDto>
            {
                new() { LineNumber = 1, Message = "Unknown movement", ErrorType = "UnknownMovement" }
            },
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(1);
        response.Errors[0].Code.Should().Be("UNKNOWN_MOVEMENT");
    }

    [Fact]
    public async Task ParseWorkoutText_WithWarnings_ReturnsWarningsInResponse()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "5 rounds\n10 widowmakers" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.Rounds)
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 80,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>
            {
                new() { LineNumber = 2, Message = "Movement 'widowmakers' was not recognized.", WarningType = "UnknownMovement", Suggestion = "Did you mean 'walking lunges'?" }
            }
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Warnings.Should().HaveCount(1);
        response.Warnings[0].Code.Should().Be("UNKNOWN_MOVEMENT");
        response.Warnings[0].Suggestion.Should().Contain("walking lunges");
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
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 100,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.ParsedWorkout.Should().NotBeNull();
        response.ParsedWorkout!.Movements.Should().HaveCount(2);
    }

    [Fact]
    public async Task ParseWorkoutText_ReturnsConfidenceScoring()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP\n10 Pull-ups" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 85,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>(),
            ConfidenceDetails = new ConfidenceBreakdown
            {
                WorkoutTypeConfidence = 100,
                TimeDomainConfidence = 100,
                MovementIdentificationConfidence = 80,
                MovementsIdentified = 1,
                TotalMovementLines = 1,
                MovementsWithCompleteData = 1
            }
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.ParseConfidence.Should().Be(0.85m);
        response.ConfidenceLevel.Should().Be("High");
        response.ConfidenceDetails.Should().NotBeNull();
        response.ConfidenceDetails!.WorkoutTypeConfidence.Should().Be(1.0m);
    }

    [Fact]
    public async Task ParseWorkoutText_PartialResult_ReturnsPartialResultWhenFailed()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP\nSome random text" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.Amrap)
            .With(x => x.TimeCapSeconds, 1200)
            .With(x => x.Movements, new List<ParsedMovementDto>())
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 30,
            Errors = new List<ParsingErrorDto>
            {
                new() { Message = "No movements detected", ErrorType = "NoMovementsDetected" }
            },
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Success.Should().BeFalse();
        response.ParsedWorkout.Should().BeNull();
        response.PartialResult.Should().NotBeNull();
        response.PartialResult!.WorkoutType.Should().Be("Amrap");
    }

    [Fact]
    public async Task ParseWorkoutText_MultipleErrors_ReturnsAllErrorsFormatted()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "Invalid workout text" };
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = null,
            ConfidenceScore = 0,
            Errors = new List<ParsingErrorDto>
            {
                new() { LineNumber = 1, Message = "Empty input", ErrorType = "EmptyInput", ErrorCode = 100 },
                new() { LineNumber = 2, Message = "No movements detected", ErrorType = "NoMovementsDetected", ErrorCode = 103 }
            },
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(2);
        response.Errors[0].Code.Should().Be("EMPTY_INPUT");
        response.Errors[1].Code.Should().Be("NO_MOVEMENTS_DETECTED");
    }

    [Fact]
    public async Task ParseWorkoutText_WithSuggestion_ReturnsSuggestionInResponse()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "10 Burpies" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.Movements, new List<ParsedMovementDto>())
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 70,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>
            {
                new()
                {
                    LineNumber = 1,
                    Message = "Movement 'Burpies' not recognized.",
                    WarningType = "UnknownMovement",
                    Suggestion = "Did you mean 'Burpees'?",
                    OriginalText = "10 Burpies",
                    SimilarNames = new List<string> { "Burpees", "Bar-facing Burpees" }
                }
            }
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Warnings.Should().HaveCount(1);
        response.Warnings[0].Suggestion.Should().Be("Did you mean 'Burpees'?");
        response.Warnings[0].OriginalText.Should().Be("10 Burpies");
    }

    [Fact]
    public async Task ParseWorkoutText_ErrorWithLineNumber_ReturnsLineInResponse()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "AMRAP 10\n-5 Push-ups" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.Amrap)
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 40,
            Errors = new List<ParsingErrorDto>
            {
                new()
                {
                    LineNumber = 2,
                    Message = "Invalid rep count '-5'",
                    ErrorType = "InvalidRepCount",
                    ErrorCode = 204,
                    OriginalText = "-5 Push-ups"
                }
            },
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Errors[0].Line.Should().Be(2);
        response.Errors[0].OriginalText.Should().Be("-5 Push-ups");
    }

    [Theory]
    [InlineData(100, "Perfect")]
    [InlineData(85, "High")]
    [InlineData(70, "Medium")]
    [InlineData(50, "Low")]
    [InlineData(25, "Low")]
    public async Task ParseWorkoutText_ConfidenceScore_MapsToCorrectLevel(int score, string expectedLevel)
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "Test workout" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = score >= 50,
            ParsedWorkout = parsedDto,
            ConfidenceScore = score,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.ParseConfidence.Should().Be(score / 100m);
        response.ConfidenceLevel.Should().Be(expectedLevel);
    }

    [Fact]
    public async Task ParseWorkoutText_IsUsable_TrueWhenSuccessful()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "20 min AMRAP\n10 Pull-ups" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 85,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.IsUsable.Should().BeTrue();
    }

    [Fact]
    public async Task ParseWorkoutText_IsUsable_FalseWhenFailed()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "Invalid text" };
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = null,
            ConfidenceScore = 10,
            Errors = new List<ParsingErrorDto>
            {
                new() { ErrorType = "EmptyInput", Message = "No valid input" }
            },
            Warnings = new List<ParsingWarningDto>()
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.IsUsable.Should().BeFalse();
    }

    [Fact]
    public async Task ParseWorkoutText_ConfidenceDetails_MapsAllFields()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "AMRAP 20\n10 Pull-ups\n15 Push-ups\n20 Squats" };
        var parsedDto = _fixture.Create<ParsedWorkoutDto>();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = true,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 90,
            Errors = new List<ParsingErrorDto>(),
            Warnings = new List<ParsingWarningDto>(),
            ConfidenceDetails = new ConfidenceBreakdown
            {
                WorkoutTypeConfidence = 100,
                TimeDomainConfidence = 100,
                MovementIdentificationConfidence = 90,
                MovementsIdentified = 3,
                TotalMovementLines = 3,
                MovementsWithCompleteData = 3
            }
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.ConfidenceDetails.Should().NotBeNull();
        response.ConfidenceDetails!.WorkoutTypeConfidence.Should().Be(1.0m);
        response.ConfidenceDetails.TimeDomainConfidence.Should().Be(1.0m);
        response.ConfidenceDetails.MovementIdentificationConfidence.Should().Be(0.9m);
        response.ConfidenceDetails.MovementsIdentified.Should().Be(3);
        response.ConfidenceDetails.TotalMovementLines.Should().Be(3);
        response.ConfidenceDetails.MovementsWithCompleteData.Should().Be(3);
        response.ConfidenceDetails.MovementIdentificationRate.Should().Be(100.0m);
    }

    [Fact]
    public async Task ParseWorkoutText_MixedErrorsAndWarnings_ReturnsAllInSeparateLists()
    {
        // Arrange
        var request = new ParseWorkoutRequest { Text = "For time:\n10 Pushups\n-5 Sqats" };
        var parsedDto = _fixture.Build<ParsedWorkoutDto>()
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .Create();
        var parsedResult = new ParsedWorkoutResult
        {
            Success = false,
            ParsedWorkout = parsedDto,
            ConfidenceScore = 35,
            Errors = new List<ParsingErrorDto>
            {
                new() { LineNumber = 3, Message = "Invalid rep count", ErrorType = "InvalidRepCount" }
            },
            Warnings = new List<ParsingWarningDto>
            {
                new() { LineNumber = 2, Message = "Unknown movement 'Pushups'", WarningType = "UnknownMovement" },
                new() { LineNumber = 3, Message = "Unknown movement 'Sqats'", WarningType = "UnknownMovement" }
            }
        };

        _workoutParsingService.ParseWorkoutAsync(request.Text, Arg.Any<CancellationToken>())
            .Returns(parsedResult);

        // Act
        var result = await _sut.ParseWorkoutText(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ParsedWorkoutResultResponse>().Subject;
        response.Errors.Should().HaveCount(1);
        response.Warnings.Should().HaveCount(2);
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
