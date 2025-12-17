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
/// Unit tests for WorkoutService.
/// </summary>
public class WorkoutServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly ICurrentUserService _currentUserService;
    private readonly WorkoutService _sut;

    public WorkoutServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new WorkoutCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new WorkoutService(_database, _currentUserService);
    }

    #region CreateWorkoutAsync Tests

    [Fact]
    public async Task CreateWorkoutAsync_WhenNotAuthenticated_ReturnsNull()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);
        var dto = CreateValidCreateWorkoutDto();

        // Act
        var result = await _sut.CreateWorkoutAsync(dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Add(Arg.Any<Workout>());
    }

    [Fact]
    public async Task CreateWorkoutAsync_WhenAuthenticated_CreatesWorkoutAndReturnsDto()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var dto = CreateValidCreateWorkoutDto();
        Workout? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Workout>()))
            .Do(x => savedEntity = x.Arg<Workout>());

        // Setup query for reload after save
        _database.Get<Workout>().Returns(x =>
        {
            if (savedEntity != null)
            {
                return new[] { savedEntity }.AsQueryable().BuildMock();
            }
            return Array.Empty<Workout>().AsQueryable().BuildMock();
        });

        // Act
        var result = await _sut.CreateWorkoutAsync(dto);

        // Assert
        _database.Received(1).Add(Arg.Any<Workout>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        savedEntity.Should().NotBeNull();
        savedEntity!.UserId.Should().Be(userId);
        savedEntity.WorkoutType.Should().Be(dto.WorkoutType);
        savedEntity.OriginalText.Should().Be(dto.OriginalText);
        savedEntity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateWorkoutAsync_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var dto = CreateValidCreateWorkoutDto();
        Workout? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Workout>()))
            .Do(x => savedEntity = x.Arg<Workout>());

        _database.Get<Workout>().Returns(x =>
        {
            if (savedEntity != null)
            {
                return new[] { savedEntity }.AsQueryable().BuildMock();
            }
            return Array.Empty<Workout>().AsQueryable().BuildMock();
        });

        var beforeCreate = DateTime.UtcNow;

        // Act
        await _sut.CreateWorkoutAsync(dto);

        var afterCreate = DateTime.UtcNow;

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        savedEntity.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task CreateWorkoutAsync_WithMovements_CreatesMovementsCorrectly()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var dto = new CreateWorkoutDto
        {
            Name = "Test WOD",
            WorkoutType = WorkoutType.ForTime,
            OriginalText = "21-15-9\nThrusters\nPull-ups",
            Movements = new List<CreateWorkoutMovementDto>
            {
                new() { MovementDefinitionId = 1, SequenceOrder = 1, RepCount = 21, LoadValue = 95, LoadUnit = LoadUnit.Lb },
                new() { MovementDefinitionId = 2, SequenceOrder = 2, RepCount = 21 }
            }
        };

        Workout? savedEntity = null;
        _database.When(x => x.Add(Arg.Any<Workout>()))
            .Do(x => savedEntity = x.Arg<Workout>());

        _database.Get<Workout>().Returns(x =>
        {
            if (savedEntity != null)
            {
                return new[] { savedEntity }.AsQueryable().BuildMock();
            }
            return Array.Empty<Workout>().AsQueryable().BuildMock();
        });

        // Act
        await _sut.CreateWorkoutAsync(dto);

        // Assert
        savedEntity.Should().NotBeNull();
        savedEntity!.Movements.Should().HaveCount(2);
        savedEntity.Movements.First().MovementDefinitionId.Should().Be(1);
        savedEntity.Movements.First().RepCount.Should().Be(21);
        savedEntity.Movements.First().LoadValue.Should().Be(95);
        savedEntity.Movements.First().LoadUnit.Should().Be(LoadUnit.Lb);
    }

    #endregion

    #region GetWorkoutByIdAsync Tests

    [Fact]
    public async Task GetWorkoutByIdAsync_ValidId_ReturnsWorkoutDto()
    {
        // Arrange
        var workout = _fixture.Create<Workout>();
        workout.IsDeleted = false;
        workout.Movements = new List<WorkoutMovement>();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetWorkoutByIdAsync(workout.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workout.Id);
        result.Name.Should().Be(workout.Name);
        result.WorkoutType.Should().Be(workout.WorkoutType.ToString());
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetWorkoutByIdAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_DeletedWorkout_ReturnsNull()
    {
        // Arrange
        var workout = _fixture.Build<Workout>()
            .With(w => w.IsDeleted, true)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetWorkoutByIdAsync(workout.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_WithMovements_ReturnsOrderedMovements()
    {
        // Arrange
        var movement1 = _fixture.Build<WorkoutMovement>()
            .With(m => m.SequenceOrder, 2)
            .Without(m => m.Workout)
            .Without(m => m.MovementDefinition)
            .Create();
        var movement2 = _fixture.Build<WorkoutMovement>()
            .With(m => m.SequenceOrder, 1)
            .Without(m => m.Workout)
            .Without(m => m.MovementDefinition)
            .Create();

        var workout = _fixture.Build<Workout>()
            .With(w => w.IsDeleted, false)
            .With(w => w.Movements, new List<WorkoutMovement> { movement1, movement2 })
            .Without(w => w.User)
            .Create();

        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetWorkoutByIdAsync(workout.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Movements.Should().HaveCount(2);
        result.Movements[0].SequenceOrder.Should().Be(1);
        result.Movements[1].SequenceOrder.Should().Be(2);
    }

    #endregion

    #region GetCurrentUserWorkoutsAsync Tests

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_WhenNotAuthenticated_ReturnsEmptyList()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);

        // Act
        var result = await _sut.GetCurrentUserWorkoutsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_WhenAuthenticated_ReturnsUserWorkouts()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var workout1 = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.CreatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var workout2 = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.CreatedAt, DateTime.UtcNow)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();

        var queryable = new[] { workout1, workout2 }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetCurrentUserWorkoutsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ExcludesDeletedWorkouts()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var activeWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var deletedWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, true)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();

        var queryable = new[] { activeWorkout, deletedWorkout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetCurrentUserWorkoutsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeWorkout.Id);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ExcludesOtherUsersWorkouts()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var otherUserId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.GetRequiredUserId().Returns(userId);

        var userWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var otherUserWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, otherUserId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();

        var queryable = new[] { userWorkout, otherUserWorkout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetCurrentUserWorkoutsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(userWorkout.Id);
    }

    #endregion

    #region GetUserWorkoutsAsync Tests

    [Fact]
    public async Task GetUserWorkoutsAsync_ValidUserId_ReturnsWorkouts()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();

        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetUserWorkoutsAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(workout.Id);
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_NoWorkouts_ReturnsEmptyList()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetUserWorkoutsAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var oldWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var newWorkout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.CreatedAt, DateTime.UtcNow)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();

        var queryable = new[] { oldWorkout, newWorkout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.GetUserWorkoutsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newWorkout.Id);
        result[1].Id.Should().Be(oldWorkout.Id);
    }

    #endregion

    #region UpdateWorkoutAsync Tests

    [Fact]
    public async Task UpdateWorkoutAsync_WorkoutNotFound_ReturnsNull()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        var dto = new UpdateWorkoutDto { Name = "Updated" };

        // Act
        var result = await _sut.UpdateWorkoutAsync(_fixture.Create<int>(), dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task UpdateWorkoutAsync_DeletedWorkout_ReturnsNull()
    {
        // Arrange
        var workout = _fixture.Build<Workout>()
            .With(w => w.IsDeleted, true)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        var dto = new UpdateWorkoutDto { Name = "Updated" };

        // Act
        var result = await _sut.UpdateWorkoutAsync(workout.Id, dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task UpdateWorkoutAsync_NotOwner_ReturnsNull()
    {
        // Arrange
        var workoutUserId = _fixture.Create<int>();
        var currentUserId = _fixture.Create<int>();

        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, workoutUserId)
            .With(w => w.IsDeleted, false)
            .With(w => w.Movements, new List<WorkoutMovement>())
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId);

        var dto = new UpdateWorkoutDto { Name = "Updated" };

        // Act
        var result = await _sut.UpdateWorkoutAsync(workout.Id, dto);

        // Assert
        result.Should().BeNull();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task UpdateWorkoutAsync_ValidOwner_UpdatesAndReturnsDto()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.Movements, new List<WorkoutMovement>())
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var dto = new UpdateWorkoutDto
        {
            Name = "Updated Name",
            WorkoutType = WorkoutType.Emom,
            TimeCapSeconds = 1200,
            ParsedDescription = "Updated description"
        };

        // Act
        var result = await _sut.UpdateWorkoutAsync(workout.Id, dto);

        // Assert
        _database.Received(1).Update(Arg.Any<Workout>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        workout.Name.Should().Be(dto.Name);
        workout.WorkoutType.Should().Be(dto.WorkoutType);
        workout.TimeCapSeconds.Should().Be(dto.TimeCapSeconds);
        workout.ParsedDescription.Should().Be(dto.ParsedDescription);
    }

    [Fact]
    public async Task UpdateWorkoutAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.UpdatedAt, originalUpdatedAt)
            .With(w => w.Movements, new List<WorkoutMovement>())
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var dto = new UpdateWorkoutDto { Name = "Updated" };

        // Act
        await _sut.UpdateWorkoutAsync(workout.Id, dto);

        // Assert
        workout.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateWorkoutAsync_WithNewMovements_ReplacesExistingMovements()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var existingMovement = _fixture.Build<WorkoutMovement>()
            .Without(m => m.Workout)
            .Without(m => m.MovementDefinition)
            .Create();
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.Movements, new List<WorkoutMovement> { existingMovement })
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var dto = new UpdateWorkoutDto
        {
            Name = "Updated",
            WorkoutType = WorkoutType.ForTime,
            Movements = new List<CreateWorkoutMovementDto>
            {
                new() { MovementDefinitionId = 10, SequenceOrder = 1, RepCount = 15 }
            }
        };

        // Act
        await _sut.UpdateWorkoutAsync(workout.Id, dto);

        // Assert
        _database.Received(1).Remove(existingMovement);
        workout.Movements.Should().HaveCount(1);
        workout.Movements.First().MovementDefinitionId.Should().Be(10);
    }

    #endregion

    #region DeleteWorkoutAsync Tests

    [Fact]
    public async Task DeleteWorkoutAsync_WorkoutNotFound_ReturnsFalse()
    {
        // Arrange
        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.DeleteWorkoutAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task DeleteWorkoutAsync_AlreadyDeleted_ReturnsFalse()
    {
        // Arrange
        var workout = _fixture.Build<Workout>()
            .With(w => w.IsDeleted, true)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.DeleteWorkoutAsync(workout.Id);

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task DeleteWorkoutAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var workoutUserId = _fixture.Create<int>();
        var currentUserId = _fixture.Create<int>();

        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, workoutUserId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId);

        // Act
        var result = await _sut.DeleteWorkoutAsync(workout.Id);

        // Assert
        result.Should().BeFalse();
        _database.DidNotReceive().Update(Arg.Any<Workout>());
    }

    [Fact]
    public async Task DeleteWorkoutAsync_ValidOwner_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        // Act
        var result = await _sut.DeleteWorkoutAsync(workout.Id);

        // Assert
        result.Should().BeTrue();
        workout.IsDeleted.Should().BeTrue();
        _database.Received(1).Update(Arg.Any<Workout>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteWorkoutAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-7);
        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .With(w => w.UpdatedAt, originalUpdatedAt)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        // Act
        await _sut.DeleteWorkoutAsync(workout.Id);

        // Assert
        workout.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region ValidateOwnershipAsync Tests

    [Fact]
    public async Task ValidateOwnershipAsync_WhenNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);

        // Act
        var result = await _sut.ValidateOwnershipAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOwnershipAsync_WhenUserIdNull_ReturnsFalse()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns((int?)null);

        // Act
        var result = await _sut.ValidateOwnershipAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOwnershipAsync_WorkoutNotFound_ReturnsFalse()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(_fixture.Create<int>());

        var queryable = Array.Empty<Workout>().AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.ValidateOwnershipAsync(_fixture.Create<int>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOwnershipAsync_DeletedWorkout_ReturnsFalse()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, true)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.ValidateOwnershipAsync(workout.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOwnershipAsync_DifferentUser_ReturnsFalse()
    {
        // Arrange
        var workoutUserId = _fixture.Create<int>();
        var currentUserId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId);

        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, workoutUserId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.ValidateOwnershipAsync(workout.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateOwnershipAsync_OwnerUser_ReturnsTrue()
    {
        // Arrange
        var userId = _fixture.Create<int>();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var workout = _fixture.Build<Workout>()
            .With(w => w.UserId, userId)
            .With(w => w.IsDeleted, false)
            .Without(w => w.Movements)
            .Without(w => w.User)
            .Create();
        var queryable = new[] { workout }.AsQueryable().BuildMock();
        _database.Get<Workout>().Returns(queryable);

        // Act
        var result = await _sut.ValidateOwnershipAsync(workout.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private CreateWorkoutDto CreateValidCreateWorkoutDto()
    {
        return new CreateWorkoutDto
        {
            Name = "Test Workout",
            WorkoutType = WorkoutType.ForTime,
            OriginalText = "21-15-9\nThrusters\nPull-ups",
            ParsedDescription = "FOR TIME - 2 movement(s)",
            TimeCapSeconds = 1200,
            Movements = new List<CreateWorkoutMovementDto>()
        };
    }

    #endregion
}
