using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Services;
using WodStrat.Services.Tests.Customizations;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for AthleteService.
/// </summary>
public class AthleteServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly AthleteService _sut;

    public AthleteServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _sut = new AthleteService(_database);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsAthleteDto()
    {
        // Arrange
        var athlete = _fixture.Create<Athlete>();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByIdAsync(athlete.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(athlete.Id);
        result.Name.Should().Be(athlete.Name);
        result.ExperienceLevel.Should().Be(athlete.ExperienceLevel.ToString());
        result.PrimaryGoal.Should().Be(athlete.PrimaryGoal.ToString());
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Athlete>().AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedAthlete_ReturnsNull()
    {
        // Arrange
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.IsDeleted, true)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByIdAsync(athlete.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CalculatesAgeCorrectly()
    {
        // Arrange
        var expectedAge = 30;
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-expectedAge)))
            .With(a => a.IsDeleted, false)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByIdAsync(athlete.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Age.Should().Be(expectedAge);
    }

    [Fact]
    public async Task GetByIdAsync_NullDateOfBirth_ReturnsNullAge()
    {
        // Arrange
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.DateOfBirth, (DateOnly?)null)
            .With(a => a.IsDeleted, false)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByIdAsync(athlete.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Age.Should().BeNull();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_ValidUserId_ReturnsAthleteDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.UserId, userId)
            .With(a => a.IsDeleted, false)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(athlete.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_InvalidUserId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Athlete>().AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_DeletedAthlete_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.UserId, userId)
            .With(a => a.IsDeleted, true)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedAthleteDto()
    {
        // Arrange
        var dto = new CreateAthleteDto
        {
            Name = "Test Athlete",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            Gender = "Male",
            HeightCm = 180m,
            WeightKg = 85m,
            ExperienceLevel = "Intermediate",
            PrimaryGoal = "ImprovePacing"
        };

        Athlete? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Athlete>()))
            .Do(x => savedEntity = x.Arg<Athlete>());

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        result.ExperienceLevel.Should().Be(dto.ExperienceLevel);
        result.PrimaryGoal.Should().Be(dto.PrimaryGoal);
        result.Id.Should().NotBeEmpty();

        savedEntity.Should().NotBeNull();
        savedEntity!.IsDeleted.Should().BeFalse();

        _database.Received(1).Add(Arg.Any<Athlete>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var dto = new CreateAthleteDto
        {
            Name = "Test Athlete",
            ExperienceLevel = "Beginner",
            PrimaryGoal = "GeneralFitness"
        };

        Athlete? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Athlete>()))
            .Do(x => savedEntity = x.Arg<Athlete>());

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _sut.CreateAsync(dto);

        var afterCreate = DateTime.UtcNow;

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        savedEntity.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task CreateAsync_ParsesEnumsCorrectly()
    {
        // Arrange
        var dto = new CreateAthleteDto
        {
            Name = "Test Athlete",
            ExperienceLevel = "advanced", // lowercase to test case-insensitive parsing
            PrimaryGoal = "buildstrength"
        };

        Athlete? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Athlete>()))
            .Do(x => savedEntity = x.Arg<Athlete>());

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.ExperienceLevel.Should().Be(ExperienceLevel.Advanced);
        savedEntity.PrimaryGoal.Should().Be(AthleteGoal.BuildStrength);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidIdAndDto_ReturnsUpdatedAthleteDto()
    {
        // Arrange
        var athlete = _fixture.Create<Athlete>();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        var dto = new UpdateAthleteDto
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
        var result = await _sut.UpdateAsync(athlete.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(dto.Name);
        result.ExperienceLevel.Should().Be(dto.ExperienceLevel);
        result.PrimaryGoal.Should().Be(dto.PrimaryGoal);

        _database.Received(1).Update(Arg.Any<Athlete>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Athlete>().AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        var dto = new UpdateAthleteDto { Name = "Test" };

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<Athlete>());
    }

    [Fact]
    public async Task UpdateAsync_DeletedAthlete_ReturnsNull()
    {
        // Arrange
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.IsDeleted, true)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        var dto = new UpdateAthleteDto { Name = "Test" };

        // Act
        var result = await _sut.UpdateAsync(athlete.Id, dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<Athlete>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.UpdatedAt, originalUpdatedAt)
            .With(a => a.IsDeleted, false)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        var dto = new UpdateAthleteDto { Name = "Test", ExperienceLevel = "Beginner", PrimaryGoal = "GeneralFitness" };

        // Act
        await _sut.UpdateAsync(athlete.Id, dto);

        // Assert
        athlete.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var athlete = _fixture.Create<Athlete>();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.DeleteAsync(athlete.Id);

        // Assert
        result.Should().BeTrue();
        athlete.IsDeleted.Should().BeTrue();

        _database.Received(1).Update(Arg.Any<Athlete>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var queryable = Array.Empty<Athlete>().AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<Athlete>());
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedAthlete_ReturnsFalse()
    {
        // Arrange
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.IsDeleted, true)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        var result = await _sut.DeleteAsync(athlete.Id);

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<Athlete>());
    }

    [Fact]
    public async Task DeleteAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var athlete = _fixture.Build<Athlete>()
            .With(a => a.UpdatedAt, originalUpdatedAt)
            .With(a => a.IsDeleted, false)
            .Without(a => a.Benchmarks)
            .Create();
        var queryable = new[] { athlete }.AsQueryable().BuildMock();
        _database.Get<Athlete>().Returns(queryable);

        // Act
        await _sut.DeleteAsync(athlete.Id);

        // Assert
        athlete.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion
}
