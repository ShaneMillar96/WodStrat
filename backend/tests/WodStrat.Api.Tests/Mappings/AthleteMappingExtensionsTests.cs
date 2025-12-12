using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using WodStrat.Api.Mappings;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Services.Dtos;
using Xunit;

namespace WodStrat.Api.Tests.Mappings;

/// <summary>
/// Unit tests for API layer AthleteMappingExtensions.
/// </summary>
public class AthleteMappingExtensionsTests
{
    private readonly IFixture _fixture;

    public AthleteMappingExtensionsTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteDtoCustomization());
    }

    #region ToResponse Tests

    [Fact]
    public void ToResponse_ValidDto_MapsAllProperties()
    {
        // Arrange
        var dto = _fixture.Create<AthleteDto>();

        // Act
        var response = dto.ToResponse();

        // Assert
        response.Id.Should().Be(dto.Id);
        response.Name.Should().Be(dto.Name);
        response.Age.Should().Be(dto.Age);
        response.Gender.Should().Be(dto.Gender);
        response.HeightCm.Should().Be(dto.HeightCm);
        response.WeightKg.Should().Be(dto.WeightKg);
        response.ExperienceLevel.Should().Be(dto.ExperienceLevel);
        response.PrimaryGoal.Should().Be(dto.PrimaryGoal);
        response.CreatedAt.Should().Be(dto.CreatedAt);
        response.UpdatedAt.Should().Be(dto.UpdatedAt);
    }

    [Fact]
    public void ToResponse_NullAge_MapsNullAge()
    {
        // Arrange
        var dto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, (int?)null)
            .Create();

        // Act
        var response = dto.ToResponse();

        // Assert
        response.Age.Should().BeNull();
    }

    [Fact]
    public void ToResponse_NullOptionalFields_MapsNullValues()
    {
        // Arrange
        var dto = _fixture.Build<AthleteDto>()
            .With(x => x.Age, (int?)null)
            .With(x => x.Gender, (string?)null)
            .With(x => x.HeightCm, (decimal?)null)
            .With(x => x.WeightKg, (decimal?)null)
            .Create();

        // Act
        var response = dto.ToResponse();

        // Assert
        response.Age.Should().BeNull();
        response.Gender.Should().BeNull();
        response.HeightCm.Should().BeNull();
        response.WeightKg.Should().BeNull();
    }

    [Fact]
    public void ToResponse_ResponseDoesNotContainDateOfBirth()
    {
        // Arrange
        var dto = _fixture.Create<AthleteDto>();

        // Act
        var response = dto.ToResponse();

        // Assert
        var properties = typeof(AthleteResponse).GetProperties();
        properties.Should().NotContain(p => p.Name == "DateOfBirth");
    }

    #endregion

    #region CreateAthleteRequest ToDto Tests

    [Fact]
    public void ToDto_ValidCreateRequest_MapsAllProperties()
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

        // Act
        var dto = request.ToDto();

        // Assert
        dto.Name.Should().Be("Test Athlete");
        dto.DateOfBirth.Should().Be(request.DateOfBirth);
        dto.Gender.Should().Be(request.Gender);
        dto.HeightCm.Should().Be(request.HeightCm);
        dto.WeightKg.Should().Be(request.WeightKg);
        dto.ExperienceLevel.Should().Be(request.ExperienceLevel);
        dto.PrimaryGoal.Should().Be(request.PrimaryGoal);
    }

    [Fact]
    public void ToDto_CreateRequestWithWhitespaceName_TrimsName()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "  Test Athlete  ",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        // Act
        var dto = request.ToDto();

        // Assert
        dto.Name.Should().Be("Test Athlete");
    }

    [Fact]
    public void ToDto_CreateRequestWithNullOptionalFields_MapsNullValues()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test",
            DateOfBirth = null,
            Gender = null,
            HeightCm = null,
            WeightKg = null,
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        // Act
        var dto = request.ToDto();

        // Assert
        dto.DateOfBirth.Should().BeNull();
        dto.Gender.Should().BeNull();
        dto.HeightCm.Should().BeNull();
        dto.WeightKg.Should().BeNull();
    }

    #endregion

    #region UpdateAthleteRequest ToDto Tests

    [Fact]
    public void ToDto_ValidUpdateRequest_MapsAllProperties()
    {
        // Arrange
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

        // Act
        var dto = request.ToDto();

        // Assert
        dto.Name.Should().Be("Updated Name");
        dto.DateOfBirth.Should().Be(request.DateOfBirth);
        dto.Gender.Should().Be(request.Gender);
        dto.HeightCm.Should().Be(request.HeightCm);
        dto.WeightKg.Should().Be(request.WeightKg);
        dto.ExperienceLevel.Should().Be(request.ExperienceLevel);
        dto.PrimaryGoal.Should().Be(request.PrimaryGoal);
    }

    [Fact]
    public void ToDto_UpdateRequestWithWhitespaceName_TrimsName()
    {
        // Arrange
        var request = new UpdateAthleteRequest
        {
            Name = "  Updated Name  ",
            ExperienceLevel = "Advanced",
            PrimaryGoal = "CompetitionPrep"
        };

        // Act
        var dto = request.ToDto();

        // Assert
        dto.Name.Should().Be("Updated Name");
    }

    [Fact]
    public void ToDto_UpdateRequestWithNullOptionalFields_MapsNullValues()
    {
        // Arrange
        var request = new UpdateAthleteRequest
        {
            Name = "Test",
            DateOfBirth = null,
            Gender = null,
            HeightCm = null,
            WeightKg = null,
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        // Act
        var dto = request.ToDto();

        // Assert
        dto.DateOfBirth.Should().BeNull();
        dto.Gender.Should().BeNull();
        dto.HeightCm.Should().BeNull();
        dto.WeightKg.Should().BeNull();
    }

    #endregion

    #region Cross-layer Mapping Consistency Tests

    [Fact]
    public void CreateRequestToDto_DtoContainsDateOfBirth()
    {
        // Arrange
        var request = new CreateAthleteRequest
        {
            Name = "Test",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        // Act
        var dto = request.ToDto();

        // Assert
        // CreateAthleteDto should contain DateOfBirth for age calculation
        dto.DateOfBirth.Should().NotBeNull();
        dto.DateOfBirth.Should().Be(request.DateOfBirth);
    }

    [Fact]
    public void ResponseDoesNotExposePrivateData()
    {
        // Assert
        var responseProperties = typeof(AthleteResponse).GetProperties();
        responseProperties.Should().NotContain(p => p.Name == "DateOfBirth");
        responseProperties.Should().NotContain(p => p.Name == "UserId");
        responseProperties.Should().NotContain(p => p.Name == "IsDeleted");
    }

    #endregion
}
