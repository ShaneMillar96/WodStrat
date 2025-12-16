using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for AthletesController.
/// </summary>
public class AthletesControllerTests
{
    private readonly IFixture _fixture;
    private readonly IAthleteService _athleteService;
    private readonly ICurrentUserService _currentUserService;
    private readonly AthletesController _sut;

    public AthletesControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteDtoCustomization());

        _athleteService = Substitute.For<IAthleteService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new AthletesController(_athleteService, _currentUserService);
    }

    #region GetCurrentUserProfile Tests

    [Fact]
    public async Task GetCurrentUserProfile_HasAthleteProfile_ReturnsOkWithAthleteResponse()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetCurrentUserProfile(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Id.Should().Be(athleteDto.Id);
        response.Name.Should().Be(athleteDto.Name);
        response.Age.Should().Be(athleteDto.Age);
        response.ExperienceLevel.Should().Be(athleteDto.ExperienceLevel);
        response.PrimaryGoal.Should().Be(athleteDto.PrimaryGoal);
    }

    [Fact]
    public async Task GetCurrentUserProfile_NoAthleteProfile_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetCurrentUserProfile(CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetCurrentUserProfile_MapsAllDtoPropertiesToResponse()
    {
        // Arrange
        var athleteDto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, 30)
            .With(x => x.Gender, "Female")
            .With(x => x.HeightCm, 165m)
            .With(x => x.WeightKg, 60m)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetCurrentUserProfile(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;

        response.Id.Should().Be(athleteDto.Id);
        response.Name.Should().Be(athleteDto.Name);
        response.Age.Should().Be(athleteDto.Age);
        response.Gender.Should().Be(athleteDto.Gender);
        response.HeightCm.Should().Be(athleteDto.HeightCm);
        response.WeightKg.Should().Be(athleteDto.WeightKg);
        response.ExperienceLevel.Should().Be(athleteDto.ExperienceLevel);
        response.PrimaryGoal.Should().Be(athleteDto.PrimaryGoal);
        response.CreatedAt.Should().Be(athleteDto.CreatedAt);
        response.UpdatedAt.Should().Be(athleteDto.UpdatedAt);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test Athlete",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            Gender = "Male",
            HeightCm = 180m,
            WeightKg = 85m,
            ExperienceLevel = "Intermediate",
            PrimaryGoal = "ImprovePacing"
        };

        var createdDto = _fixture.Build<AthleteDto>()
            .With(x => x.Name, request.Name)
            .With(x => x.ExperienceLevel, request.ExperienceLevel)
            .With(x => x.PrimaryGoal, request.PrimaryGoal)
            .Create();

        _athleteService.CreateForCurrentUserAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(AthletesController.GetCurrentUserProfile));
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Create_UserAlreadyHasProfile_ReturnsConflict()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test Athlete",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        _athleteService.CreateForCurrentUserAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_ValidRequest_CallsServiceWithMappedDto()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "  Test Athlete  ",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            Gender = "Male",
            HeightCm = 180m,
            WeightKg = 85m,
            ExperienceLevel = "Intermediate",
            PrimaryGoal = "ImprovePacing"
        };

        var createdDto = _fixture.Create<AthleteDto>();
        _athleteService.CreateForCurrentUserAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        await _sut.Create(request, CancellationToken.None);

        // Assert
        await _athleteService.Received(1).CreateForCurrentUserAsync(
            Arg.Is<CreateAthleteDto>(dto =>
                dto.Name == "Test Athlete" && // Trimmed
                dto.Gender == request.Gender &&
                dto.HeightCm == request.HeightCm &&
                dto.WeightKg == request.WeightKg &&
                dto.ExperienceLevel == request.ExperienceLevel &&
                dto.PrimaryGoal == request.PrimaryGoal),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateCurrentUserProfile Tests

    [Fact]
    public async Task UpdateCurrentUserProfile_ValidRequest_ReturnsOkWithUpdatedResponse()
    {
        // Arrange
        var athleteId = 1;
        var currentAthlete = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .Create();

        var request = new UpdateAthleteRequest
        {
            Name = "Updated Name",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            Gender = "Female",
            HeightCm = 165m,
            WeightKg = 60m,
            ExperienceLevel = "Advanced",
            PrimaryGoal = "CompetitionPrep"
        };

        var updatedDto = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .With(x => x.Name, request.Name)
            .With(x => x.ExperienceLevel, request.ExperienceLevel)
            .With(x => x.PrimaryGoal, request.PrimaryGoal)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(currentAthlete);
        _athleteService.UpdateAsync(athleteId, Arg.Any<UpdateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        var result = await _sut.UpdateCurrentUserProfile(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Id.Should().Be(athleteId);
        response.Name.Should().Be(request.Name);
        response.ExperienceLevel.Should().Be(request.ExperienceLevel);
        response.PrimaryGoal.Should().Be(request.PrimaryGoal);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_NoAthleteProfile_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateAthleteRequest
        {
            Name = "Test",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.UpdateCurrentUserProfile(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_CallsServiceWithCorrectIdAndMappedDto()
    {
        // Arrange
        var athleteId = 1;
        var currentAthlete = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .Create();

        var request = new UpdateAthleteRequest
        {
            Name = "  Updated Name  ",
            ExperienceLevel = "Advanced",
            PrimaryGoal = "BuildStrength"
        };

        var updatedDto = _fixture.Create<AthleteDto>();
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(currentAthlete);
        _athleteService.UpdateAsync(athleteId, Arg.Any<UpdateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        await _sut.UpdateCurrentUserProfile(request, CancellationToken.None);

        // Assert
        await _athleteService.Received(1).UpdateAsync(
            athleteId,
            Arg.Is<UpdateAthleteDto>(dto =>
                dto.Name == "Updated Name" && // Trimmed
                dto.ExperienceLevel == request.ExperienceLevel &&
                dto.PrimaryGoal == request.PrimaryGoal),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteCurrentUserProfile Tests

    [Fact]
    public async Task DeleteCurrentUserProfile_ValidProfile_ReturnsNoContent()
    {
        // Arrange
        var athleteId = 1;
        var currentAthlete = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(currentAthlete);
        _athleteService.DeleteAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.DeleteCurrentUserProfile(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCurrentUserProfile_NoAthleteProfile_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.DeleteCurrentUserProfile(CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteCurrentUserProfile_CallsServiceWithCorrectId()
    {
        // Arrange
        var athleteId = 1;
        var currentAthlete = _fixture.Build<AthleteDto>()
            .With(x => x.Id, athleteId)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(currentAthlete);
        _athleteService.DeleteAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.DeleteCurrentUserProfile(CancellationToken.None);

        // Assert
        await _athleteService.Received(1).DeleteAsync(athleteId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Response Mapping Tests

    [Fact]
    public async Task GetCurrentUserProfile_NeverExposesDateOfBirth()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetCurrentUserProfile(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;

        // AthleteResponse should not have a DateOfBirth property
        var properties = typeof(AthleteResponse).GetProperties();
        properties.Should().NotContain(p => p.Name == "DateOfBirth");
    }

    [Fact]
    public async Task GetCurrentUserProfile_ReturnsAgeNotDateOfBirth()
    {
        // Arrange
        var athleteDto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, 25)
            .Create();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetCurrentUserProfile(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Age.Should().Be(25);
    }

    #endregion
}
