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
    private readonly AthletesController _sut;

    public AthletesControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteDtoCustomization());

        _athleteService = Substitute.For<IAthleteService>();
        _sut = new AthletesController(_athleteService);
    }

    #region GetById Tests

    [Fact]
    public async Task GetById_ValidId_ReturnsOkWithAthleteResponse()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        _athleteService.GetByIdAsync(athleteDto.Id, Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetById(athleteDto.Id, CancellationToken.None);

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
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _athleteService.GetByIdAsync(invalidId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetById(invalidId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_MapsAllDtoPropertiesToResponse()
    {
        // Arrange
        var athleteDto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, 30)
            .With(x => x.Gender, "Female")
            .With(x => x.HeightCm, 165m)
            .With(x => x.WeightKg, 60m)
            .Create();

        _athleteService.GetByIdAsync(athleteDto.Id, Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetById(athleteDto.Id, CancellationToken.None);

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

    #region GetCurrentUserProfile Tests

    [Fact]
    public void GetCurrentUserProfile_ReturnsNotImplemented()
    {
        // Act
        var result = _sut.GetCurrentUserProfile();

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(501);
    }

    [Fact]
    public void GetCurrentUserProfile_ReturnsProperProblemDetails()
    {
        // Act
        var result = _sut.GetCurrentUserProfile();

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        var responseValue = statusCodeResult.Value;
        responseValue.Should().NotBeNull();
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

        _athleteService.CreateAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(AthletesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(createdDto.Id);

        var response = createdResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Name.Should().Be(request.Name);
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
        _athleteService.CreateAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        await _sut.Create(request, CancellationToken.None);

        // Assert
        await _athleteService.Received(1).CreateAsync(
            Arg.Is<CreateAthleteDto>(dto =>
                dto.Name == "Test Athlete" && // Trimmed
                dto.Gender == request.Gender &&
                dto.HeightCm == request.HeightCm &&
                dto.WeightKg == request.WeightKg &&
                dto.ExperienceLevel == request.ExperienceLevel &&
                dto.PrimaryGoal == request.PrimaryGoal),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Returns201StatusCode()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        var createdDto = _fixture.Create<AthleteDto>();
        _athleteService.CreateAsync(Arg.Any<CreateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(createdDto);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidIdAndRequest_ReturnsOkWithUpdatedResponse()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
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

        _athleteService.UpdateAsync(athleteId, Arg.Any<UpdateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        var result = await _sut.Update(athleteId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Id.Should().Be(athleteId);
        response.Name.Should().Be(request.Name);
        response.ExperienceLevel.Should().Be(request.ExperienceLevel);
        response.PrimaryGoal.Should().Be(request.PrimaryGoal);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var request = new UpdateAthleteRequest
        {
            Name = "Test",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        _athleteService.UpdateAsync(invalidId, Arg.Any<UpdateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Update(invalidId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_CallsServiceWithCorrectIdAndMappedDto()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        var request = new UpdateAthleteRequest
        {
            Name = "  Updated Name  ",
            ExperienceLevel = "Advanced",
            PrimaryGoal = "BuildStrength"
        };

        var updatedDto = _fixture.Create<AthleteDto>();
        _athleteService.UpdateAsync(athleteId, Arg.Any<UpdateAthleteDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        await _sut.Update(athleteId, request, CancellationToken.None);

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

    #region Delete Tests

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        _athleteService.DeleteAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Delete(athleteId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _athleteService.DeleteAsync(invalidId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Delete(invalidId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        _athleteService.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_CallsServiceWithCorrectId()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        _athleteService.DeleteAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.Delete(athleteId, CancellationToken.None);

        // Assert
        await _athleteService.Received(1).DeleteAsync(athleteId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Response Mapping Tests

    [Fact]
    public async Task GetById_NeverExposesDateOfBirth()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        _athleteService.GetByIdAsync(athleteDto.Id, Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetById(athleteDto.Id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;

        // AthleteResponse should not have a DateOfBirth property
        var properties = typeof(AthleteResponse).GetProperties();
        properties.Should().NotContain(p => p.Name == "DateOfBirth");
    }

    [Fact]
    public async Task GetById_ReturnsAgeNotDateOfBirth()
    {
        // Arrange
        var athleteDto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, 25)
            .Create();

        _athleteService.GetByIdAsync(athleteDto.Id, Arg.Any<CancellationToken>())
            .Returns(athleteDto);

        // Act
        var result = await _sut.GetById(athleteDto.Id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteResponse>().Subject;
        response.Age.Should().Be(25);
    }

    #endregion
}
