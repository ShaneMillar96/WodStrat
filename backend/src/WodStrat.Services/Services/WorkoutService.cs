using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for workout CRUD operations.
/// </summary>
public class WorkoutService : IWorkoutService
{
    private readonly IWodStratDatabase _database;
    private readonly ICurrentUserService _currentUserService;

    public WorkoutService(
        IWodStratDatabase database,
        ICurrentUserService currentUserService)
    {
        _database = database;
        _currentUserService = currentUserService;
    }

    #region Workout CRUD Operations

    /// <inheritdoc />
    public async Task<WorkoutDto?> CreateWorkoutAsync(CreateWorkoutDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return null;
        }

        var userId = _currentUserService.GetRequiredUserId();
        var entity = dto.ToEntity(userId);

        _database.Add(entity);
        await _database.SaveChangesAsync(cancellationToken);

        // Load the created workout with movements
        var savedWorkout = await _database.Get<Workout>()
            .Include(w => w.Movements)
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return savedWorkout?.ToDto();
    }

    /// <inheritdoc />
    public async Task<WorkoutDto?> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _database.Get<Workout>()
            .Include(w => w.Movements.OrderBy(m => m.SequenceOrder))
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == id && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return workout?.ToDto();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutDto>> GetCurrentUserWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Array.Empty<WorkoutDto>();
        }

        var userId = _currentUserService.GetRequiredUserId();
        return await GetUserWorkoutsAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutDto>> GetUserWorkoutsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _database.Get<Workout>()
            .Include(w => w.Movements.OrderBy(m => m.SequenceOrder))
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.UserId == userId && !w.IsDeleted)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workouts.Select(w => w.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<WorkoutDto?> UpdateWorkoutAsync(int id, UpdateWorkoutDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<Workout>()
            .Include(w => w.Movements)
            .Where(w => w.Id == id && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        // Validate ownership
        if (!await ValidateOwnershipAsync(id, cancellationToken))
        {
            return null;
        }

        dto.ApplyTo(entity, _database);

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        // Reload with movements
        var updatedWorkout = await _database.Get<Workout>()
            .Include(w => w.Movements.OrderBy(m => m.SequenceOrder))
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return updatedWorkout?.ToDto();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteWorkoutAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<Workout>()
            .Where(w => w.Id == id && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return false;
        }

        // Validate ownership
        if (!await ValidateOwnershipAsync(id, cancellationToken))
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return true;
    }

    #endregion

    #region Ownership Validation

    /// <inheritdoc />
    public async Task<bool> ValidateOwnershipAsync(int workoutId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return false;
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        var workout = await _database.Get<Workout>()
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            return false;
        }

        return workout.UserId == userId.Value;
    }

    #endregion
}
