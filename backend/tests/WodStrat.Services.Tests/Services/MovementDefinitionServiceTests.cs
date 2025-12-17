using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Services;
using WodStrat.Services.Tests.Customizations;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for MovementDefinitionService.
/// </summary>
public class MovementDefinitionServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly MovementDefinitionService _sut;

    public MovementDefinitionServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new WorkoutCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _sut = new MovementDefinitionService(_database);
    }

    #region GetAllActiveMovementsAsync Tests

    [Fact]
    public async Task GetAllActiveMovementsAsync_ReturnsActiveMovements()
    {
        // Arrange
        var movement1 = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var movement2 = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, true);
        var inactiveMovement = CreateMovementDefinition("inactive", "Inactive Move", MovementCategory.Cardio, false);

        var queryable = new[] { movement1, movement2, inactiveMovement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllActiveMovementsAsync();

        // Assert
        result.Should().HaveCount(2);
        // Only active movements should be returned (inactive was filtered out)
    }

    [Fact]
    public async Task GetAllActiveMovementsAsync_NoMovements_ReturnsEmptyList()
    {
        // Arrange
        var queryable = Array.Empty<MovementDefinition>().AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllActiveMovementsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActiveMovementsAsync_OrdersByCategoryThenDisplayName()
    {
        // Arrange
        var cardio = CreateMovementDefinition("row", "Row", MovementCategory.Cardio, true);
        var gymnastics1 = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var gymnastics2 = CreateMovementDefinition("air_squat", "Air Squat", MovementCategory.Gymnastics, true);
        var weightlifting = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, true);

        var queryable = new[] { cardio, gymnastics1, gymnastics2, weightlifting }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllActiveMovementsAsync();

        // Assert
        result.Should().HaveCount(4);
        // Ordered by category then display name
        result[0].Category.Should().Be(MovementCategory.Weightlifting.ToString());
        result[1].Category.Should().Be(MovementCategory.Gymnastics.ToString());
        result[2].Category.Should().Be(MovementCategory.Gymnastics.ToString());
        result[3].Category.Should().Be(MovementCategory.Cardio.ToString());
    }

    [Fact]
    public async Task GetAllActiveMovementsAsync_IncludesAliases()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id },
            new() { Id = 2, Alias = "ttb", MovementDefinitionId = movement.Id }
        };

        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetAllActiveMovementsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Aliases.Should().HaveCount(2);
        result[0].Aliases.Should().Contain("t2b");
        result[0].Aliases.Should().Contain("ttb");
    }

    #endregion

    #region GetMovementsByCategoryAsync Tests

    [Fact]
    public async Task GetMovementsByCategoryAsync_ValidCategory_ReturnsFilteredMovements()
    {
        // Arrange
        var gymnastics = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var weightlifting = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, true);

        var queryable = new[] { gymnastics, weightlifting }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementsByCategoryAsync("Gymnastics");

        // Assert
        result.Should().HaveCount(1);
        result.First().CanonicalName.Should().Be("pull_up");
    }

    [Fact]
    public async Task GetMovementsByCategoryAsync_CaseInsensitive()
    {
        // Arrange
        var movement = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, true);
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementsByCategoryAsync("WEIGHTLIFTING");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMovementsByCategoryAsync_InvalidCategory_ReturnsEmptyList()
    {
        // Arrange
        var movement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementsByCategoryAsync("InvalidCategory");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMovementsByCategoryAsync_ExcludesInactiveMovements()
    {
        // Arrange
        var active = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var inactive = CreateMovementDefinition("burpee", "Burpee", MovementCategory.Gymnastics, false);

        var queryable = new[] { active, inactive }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementsByCategoryAsync("Gymnastics");

        // Assert
        result.Should().HaveCount(1);
        result.First().CanonicalName.Should().Be("pull_up");
    }

    [Fact]
    public async Task GetMovementsByCategoryAsync_OrdersByDisplayName()
    {
        // Arrange
        var zMove = CreateMovementDefinition("z_press", "Z-Press", MovementCategory.Weightlifting, true);
        var aMove = CreateMovementDefinition("atlas_stone", "Atlas Stone", MovementCategory.Weightlifting, true);

        var queryable = new[] { zMove, aMove }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementsByCategoryAsync("Weightlifting");

        // Assert
        result.Should().HaveCount(2);
        result[0].DisplayName.Should().Be("Atlas Stone");
        result[1].DisplayName.Should().Be("Z-Press");
    }

    #endregion

    #region GetMovementByCanonicalNameAsync Tests

    [Fact]
    public async Task GetMovementByCanonicalNameAsync_ExactMatch_ReturnsMovement()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementByCanonicalNameAsync("toes_to_bar");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("toes_to_bar");
        result.DisplayName.Should().Be("Toes-to-Bar");
    }

    [Fact]
    public async Task GetMovementByCanonicalNameAsync_CaseInsensitive()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementByCanonicalNameAsync("TOES_TO_BAR");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("toes_to_bar");
    }

    [Fact]
    public async Task GetMovementByCanonicalNameAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<MovementDefinition>().AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementByCanonicalNameAsync("nonexistent_movement");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMovementByCanonicalNameAsync_InactiveMovement_ReturnsNull()
    {
        // Arrange
        var movement = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, false);
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementByCanonicalNameAsync("deadlift");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMovementByCanonicalNameAsync_IncludesAliases()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id }
        };
        var queryable = new[] { movement }.AsQueryable().BuildMock();
        _database.Get<MovementDefinition>().Returns(queryable);

        // Act
        var result = await _sut.GetMovementByCanonicalNameAsync("toes_to_bar");

        // Assert
        result.Should().NotBeNull();
        result!.Aliases.Should().Contain("t2b");
    }

    #endregion

    #region FindMovementByAliasAsync Tests

    [Fact]
    public async Task FindMovementByAliasAsync_FindsByAlias_ReturnsMovement()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("t2b");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("toes_to_bar");
    }

    [Fact]
    public async Task FindMovementByAliasAsync_CaseInsensitive()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("T2B");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("toes_to_bar");
    }

    [Fact]
    public async Task FindMovementByAliasAsync_TrimsWhitespace()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("  t2b  ");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("toes_to_bar");
    }

    [Fact]
    public async Task FindMovementByAliasAsync_FallsBackToCanonicalName()
    {
        // Arrange
        var movement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>();

        var aliasQueryable = Array.Empty<MovementAlias>().AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("pull_up");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("pull_up");
    }

    [Fact]
    public async Task FindMovementByAliasAsync_FallsBackToDisplayName()
    {
        // Arrange
        var movement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>();

        var aliasQueryable = Array.Empty<MovementAlias>().AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("pull-up");

        // Assert
        result.Should().NotBeNull();
        result!.CanonicalName.Should().Be("pull_up");
    }

    [Fact]
    public async Task FindMovementByAliasAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var aliasQueryable = Array.Empty<MovementAlias>().AsQueryable().BuildMock();
        var movementQueryable = Array.Empty<MovementDefinition>().AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindMovementByAliasAsync_InactiveMovementAlias_ReturnsNull()
    {
        // Arrange
        var movement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, false);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "pu", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.FindMovementByAliasAsync("pu");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAliasLookupAsync Tests

    [Fact]
    public async Task GetAliasLookupAsync_ReturnsAliasesAndCanonicalNames()
    {
        // Arrange
        var movement = CreateMovementDefinition("toes_to_bar", "Toes-to-Bar", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "t2b", MovementDefinitionId = movement.Id, MovementDefinition = movement },
            new() { Id = 2, Alias = "ttb", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.GetAliasLookupAsync();

        // Assert
        result.Should().ContainKey("t2b");
        result.Should().ContainKey("ttb");
        result.Should().ContainKey("toes_to_bar");
        result.Should().ContainKey("toes-to-bar");
        result["t2b"].Should().Be(movement.Id);
        result["ttb"].Should().Be(movement.Id);
    }

    [Fact]
    public async Task GetAliasLookupAsync_CaseInsensitiveLookup()
    {
        // Arrange
        var movement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        movement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "PU", MovementDefinitionId = movement.Id, MovementDefinition = movement }
        };

        var aliasQueryable = movement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.GetAliasLookupAsync();

        // Assert
        result.Should().ContainKey("pu");
        result.Should().ContainKey("PU");
        result.Should().ContainKey("Pu");
    }

    [Fact]
    public async Task GetAliasLookupAsync_ExcludesInactiveMovements()
    {
        // Arrange
        var activeMovement = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var inactiveMovement = CreateMovementDefinition("burpee", "Burpee", MovementCategory.Gymnastics, false);

        activeMovement.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "pu", MovementDefinitionId = activeMovement.Id, MovementDefinition = activeMovement }
        };
        inactiveMovement.Aliases = new List<MovementAlias>
        {
            new() { Id = 2, Alias = "bp", MovementDefinitionId = inactiveMovement.Id, MovementDefinition = inactiveMovement }
        };

        var aliasQueryable = activeMovement.Aliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { activeMovement }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.GetAliasLookupAsync();

        // Assert
        result.Should().ContainKey("pu");
        result.Should().NotContainKey("bp");
        result.Should().NotContainKey("burpee");
    }

    [Fact]
    public async Task GetAliasLookupAsync_NoMovements_ReturnsEmptyDictionary()
    {
        // Arrange
        var aliasQueryable = Array.Empty<MovementAlias>().AsQueryable().BuildMock();
        var movementQueryable = Array.Empty<MovementDefinition>().AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.GetAliasLookupAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAliasLookupAsync_MultipleMovements_ReturnsAllMappings()
    {
        // Arrange
        var movement1 = CreateMovementDefinition("pull_up", "Pull-up", MovementCategory.Gymnastics, true);
        var movement2 = CreateMovementDefinition("deadlift", "Deadlift", MovementCategory.Weightlifting, true);

        movement1.Aliases = new List<MovementAlias>
        {
            new() { Id = 1, Alias = "pu", MovementDefinitionId = movement1.Id, MovementDefinition = movement1 }
        };
        movement2.Aliases = new List<MovementAlias>
        {
            new() { Id = 2, Alias = "dl", MovementDefinitionId = movement2.Id, MovementDefinition = movement2 }
        };

        var allAliases = movement1.Aliases.Concat(movement2.Aliases).ToList();
        var aliasQueryable = allAliases.AsQueryable().BuildMock();
        var movementQueryable = new[] { movement1, movement2 }.AsQueryable().BuildMock();

        _database.Get<MovementAlias>().Returns(aliasQueryable);
        _database.Get<MovementDefinition>().Returns(movementQueryable);

        // Act
        var result = await _sut.GetAliasLookupAsync();

        // Assert
        result.Should().ContainKey("pu");
        result.Should().ContainKey("dl");
        result.Should().ContainKey("pull_up");
        result.Should().ContainKey("deadlift");
        result["pu"].Should().Be(movement1.Id);
        result["dl"].Should().Be(movement2.Id);
    }

    #endregion

    #region Helper Methods

    private MovementDefinition CreateMovementDefinition(string canonicalName, string displayName, MovementCategory category, bool isActive)
    {
        return new MovementDefinition
        {
            Id = _fixture.Create<int>(),
            CanonicalName = canonicalName,
            DisplayName = displayName,
            Category = category,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Aliases = new List<MovementAlias>()
        };
    }

    #endregion
}
