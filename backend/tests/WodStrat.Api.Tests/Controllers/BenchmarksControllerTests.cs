using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.Tests.Customizations;
using WodStrat.Api.ViewModels.Benchmarks;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for BenchmarksController.
/// </summary>
public class BenchmarksControllerTests
{
    private readonly IFixture _fixture;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IAthleteService _athleteService;
    private readonly ICurrentUserService _currentUserService;
    private readonly BenchmarksController _sut;

    public BenchmarksControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteDtoCustomization())
            .Customize(new BenchmarkDtoCustomization());

        _benchmarkService = Substitute.For<IBenchmarkService>();
        _athleteService = Substitute.For<IAthleteService>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        _sut = new BenchmarksController(_benchmarkService, _athleteService, _currentUserService);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_UserHasAthleteWithBenchmarks_ReturnsOkWithBenchmarks()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        var benchmarks = _fixture.CreateMany<AthleteBenchmarkDto>(3).ToList();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetCurrentUserBenchmarksAsync(Arg.Any<CancellationToken>())
            .Returns(benchmarks);

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<AthleteBenchmarkResponse>>().Subject;
        response.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_AthleteHasNoBenchmarks_ReturnsEmptyList()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetCurrentUserBenchmarksAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AthleteBenchmarkDto>());

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<AthleteBenchmarkResponse>>().Subject;
        response.Should().BeEmpty();
    }

    #endregion

    #region GetSummary Tests

    [Fact]
    public async Task GetSummary_UserHasAthlete_ReturnsOkWithSummary()
    {
        // Arrange
        var summary = _fixture.Create<BenchmarkSummaryDto>();
        _benchmarkService.GetCurrentUserBenchmarkSummaryAsync(Arg.Any<CancellationToken>())
            .Returns(summary);

        // Act
        var result = await _sut.GetSummary(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BenchmarkSummaryResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSummary_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        _benchmarkService.GetCurrentUserBenchmarkSummaryAsync(Arg.Any<CancellationToken>())
            .Returns((BenchmarkSummaryDto?)null);

        // Act
        var result = await _sut.GetSummary(CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetDefinitions Tests

    [Fact]
    public async Task GetDefinitions_NoCategory_ReturnsAllDefinitions()
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(5).ToList();
        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetDefinitions(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject;
        response.Should().HaveCount(5);

        await _benchmarkService.Received(1).GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
        await _benchmarkService.DidNotReceive().GetDefinitionsByCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDefinitions_WithCategory_ReturnsFilteredDefinitions()
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(2).ToList();
        _benchmarkService.GetDefinitionsByCategoryAsync("Cardio", Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetDefinitions("Cardio", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject;
        response.Should().HaveCount(2);

        await _benchmarkService.DidNotReceive().GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
        await _benchmarkService.Received(1).GetDefinitionsByCategoryAsync("Cardio", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDefinitions_EmptyCategory_TreatedAsNoFilter()
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(3).ToList();
        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetDefinitions("", CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetDefinitionBySlug Tests

    [Fact]
    public async Task GetDefinitionBySlug_ValidSlug_ReturnsDefinition()
    {
        // Arrange
        var definition = _fixture.Create<BenchmarkDefinitionDto>();
        _benchmarkService.GetDefinitionBySlugAsync("fran", Arg.Any<CancellationToken>())
            .Returns(definition);

        // Act
        var result = await _sut.GetDefinitionBySlug("fran", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BenchmarkDefinitionResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDefinitionBySlug_InvalidSlug_ReturnsNotFound()
    {
        // Arrange
        _benchmarkService.GetDefinitionBySlugAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((BenchmarkDefinitionDto?)null);

        // Act
        var result = await _sut.GetDefinitionBySlug("nonexistent", CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = 1,
            Value = 180,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "Good form"
        };

        var createdBenchmark = _fixture.Create<AthleteBenchmarkDto>();
        _benchmarkService.RecordCurrentUserBenchmarkAsync(Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((createdBenchmark, false, false));

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        var request = _fixture.Create<RecordBenchmarkRequest>();
        _benchmarkService.RecordCurrentUserBenchmarkAsync(Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(((AthleteBenchmarkDto?)null, false, true));

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_DuplicateBenchmark_ReturnsConflict()
    {
        // Arrange
        var request = _fixture.Create<RecordBenchmarkRequest>();
        _benchmarkService.RecordCurrentUserBenchmarkAsync(Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(((AthleteBenchmarkDto?)null, true, false));

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_BenchmarkDefinitionNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = _fixture.Create<RecordBenchmarkRequest>();
        _benchmarkService.RecordCurrentUserBenchmarkAsync(Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(((AthleteBenchmarkDto?)null, false, false));

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ValidIdAndOwnership_ReturnsOk()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        var benchmark = _fixture.Create<AthleteBenchmarkDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarkByIdAsync(athleteDto.Id, 1, Arg.Any<CancellationToken>())
            .Returns(benchmark);

        // Act
        var result = await _sut.GetById(1, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetById(1, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_BenchmarkNotFound_ReturnsNotFound()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarkByIdAsync(athleteDto.Id, 999, Arg.Any<CancellationToken>())
            .Returns((AthleteBenchmarkDto?)null);

        // Act
        var result = await _sut.GetById(999, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        var request = new UpdateBenchmarkRequest
        {
            Value = 200,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "Improved"
        };
        var updatedBenchmark = _fixture.Create<AthleteBenchmarkDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.UpdateBenchmarkAsync(athleteDto.Id, 1, Arg.Any<UpdateBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedBenchmark);

        // Act
        var result = await _sut.Update(1, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        var request = _fixture.Create<UpdateBenchmarkRequest>();

        // Act
        var result = await _sut.Update(1, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Update_BenchmarkNotFound_ReturnsNotFound()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();
        var request = _fixture.Create<UpdateBenchmarkRequest>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.UpdateBenchmarkAsync(athleteDto.Id, 999, Arg.Any<UpdateBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((AthleteBenchmarkDto?)null);

        // Act
        var result = await _sut.Update(999, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.DeleteBenchmarkAsync(athleteDto.Id, 1, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_UserHasNoAthlete_ReturnsNotFound()
    {
        // Arrange
        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Delete(1, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Delete_BenchmarkNotFound_ReturnsNotFound()
    {
        // Arrange
        var athleteDto = _fixture.Create<AthleteDto>();

        _athleteService.GetCurrentUserAthleteAsync(Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.DeleteBenchmarkAsync(athleteDto.Id, 999, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Delete(999, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion
}
