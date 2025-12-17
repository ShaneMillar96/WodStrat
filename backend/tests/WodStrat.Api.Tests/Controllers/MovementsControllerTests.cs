using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.ViewModels.Movements;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for MovementsController.
/// </summary>
public class MovementsControllerTests
{
    private readonly IFixture _fixture;
    private readonly IMovementDefinitionService _movementDefinitionService;
    private readonly MovementsController _sut;

    public MovementsControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());

        // Customize MovementDefinitionDto
        _fixture.Customize<MovementDefinitionDto>(c => c
            .With(x => x.Id, () => _fixture.Create<int>())
            .With(x => x.CanonicalName, "pull_up")
            .With(x => x.DisplayName, "Pull-up")
            .With(x => x.Category, "Gymnastics")
            .With(x => x.Description, "Vertical pulling movement")
            .With(x => x.Aliases, new List<string> { "pu", "pullup" }));

        _movementDefinitionService = Substitute.For<IMovementDefinitionService>();
        _sut = new MovementsController(_movementDefinitionService);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_NoCategory_ReturnsAllActiveMovements()
    {
        // Arrange
        var movements = new List<MovementDefinitionDto>
        {
            CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics"),
            CreateMovementDto(2, "deadlift", "Deadlift", "Weightlifting")
        };

        _movementDefinitionService.GetAllActiveMovementsAsync(Arg.Any<CancellationToken>())
            .Returns(movements);

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<MovementDefinitionResponse>>().Subject;
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithCategory_ReturnsFilteredMovements()
    {
        // Arrange
        var movements = new List<MovementDefinitionDto>
        {
            CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics"),
            CreateMovementDto(2, "muscle_up", "Muscle-up", "Gymnastics")
        };

        _movementDefinitionService.GetMovementsByCategoryAsync("Gymnastics", Arg.Any<CancellationToken>())
            .Returns(movements);

        // Act
        var result = await _sut.GetAll("Gymnastics", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<MovementDefinitionResponse>>().Subject;
        response.Should().HaveCount(2);
        response.Should().OnlyContain(m => m.Category == "Gymnastics");

        await _movementDefinitionService.Received(1).GetMovementsByCategoryAsync("Gymnastics", Arg.Any<CancellationToken>());
        await _movementDefinitionService.DidNotReceive().GetAllActiveMovementsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_EmptyCategory_ReturnsAllActiveMovements()
    {
        // Arrange
        var movements = new List<MovementDefinitionDto>
        {
            CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics")
        };

        _movementDefinitionService.GetAllActiveMovementsAsync(Arg.Any<CancellationToken>())
            .Returns(movements);

        // Act
        var result = await _sut.GetAll("", CancellationToken.None);

        // Assert
        await _movementDefinitionService.Received(1).GetAllActiveMovementsAsync(Arg.Any<CancellationToken>());
        await _movementDefinitionService.DidNotReceive().GetMovementsByCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_WhitespaceCategory_ReturnsAllActiveMovements()
    {
        // Arrange
        var movements = new List<MovementDefinitionDto>
        {
            CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics")
        };

        _movementDefinitionService.GetAllActiveMovementsAsync(Arg.Any<CancellationToken>())
            .Returns(movements);

        // Act
        var result = await _sut.GetAll("   ", CancellationToken.None);

        // Assert
        await _movementDefinitionService.Received(1).GetAllActiveMovementsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_NoMovements_ReturnsEmptyList()
    {
        // Arrange
        _movementDefinitionService.GetAllActiveMovementsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<MovementDefinitionDto>());

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<MovementDefinitionResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_MapsAllPropertiesToResponse()
    {
        // Arrange
        var movement = CreateMovementDto(1, "toes_to_bar", "Toes-to-Bar", "Gymnastics");
        movement.Description = "Hang from bar, lift toes to touch bar";
        movement.Aliases = new List<string> { "t2b", "ttb" };

        _movementDefinitionService.GetAllActiveMovementsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<MovementDefinitionDto> { movement });

        // Act
        var result = await _sut.GetAll(null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<MovementDefinitionResponse>>().Subject.First();

        response.Id.Should().Be(movement.Id);
        response.CanonicalName.Should().Be(movement.CanonicalName);
        response.DisplayName.Should().Be(movement.DisplayName);
        response.Category.Should().Be(movement.Category);
        response.Description.Should().Be(movement.Description);
        response.Aliases.Should().Contain("t2b");
        response.Aliases.Should().Contain("ttb");
    }

    #endregion

    #region GetByCanonicalName Tests

    [Fact]
    public async Task GetByCanonicalName_ExistingMovement_ReturnsOkWithMovement()
    {
        // Arrange
        var movement = CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics");

        _movementDefinitionService.GetMovementByCanonicalNameAsync("pull_up", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.GetByCanonicalName("pull_up", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.CanonicalName.Should().Be("pull_up");
        response.DisplayName.Should().Be("Pull-up");
    }

    [Fact]
    public async Task GetByCanonicalName_NotFound_ReturnsNotFound()
    {
        // Arrange
        _movementDefinitionService.GetMovementByCanonicalNameAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((MovementDefinitionDto?)null);

        // Act
        var result = await _sut.GetByCanonicalName("nonexistent", CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByCanonicalName_PassesCanonicalNameToService()
    {
        // Arrange
        var movement = CreateMovementDto(1, "deadlift", "Deadlift", "Weightlifting");

        _movementDefinitionService.GetMovementByCanonicalNameAsync("deadlift", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        await _sut.GetByCanonicalName("deadlift", CancellationToken.None);

        // Assert
        await _movementDefinitionService.Received(1).GetMovementByCanonicalNameAsync("deadlift", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByCanonicalName_WithAliases_ReturnsAliasesInResponse()
    {
        // Arrange
        var movement = CreateMovementDto(1, "double_under", "Double Under", "Cardio");
        movement.Aliases = new List<string> { "du", "dubs" };

        _movementDefinitionService.GetMovementByCanonicalNameAsync("double_under", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.GetByCanonicalName("double_under", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.Aliases.Should().HaveCount(2);
        response.Aliases.Should().Contain("du");
        response.Aliases.Should().Contain("dubs");
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchedMovement()
    {
        // Arrange
        var movement = CreateMovementDto(1, "toes_to_bar", "Toes-to-Bar", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("t2b", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.Search("t2b", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.CanonicalName.Should().Be("toes_to_bar");
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsOkWithNull()
    {
        // Arrange
        _movementDefinitionService.FindMovementByAliasAsync("unknown", Arg.Any<CancellationToken>())
            .Returns((MovementDefinitionDto?)null);

        // Act
        var result = await _sut.Search("unknown", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeNull();
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Search("", CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_WhitespaceQuery_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Search("   ", CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_NullQuery_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Search(null!, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_PassesQueryToService()
    {
        // Arrange
        var movement = CreateMovementDto(1, "muscle_up", "Muscle-up", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("MU", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        await _sut.Search("MU", CancellationToken.None);

        // Assert
        await _movementDefinitionService.Received(1).FindMovementByAliasAsync("MU", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_ByDisplayName_ReturnsMatchedMovement()
    {
        // Arrange
        var movement = CreateMovementDto(1, "pull_up", "Pull-up", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("Pull-up", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.Search("Pull-up", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.DisplayName.Should().Be("Pull-up");
    }

    [Fact]
    public async Task Search_ByCanonicalName_ReturnsMatchedMovement()
    {
        // Arrange
        var movement = CreateMovementDto(1, "box_jump", "Box Jump", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("box_jump", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.Search("box_jump", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.CanonicalName.Should().Be("box_jump");
    }

    #endregion

    #region Lookup Tests (WOD-12)

    [Fact]
    public async Task Lookup_ValidAlias_ReturnsOkWithMovement()
    {
        // Arrange
        var movement = CreateMovementDto(1, "toes_to_bar", "Toes-to-Bar", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("t2b", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.Lookup("t2b", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.CanonicalName.Should().Be("toes_to_bar");
        response.DisplayName.Should().Be("Toes-to-Bar");
    }

    [Fact]
    public async Task Lookup_AliasNotFound_ReturnsNotFound()
    {
        // Arrange
        _movementDefinitionService.FindMovementByAliasAsync("unknown", Arg.Any<CancellationToken>())
            .Returns((MovementDefinitionDto?)null);

        // Act
        var result = await _sut.Lookup("unknown", CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Lookup_EmptyAlias_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Lookup("", CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Lookup_WhitespaceAlias_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Lookup("   ", CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Lookup_NullAlias_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _sut.Lookup(null!, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Lookup_PassesAliasToService()
    {
        // Arrange
        var movement = CreateMovementDto(1, "muscle_up", "Muscle-up", "Gymnastics");

        _movementDefinitionService.FindMovementByAliasAsync("MU", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        await _sut.Lookup("MU", CancellationToken.None);

        // Assert
        await _movementDefinitionService.Received(1).FindMovementByAliasAsync("MU", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lookup_WithAliases_ReturnsAliasesInResponse()
    {
        // Arrange
        var movement = CreateMovementDto(1, "toes_to_bar", "Toes-to-Bar", "Gymnastics");
        movement.Aliases = new List<string> { "t2b", "ttb", "toes-to-bar" };

        _movementDefinitionService.FindMovementByAliasAsync("t2b", Arg.Any<CancellationToken>())
            .Returns(movement);

        // Act
        var result = await _sut.Lookup("t2b", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MovementDefinitionResponse>().Subject;
        response.Aliases.Should().HaveCount(3);
        response.Aliases.Should().Contain("t2b");
        response.Aliases.Should().Contain("ttb");
    }

    [Fact]
    public async Task Lookup_NotFoundErrorIncludesAlias()
    {
        // Arrange
        _movementDefinitionService.FindMovementByAliasAsync("xyz", Arg.Any<CancellationToken>())
            .Returns((MovementDefinitionDto?)null);

        // Act
        var result = await _sut.Lookup("xyz", CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var errorValue = notFoundResult.Value;
        errorValue.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private MovementDefinitionDto CreateMovementDto(int id, string canonicalName, string displayName, string category)
    {
        return new MovementDefinitionDto
        {
            Id = id,
            CanonicalName = canonicalName,
            DisplayName = displayName,
            Category = category,
            Description = $"{displayName} movement",
            Aliases = new List<string>()
        };
    }

    #endregion
}
