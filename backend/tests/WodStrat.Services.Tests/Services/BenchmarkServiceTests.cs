using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Services;
using WodStrat.Services.Tests.Customizations;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for BenchmarkService.
/// </summary>
public class BenchmarkServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAthleteService _athleteService;
    private readonly BenchmarkService _sut;

    public BenchmarkServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new BenchmarkCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _athleteService = Substitute.For<IAthleteService>();
        _sut = new BenchmarkService(_database, _currentUserService, _athleteService);
    }

    #region GetAllDefinitionsAsync Tests

    [Fact]
    public async Task GetAllDefinitionsAsync_ReturnsActiveDefinitions()
    {
        // Arrange
        var activeDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();
        var inactiveDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, false)
            .Create();

        var queryable = new[] { activeDefinition, inactiveDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllDefinitionsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeDefinition.Id);
    }

    [Fact]
    public async Task GetAllDefinitionsAsync_OrdersByDisplayOrderThenName()
    {
        // Arrange
        var definition1 = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.DisplayOrder, 2)
            .With(x => x.Name, "Alpha")
            .Create();
        var definition2 = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.DisplayOrder, 1)
            .With(x => x.Name, "Beta")
            .Create();
        var definition3 = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.DisplayOrder, 2)
            .With(x => x.Name, "Zeta")
            .Create();

        var queryable = new[] { definition1, definition2, definition3 }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllDefinitionsAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Beta"); // DisplayOrder 1
        result[1].Name.Should().Be("Alpha"); // DisplayOrder 2, Alpha < Zeta
        result[2].Name.Should().Be("Zeta"); // DisplayOrder 2, Zeta
    }

    [Fact]
    public async Task GetAllDefinitionsAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var queryable = Array.Empty<BenchmarkDefinition>().AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllDefinitionsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDefinitionsAsync_MapsAllPropertiesToDto()
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Category, BenchmarkCategory.Strength)
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllDefinitionsAsync();

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(definition.Id);
        dto.Name.Should().Be(definition.Name);
        dto.Slug.Should().Be(definition.Slug);
        dto.Description.Should().Be(definition.Description);
        dto.Category.Should().Be("Strength");
        dto.MetricType.Should().Be("Weight");
        dto.Unit.Should().Be(definition.Unit);
        dto.DisplayOrder.Should().Be(definition.DisplayOrder);
    }

    #endregion

    #region GetDefinitionsByCategoryAsync Tests

    [Fact]
    public async Task GetDefinitionsByCategoryAsync_ValidCategory_ReturnsFilteredDefinitions()
    {
        // Arrange
        var cardioDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .Create();
        var strengthDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Category, BenchmarkCategory.Strength)
            .Create();

        var queryable = new[] { cardioDefinition, strengthDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionsByCategoryAsync("Cardio");

        // Assert
        result.Should().HaveCount(1);
        result.First().Category.Should().Be("Cardio");
    }

    [Theory]
    [InlineData("cardio")]
    [InlineData("CARDIO")]
    [InlineData("Cardio")]
    public async Task GetDefinitionsByCategoryAsync_CaseInsensitive_ReturnsCorrectResults(string category)
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionsByCategoryAsync(category);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDefinitionsByCategoryAsync_InvalidCategory_ReturnsEmptyList()
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionsByCategoryAsync("InvalidCategory");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDefinitionsByCategoryAsync_ExcludesInactiveDefinitions()
    {
        // Arrange
        var activeDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .Create();
        var inactiveDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, false)
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .Create();

        var queryable = new[] { activeDefinition, inactiveDefinition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionsByCategoryAsync("Cardio");

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeDefinition.Id);
    }

    #endregion

    #region GetDefinitionBySlugAsync Tests

    [Fact]
    public async Task GetDefinitionBySlugAsync_ValidSlug_ReturnsDefinition()
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Slug, "fran")
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionBySlugAsync("fran");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("fran");
    }

    [Fact]
    public async Task GetDefinitionBySlugAsync_InvalidSlug_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<BenchmarkDefinition>().AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionBySlugAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDefinitionBySlugAsync_InactiveDefinition_ReturnsNull()
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, false)
            .With(x => x.Slug, "fran")
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionBySlugAsync("fran");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDefinitionBySlugAsync_NormalizesToLowercase()
    {
        // Arrange
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .With(x => x.Slug, "fran")
            .Create();

        var queryable = new[] { definition }.AsQueryable().BuildMock();
        _database.Get<BenchmarkDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetDefinitionBySlugAsync("FRAN");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetAthleteBenchmarksAsync Tests

    [Fact]
    public async Task GetAthleteBenchmarksAsync_ValidAthleteId_ReturnsBenchmarks()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.BenchmarkDefinitionId, definition.Id)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarksAsync(athleteId);

        // Assert
        result.Should().HaveCount(1);
        result.First().AthleteId.Should().Be(athleteId);
    }

    [Fact]
    public async Task GetAthleteBenchmarksAsync_ExcludesDeletedBenchmarks()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.IsActive, true)
            .Create();
        var activeBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();
        var deletedBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, true)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { activeBenchmark, deletedBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarksAsync(athleteId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeBenchmark.Id);
    }

    [Fact]
    public async Task GetAthleteBenchmarksAsync_NoMatchingAthleteId_ReturnsEmptyList()
    {
        // Arrange
        var queryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarksAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAthleteBenchmarksAsync_IncludesFormattedValue()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.MetricType, BenchmarkMetricType.Time)
            .With(x => x.Unit, "seconds")
            .Create();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.Value, 195m) // 3:15
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarksAsync(athleteId);

        // Assert
        result.First().FormattedValue.Should().Be("3:15");
    }

    #endregion

    #region GetAthleteBenchmarkByIdAsync Tests

    [Fact]
    public async Task GetAthleteBenchmarkByIdAsync_ValidIds_ReturnsBenchmark()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarkByIdAsync(athleteId, benchmarkId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(benchmarkId);
    }

    [Fact]
    public async Task GetAthleteBenchmarkByIdAsync_InvalidBenchmarkId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarkByIdAsync(_fixture.Create<int>(), _fixture.Create<int>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAthleteBenchmarkByIdAsync_WrongAthleteId_ReturnsNull()
    {
        // Arrange
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, _fixture.Create<int>()) // Different athlete
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarkByIdAsync(_fixture.Create<int>(), benchmarkId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAthleteBenchmarkByIdAsync_DeletedBenchmark_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, true)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetAthleteBenchmarkByIdAsync(athleteId, benchmarkId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetBenchmarkSummaryAsync Tests

    [Fact]
    public async Task GetBenchmarkSummaryAsync_ReturnsSummaryWithCorrectTotalCount()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmarks = Enumerable.Range(0, 5)
            .Select(_ => _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, definition)
                .Create())
            .ToArray();

        var queryable = benchmarks.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetBenchmarkSummaryAsync(athleteId);

        // Assert
        result.TotalBenchmarks.Should().Be(5);
        result.AthleteId.Should().Be(athleteId);
    }

    [Fact]
    public async Task GetBenchmarkSummaryAsync_MeetsMinimumRequirement_WhenThreeOrMoreBenchmarks()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmarks = Enumerable.Range(0, 3)
            .Select(_ => _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, definition)
                .Create())
            .ToArray();

        var queryable = benchmarks.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetBenchmarkSummaryAsync(athleteId);

        // Assert
        result.MeetsMinimumRequirement.Should().BeTrue();
        result.MinimumRequired.Should().Be(3);
    }

    [Fact]
    public async Task GetBenchmarkSummaryAsync_DoesNotMeetMinimumRequirement_WhenFewerThanThreeBenchmarks()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmarks = Enumerable.Range(0, 2)
            .Select(_ => _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, definition)
                .Create())
            .ToArray();

        var queryable = benchmarks.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetBenchmarkSummaryAsync(athleteId);

        // Assert
        result.MeetsMinimumRequirement.Should().BeFalse();
    }

    [Fact]
    public async Task GetBenchmarkSummaryAsync_GroupsBenchmarksByCategory()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var cardioDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .Create();
        var strengthDefinition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Category, BenchmarkCategory.Strength)
            .Create();

        var benchmarks = new[]
        {
            _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, cardioDefinition)
                .Create(),
            _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, cardioDefinition)
                .Create(),
            _fixture.Build<AthleteBenchmark>()
                .With(x => x.AthleteId, athleteId)
                .With(x => x.IsDeleted, false)
                .With(x => x.BenchmarkDefinition, strengthDefinition)
                .Create()
        };

        var queryable = benchmarks.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetBenchmarkSummaryAsync(athleteId);

        // Assert
        result.BenchmarksByCategory.Should().ContainKey("Cardio");
        result.BenchmarksByCategory.Should().ContainKey("Strength");
        result.BenchmarksByCategory["Cardio"].Should().Be(2);
        result.BenchmarksByCategory["Strength"].Should().Be(1);
    }

    [Fact]
    public async Task GetBenchmarkSummaryAsync_IncludesBenchmarkDetails()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var benchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { benchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.GetBenchmarkSummaryAsync(athleteId);

        // Assert
        result.Benchmarks.Should().HaveCount(1);
        result.Benchmarks.First().Id.Should().Be(benchmark.Id);
    }

    #endregion

    #region RecordBenchmarkAsync Tests

    [Fact]
    public async Task RecordBenchmarkAsync_NewBenchmark_ReturnsCreatedBenchmark()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definitionId = _fixture.Create<int>();
        var dto = new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = definitionId,
            Value = 195.5m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "RX"
        };

        // No existing benchmark
        var emptyQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(emptyQueryable);

        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Id, definitionId)
            .With(x => x.MetricType, BenchmarkMetricType.Time)
            .Create();

        AthleteBenchmark? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<AthleteBenchmark>()))
            .Do(x => savedEntity = x.Arg<AthleteBenchmark>());

        // Setup for fetching the saved benchmark
        _database.Get<AthleteBenchmark>().Returns(x =>
        {
            if (savedEntity != null)
            {
                savedEntity.BenchmarkDefinition = definition;
                return new[] { savedEntity }.AsQueryable().BuildMock();
            }
            return emptyQueryable;
        });

        // Act
        var (result, isDuplicate) = await _sut.RecordBenchmarkAsync(athleteId, dto);

        // Assert
        isDuplicate.Should().BeFalse();
        result.Should().NotBeNull();
        savedEntity.Should().NotBeNull();
        savedEntity!.AthleteId.Should().Be(athleteId);
        savedEntity.BenchmarkDefinitionId.Should().Be(definitionId);
        savedEntity.Value.Should().Be(195.5m);
        savedEntity.Notes.Should().Be("RX");

        _database.Received(1).Add(Arg.Any<AthleteBenchmark>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordBenchmarkAsync_DuplicateBenchmark_ReturnsDuplicateFlag()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definitionId = _fixture.Create<int>();
        var dto = new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = definitionId,
            Value = 195.5m
        };

        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.BenchmarkDefinitionId, definitionId)
            .With(x => x.IsDeleted, false)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var (result, isDuplicate) = await _sut.RecordBenchmarkAsync(athleteId, dto);

        // Assert
        isDuplicate.Should().BeTrue();
        result.Should().BeNull();
        _database.DidNotReceive().Add(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task RecordBenchmarkAsync_DeletedExistingBenchmark_AllowsNewRecord()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var definitionId = _fixture.Create<int>();
        var dto = new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = definitionId,
            Value = 195.5m
        };

        var deletedBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.BenchmarkDefinitionId, definitionId)
            .With(x => x.IsDeleted, true) // Deleted
            .Create();

        // First call returns deleted benchmark (shouldn't match), second call returns saved entity
        var emptyQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        var definition = _fixture.Build<BenchmarkDefinition>()
            .With(x => x.Id, definitionId)
            .Create();

        AthleteBenchmark? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<AthleteBenchmark>()))
            .Do(x => savedEntity = x.Arg<AthleteBenchmark>());

        _database.Get<AthleteBenchmark>().Returns(x =>
        {
            if (savedEntity != null)
            {
                savedEntity.BenchmarkDefinition = definition;
                return new[] { savedEntity }.AsQueryable().BuildMock();
            }
            return emptyQueryable;
        });

        // Act
        var (result, isDuplicate) = await _sut.RecordBenchmarkAsync(athleteId, dto);

        // Assert
        isDuplicate.Should().BeFalse();
        _database.Received(1).Add(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task RecordBenchmarkAsync_NoRecordedAt_DefaultsToToday()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var dto = new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m,
            RecordedAt = null // No date provided
        };

        var emptyQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(emptyQueryable);

        AthleteBenchmark? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<AthleteBenchmark>()))
            .Do(x => savedEntity = x.Arg<AthleteBenchmark>());

        // Act
        await _sut.RecordBenchmarkAsync(athleteId, dto);

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.RecordedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public async Task RecordBenchmarkAsync_SetsTimestamps()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var dto = new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m
        };

        var emptyQueryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(emptyQueryable);

        AthleteBenchmark? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<AthleteBenchmark>()))
            .Do(x => savedEntity = x.Arg<AthleteBenchmark>());

        var beforeCreate = DateTime.UtcNow;

        // Act
        await _sut.RecordBenchmarkAsync(athleteId, dto);

        var afterCreate = DateTime.UtcNow;

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        savedEntity.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        savedEntity.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region UpdateBenchmarkAsync Tests

    [Fact]
    public async Task UpdateBenchmarkAsync_ValidIds_ReturnsUpdatedBenchmark()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.Value, 200m)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var dto = new UpdateBenchmarkDto
        {
            Value = 180m, // Improved time
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "PR"
        };

        // Act
        var result = await _sut.UpdateBenchmarkAsync(athleteId, benchmarkId, dto);

        // Assert
        result.Should().NotBeNull();
        existingBenchmark.Value.Should().Be(180m);
        existingBenchmark.Notes.Should().Be("PR");

        _database.Received(1).Update(Arg.Any<AthleteBenchmark>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateBenchmarkAsync_InvalidBenchmarkId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var dto = new UpdateBenchmarkDto { Value = 180m };

        // Act
        var result = await _sut.UpdateBenchmarkAsync(_fixture.Create<int>(), _fixture.Create<int>(), dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task UpdateBenchmarkAsync_WrongAthleteId_ReturnsNull()
    {
        // Arrange
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, _fixture.Create<int>()) // Different athlete
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var dto = new UpdateBenchmarkDto { Value = 180m };

        // Act
        var result = await _sut.UpdateBenchmarkAsync(_fixture.Create<int>(), benchmarkId, dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task UpdateBenchmarkAsync_DeletedBenchmark_ReturnsNull()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var definition = _fixture.Create<BenchmarkDefinition>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, true)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var dto = new UpdateBenchmarkDto { Value = 180m };

        // Act
        var result = await _sut.UpdateBenchmarkAsync(athleteId, benchmarkId, dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task UpdateBenchmarkAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var definition = _fixture.Create<BenchmarkDefinition>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.UpdatedAt, originalUpdatedAt)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var dto = new UpdateBenchmarkDto { Value = 180m };

        // Act
        await _sut.UpdateBenchmarkAsync(athleteId, benchmarkId, dto);

        // Assert
        existingBenchmark.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateBenchmarkAsync_OnlyRecordedAtProvided_UpdatesOnlyRecordedAt()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var originalValue = 200m;
        var originalNotes = "Original notes";
        var definition = _fixture.Create<BenchmarkDefinition>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.Value, originalValue)
            .With(x => x.Notes, originalNotes)
            .With(x => x.IsDeleted, false)
            .With(x => x.BenchmarkDefinition, definition)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var dto = new UpdateBenchmarkDto
        {
            Value = 180m,
            RecordedAt = newDate,
            Notes = null // Clear notes
        };

        // Act
        await _sut.UpdateBenchmarkAsync(athleteId, benchmarkId, dto);

        // Assert
        existingBenchmark.RecordedAt.Should().Be(newDate);
        existingBenchmark.Value.Should().Be(180m);
        existingBenchmark.Notes.Should().BeNull();
    }

    #endregion

    #region DeleteBenchmarkAsync Tests

    [Fact]
    public async Task DeleteBenchmarkAsync_ValidIds_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, false)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.DeleteBenchmarkAsync(athleteId, benchmarkId);

        // Assert
        result.Should().BeTrue();
        existingBenchmark.IsDeleted.Should().BeTrue();

        _database.Received(1).Update(Arg.Any<AthleteBenchmark>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteBenchmarkAsync_InvalidBenchmarkId_ReturnsFalse()
    {
        // Arrange
        var queryable = Array.Empty<AthleteBenchmark>().AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.DeleteBenchmarkAsync(_fixture.Create<int>(), _fixture.Create<int>());

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task DeleteBenchmarkAsync_WrongAthleteId_ReturnsFalse()
    {
        // Arrange
        var benchmarkId = _fixture.Create<int>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, _fixture.Create<int>()) // Different athlete
            .With(x => x.IsDeleted, false)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.DeleteBenchmarkAsync(_fixture.Create<int>(), benchmarkId);

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task DeleteBenchmarkAsync_AlreadyDeleted_ReturnsFalse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.IsDeleted, true) // Already deleted
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        var result = await _sut.DeleteBenchmarkAsync(athleteId, benchmarkId);

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<AthleteBenchmark>());
    }

    [Fact]
    public async Task DeleteBenchmarkAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var existingBenchmark = _fixture.Build<AthleteBenchmark>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.UpdatedAt, originalUpdatedAt)
            .With(x => x.IsDeleted, false)
            .Create();

        var queryable = new[] { existingBenchmark }.AsQueryable().BuildMock();
        _database.Get<AthleteBenchmark>().Returns(queryable);

        // Act
        await _sut.DeleteBenchmarkAsync(athleteId, benchmarkId);

        // Assert
        existingBenchmark.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion
}
