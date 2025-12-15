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
/// Unit tests for AthleteBenchmarksController.
/// </summary>
public class AthleteBenchmarksControllerTests
{
    private readonly IFixture _fixture;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IAthleteService _athleteService;
    private readonly AthleteBenchmarksController _sut;

    public AthleteBenchmarksControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteDtoCustomization())
            .Customize(new BenchmarkDtoCustomization());

        _benchmarkService = Substitute.For<IBenchmarkService>();
        _athleteService = Substitute.For<IAthleteService>();
        _sut = new AthleteBenchmarksController(_benchmarkService, _athleteService);
    }

    #region GetByAthlete Tests

    [Fact]
    public async Task GetByAthlete_ValidAthleteId_ReturnsOkWithBenchmarks()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var benchmarks = _fixture.CreateMany<AthleteBenchmarkDto>(3).ToList();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarksAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(benchmarks);

        // Act
        var result = await _sut.GetByAthlete(athleteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<AthleteBenchmarkResponse>>().Subject;
        response.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByAthlete_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetByAthlete(athleteId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        await _benchmarkService.DidNotReceive().GetAthleteBenchmarksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByAthlete_NoBenchmarks_ReturnsEmptyList()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarksAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(new List<AthleteBenchmarkDto>());

        // Act
        var result = await _sut.GetByAthlete(athleteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<AthleteBenchmarkResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByAthlete_MapsAllDtoPropertiesToResponse()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var benchmarkDto = _fixture.Build<AthleteBenchmarkDto>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.BenchmarkName, "Fran")
            .With(x => x.BenchmarkCategory, "HeroWod")
            .With(x => x.Value, 195.5m)
            .With(x => x.FormattedValue, "3:15")
            .With(x => x.Unit, "seconds")
            .With(x => x.Notes, "RX")
            .Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarksAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(new List<AthleteBenchmarkDto> { benchmarkDto });

        // Act
        var result = await _sut.GetByAthlete(athleteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<AthleteBenchmarkResponse>>().Subject.First();

        response.Id.Should().Be(benchmarkDto.Id);
        response.AthleteId.Should().Be(athleteId);
        response.BenchmarkDefinitionId.Should().Be(benchmarkDto.BenchmarkDefinitionId);
        response.BenchmarkName.Should().Be("Fran");
        response.BenchmarkCategory.Should().Be("HeroWod");
        response.Value.Should().Be(195.5m);
        response.FormattedValue.Should().Be("3:15");
        response.Unit.Should().Be("seconds");
        response.Notes.Should().Be("RX");
    }

    #endregion

    #region GetSummary Tests

    [Fact]
    public async Task GetSummary_ValidAthleteId_ReturnsOkWithSummary()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var summaryDto = _fixture.Build<BenchmarkSummaryDto>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.TotalBenchmarks, 5)
            .With(x => x.MeetsMinimumRequirement, true)
            .Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetBenchmarkSummaryAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(summaryDto);

        // Act
        var result = await _sut.GetSummary(athleteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BenchmarkSummaryResponse>().Subject;
        response.AthleteId.Should().Be(athleteId);
        response.TotalBenchmarks.Should().Be(5);
        response.MeetsMinimumRequirement.Should().BeTrue();
    }

    [Fact]
    public async Task GetSummary_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetSummary(athleteId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        await _benchmarkService.DidNotReceive().GetBenchmarkSummaryAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ValidIds_ReturnsOkWithBenchmark()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var benchmarkDto = _fixture.Build<AthleteBenchmarkDto>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarkByIdAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>())
            .Returns(benchmarkDto);

        // Act
        var result = await _sut.GetById(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Id.Should().Be(benchmarkId);
    }

    [Fact]
    public async Task GetById_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.GetById(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_InvalidBenchmarkId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.GetAthleteBenchmarkByIdAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>())
            .Returns((AthleteBenchmarkDto?)null);

        // Act
        var result = await _sut.GetById(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "RX"
        };

        var createdDto = _fixture.Build<AthleteBenchmarkDto>()
            .With(x => x.AthleteId, athleteId)
            .With(x => x.BenchmarkDefinitionId, request.BenchmarkDefinitionId)
            .With(x => x.Value, request.Value)
            .Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.RecordBenchmarkAsync(athleteId, Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((createdDto, false));

        // Act
        var result = await _sut.Create(athleteId, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(AthleteBenchmarksController.GetById));
        createdResult.RouteValues.Should().ContainKey("athleteId");
        createdResult.RouteValues.Should().ContainKey("benchmarkId");
        createdResult.RouteValues!["athleteId"].Should().Be(athleteId);
        createdResult.RouteValues!["benchmarkId"].Should().Be(createdDto.Id);
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Value.Should().Be(195.5m);
    }

    [Fact]
    public async Task Create_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m
        };

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Create(athleteId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        await _benchmarkService.DidNotReceive().RecordBenchmarkAsync(Arg.Any<int>(), Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_DuplicateBenchmark_ReturnsConflict()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m
        };

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.RecordBenchmarkAsync(athleteId, Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((null, true)); // IsDuplicate = true

        // Act
        var result = await _sut.Create(athleteId, request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_MapsRequestToDto()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new RecordBenchmarkRequest
        {
            BenchmarkDefinitionId = _fixture.Create<int>(),
            Value = 195.5m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            Notes = "  Test Notes  " // Should be trimmed
        };

        var createdDto = _fixture.Create<AthleteBenchmarkDto>();
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.RecordBenchmarkAsync(athleteId, Arg.Any<RecordBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((createdDto, false));

        // Act
        await _sut.Create(athleteId, request, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).RecordBenchmarkAsync(
            athleteId,
            Arg.Is<RecordBenchmarkDto>(dto =>
                dto.BenchmarkDefinitionId == request.BenchmarkDefinitionId &&
                dto.Value == 195.5m &&
                dto.RecordedAt == request.RecordedAt &&
                dto.Notes == "Test Notes"), // Trimmed
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidRequest_ReturnsOkWithUpdatedBenchmark()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new UpdateBenchmarkRequest
        {
            Value = 180m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "PR"
        };

        var updatedDto = _fixture.Build<AthleteBenchmarkDto>()
            .With(x => x.Id, benchmarkId)
            .With(x => x.AthleteId, athleteId)
            .With(x => x.Value, 180m)
            .With(x => x.Notes, "PR")
            .Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.UpdateBenchmarkAsync(athleteId, benchmarkId, Arg.Any<UpdateBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        var result = await _sut.Update(athleteId, benchmarkId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AthleteBenchmarkResponse>().Subject;
        response.Value.Should().Be(180m);
        response.Notes.Should().Be("PR");
    }

    [Fact]
    public async Task Update_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var request = new UpdateBenchmarkRequest { Value = 180m };

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Update(athleteId, benchmarkId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_InvalidBenchmarkId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new UpdateBenchmarkRequest { Value = 180m };

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.UpdateBenchmarkAsync(athleteId, benchmarkId, Arg.Any<UpdateBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns((AthleteBenchmarkDto?)null);

        // Act
        var result = await _sut.Update(athleteId, benchmarkId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_MapsRequestToDto()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();
        var request = new UpdateBenchmarkRequest
        {
            Value = 180m,
            RecordedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            Notes = "  Updated Notes  " // Should be trimmed
        };

        var updatedDto = _fixture.Create<AthleteBenchmarkDto>();
        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.UpdateBenchmarkAsync(athleteId, benchmarkId, Arg.Any<UpdateBenchmarkDto>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        // Act
        await _sut.Update(athleteId, benchmarkId, request, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).UpdateBenchmarkAsync(
            athleteId,
            benchmarkId,
            Arg.Is<UpdateBenchmarkDto>(dto =>
                dto.Value == 180m &&
                dto.RecordedAt == request.RecordedAt &&
                dto.Notes == "Updated Notes"), // Trimmed
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ValidIds_ReturnsNoContent()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.DeleteBenchmarkAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Delete(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_InvalidAthleteId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns((AthleteDto?)null);

        // Act
        var result = await _sut.Delete(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_InvalidBenchmarkId_ReturnsNotFound()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.DeleteBenchmarkAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Delete(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_CallsServiceWithCorrectIds()
    {
        // Arrange
        var athleteId = _fixture.Create<int>();
        var benchmarkId = _fixture.Create<int>();
        var athleteDto = _fixture.Build<AthleteDto>().With(x => x.Id, athleteId).Create();

        _athleteService.GetByIdAsync(athleteId, Arg.Any<CancellationToken>())
            .Returns(athleteDto);
        _benchmarkService.DeleteBenchmarkAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.Delete(athleteId, benchmarkId, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).DeleteBenchmarkAsync(athleteId, benchmarkId, Arg.Any<CancellationToken>());
    }

    #endregion
}
