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
/// Unit tests for BenchmarkDefinitionsController.
/// </summary>
public class BenchmarkDefinitionsControllerTests
{
    private readonly IFixture _fixture;
    private readonly IBenchmarkService _benchmarkService;
    private readonly BenchmarkDefinitionsController _sut;

    public BenchmarkDefinitionsControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new BenchmarkDtoCustomization());

        _benchmarkService = Substitute.For<IBenchmarkService>();
        _sut = new BenchmarkDefinitionsController(_benchmarkService);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_NoCategory_ReturnsAllDefinitions()
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(3).ToList();
        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject;
        response.Should().HaveCount(3);

        await _benchmarkService.Received(1).GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
        await _benchmarkService.DidNotReceive().GetDefinitionsByCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_WithCategory_ReturnsFilteredDefinitions()
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(2).ToList();
        _benchmarkService.GetDefinitionsByCategoryAsync("Cardio", Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetAll("Cardio", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject;
        response.Should().HaveCount(2);

        await _benchmarkService.Received(1).GetDefinitionsByCategoryAsync("Cardio", Arg.Any<CancellationToken>());
        await _benchmarkService.DidNotReceive().GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAll_EmptyOrWhitespaceCategory_ReturnsAllDefinitions(string category)
    {
        // Arrange
        var definitions = _fixture.CreateMany<BenchmarkDefinitionDto>(3).ToList();
        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(definitions);

        // Act
        var result = await _sut.GetAll(category, CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).GetAllDefinitionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_NoDefinitions_ReturnsEmptyList()
    {
        // Arrange
        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BenchmarkDefinitionDto>());

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_MapsAllDtoPropertiesToResponse()
    {
        // Arrange
        var dto = _fixture.Build<BenchmarkDefinitionDto>()
            .With(x => x.Name, "Test Benchmark")
            .With(x => x.Slug, "test-benchmark")
            .With(x => x.Description, "Test description")
            .With(x => x.Category, "Strength")
            .With(x => x.MetricType, "Weight")
            .With(x => x.Unit, "kg")
            .With(x => x.DisplayOrder, 5)
            .Create();

        _benchmarkService.GetAllDefinitionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<BenchmarkDefinitionDto> { dto });

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<BenchmarkDefinitionResponse>>().Subject.First();

        response.Id.Should().Be(dto.Id);
        response.Name.Should().Be("Test Benchmark");
        response.Slug.Should().Be("test-benchmark");
        response.Description.Should().Be("Test description");
        response.Category.Should().Be("Strength");
        response.MetricType.Should().Be("Weight");
        response.Unit.Should().Be("kg");
        response.DisplayOrder.Should().Be(5);
    }

    #endregion

    #region GetBySlug Tests

    [Fact]
    public async Task GetBySlug_ValidSlug_ReturnsOkWithDefinition()
    {
        // Arrange
        var dto = _fixture.Create<BenchmarkDefinitionDto>();
        _benchmarkService.GetDefinitionBySlugAsync("fran", Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        var result = await _sut.GetBySlug("fran", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BenchmarkDefinitionResponse>().Subject;
        response.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GetBySlug_InvalidSlug_ReturnsNotFound()
    {
        // Arrange
        _benchmarkService.GetDefinitionBySlugAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((BenchmarkDefinitionDto?)null);

        // Act
        var result = await _sut.GetBySlug("nonexistent", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBySlug_CallsServiceWithCorrectSlug()
    {
        // Arrange
        var dto = _fixture.Create<BenchmarkDefinitionDto>();
        _benchmarkService.GetDefinitionBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        await _sut.GetBySlug("test-slug", CancellationToken.None);

        // Assert
        await _benchmarkService.Received(1).GetDefinitionBySlugAsync("test-slug", Arg.Any<CancellationToken>());
    }

    #endregion
}
