using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping between Workout entities and DTOs.
/// </summary>
public static class WorkoutMappingExtensions
{
    /// <summary>
    /// Maps a Workout entity to a WorkoutDto.
    /// </summary>
    /// <param name="entity">The workout entity (must include Movements navigation).</param>
    /// <returns>The workout DTO.</returns>
    public static WorkoutDto ToDto(this Workout entity)
    {
        return new WorkoutDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            WorkoutType = entity.WorkoutType.ToString(),
            OriginalText = entity.OriginalText,
            ParsedDescription = entity.ParsedDescription,
            TimeCapSeconds = entity.TimeCapSeconds,
            TimeCapFormatted = FormatSeconds(entity.TimeCapSeconds),
            RoundCount = entity.RoundCount,
            IntervalDurationSeconds = entity.IntervalDurationSeconds,
            IntervalDurationFormatted = FormatSeconds(entity.IntervalDurationSeconds),
            Movements = entity.Movements?.OrderBy(m => m.SequenceOrder).Select(m => m.ToDto()).ToList()
                        ?? new List<WorkoutMovementDto>(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a WorkoutMovement entity to a WorkoutMovementDto.
    /// </summary>
    /// <param name="entity">The workout movement entity (must include MovementDefinition navigation).</param>
    /// <returns>The workout movement DTO.</returns>
    public static WorkoutMovementDto ToDto(this WorkoutMovement entity)
    {
        var definition = entity.MovementDefinition;

        return new WorkoutMovementDto
        {
            Id = entity.Id,
            MovementDefinitionId = entity.MovementDefinitionId,
            MovementName = definition?.DisplayName ?? string.Empty,
            MovementCategory = definition?.Category.ToString() ?? string.Empty,
            SequenceOrder = entity.SequenceOrder,
            RepCount = entity.RepCount,
            LoadValue = entity.LoadValue,
            LoadUnit = entity.LoadUnit?.ToString(),
            LoadFormatted = FormatLoad(entity.LoadValue, entity.LoadUnit?.ToString()),
            DistanceValue = entity.DistanceValue,
            DistanceUnit = entity.DistanceUnit?.ToString(),
            DistanceFormatted = FormatDistance(entity.DistanceValue, entity.DistanceUnit?.ToString()),
            Calories = entity.Calories,
            DurationSeconds = entity.DurationSeconds,
            DurationFormatted = FormatSeconds(entity.DurationSeconds),
            Notes = entity.Notes
        };
    }

    /// <summary>
    /// Maps a CreateWorkoutDto to a new Workout entity.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="userId">The user's unique identifier.</param>
    /// <returns>A new workout entity with movements.</returns>
    public static Workout ToEntity(this CreateWorkoutDto dto, int userId)
    {
        var entity = new Workout
        {
            UserId = userId,
            Name = dto.Name,
            WorkoutType = dto.WorkoutType,
            OriginalText = dto.OriginalText,
            ParsedDescription = dto.ParsedDescription,
            TimeCapSeconds = dto.TimeCapSeconds,
            RoundCount = dto.RoundCount,
            IntervalDurationSeconds = dto.IntervalDurationSeconds,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add movements
        foreach (var movementDto in dto.Movements)
        {
            entity.Movements.Add(movementDto.ToEntity());
        }

        return entity;
    }

    /// <summary>
    /// Maps a CreateWorkoutMovementDto to a new WorkoutMovement entity.
    /// </summary>
    /// <param name="dto">The create movement DTO.</param>
    /// <returns>A new workout movement entity.</returns>
    public static WorkoutMovement ToEntity(this CreateWorkoutMovementDto dto)
    {
        return new WorkoutMovement
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            SequenceOrder = dto.SequenceOrder,
            RepCount = dto.RepCount,
            LoadValue = dto.LoadValue,
            LoadUnit = dto.LoadUnit,
            DistanceValue = dto.DistanceValue,
            DistanceUnit = dto.DistanceUnit,
            Calories = dto.Calories,
            DurationSeconds = dto.DurationSeconds,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Applies values from UpdateWorkoutDto to an existing Workout entity.
    /// </summary>
    /// <param name="dto">The update DTO.</param>
    /// <param name="entity">The entity to update.</param>
    /// <param name="database">Database interface for removing old movements.</param>
    public static void ApplyTo(this UpdateWorkoutDto dto, Workout entity, IWodStratDatabase database)
    {
        entity.Name = dto.Name;
        entity.WorkoutType = dto.WorkoutType;
        entity.ParsedDescription = dto.ParsedDescription;
        entity.TimeCapSeconds = dto.TimeCapSeconds;
        entity.RoundCount = dto.RoundCount;
        entity.IntervalDurationSeconds = dto.IntervalDurationSeconds;
        entity.UpdatedAt = DateTime.UtcNow;

        // Replace movements if provided
        if (dto.Movements != null)
        {
            // Remove existing movements
            foreach (var movement in entity.Movements.ToList())
            {
                database.Remove(movement);
            }
            entity.Movements.Clear();

            // Add new movements
            foreach (var movementDto in dto.Movements)
            {
                entity.Movements.Add(movementDto.ToEntity());
            }
        }
    }

    /// <summary>
    /// Creates a CreateWorkoutDto from a ParsedWorkoutDto.
    /// Used to convert parsing results to creation input.
    /// </summary>
    /// <param name="parsed">The parsed workout DTO.</param>
    /// <returns>A create workout DTO.</returns>
    public static CreateWorkoutDto ToCreateDto(this ParsedWorkoutDto parsed)
    {
        return new CreateWorkoutDto
        {
            WorkoutType = parsed.WorkoutType,
            OriginalText = parsed.OriginalText,
            ParsedDescription = parsed.ParsedDescription,
            TimeCapSeconds = parsed.TimeCapSeconds,
            RoundCount = parsed.RoundCount,
            IntervalDurationSeconds = parsed.IntervalDurationSeconds,
            Movements = parsed.Movements
                .Where(m => m.MovementDefinitionId.HasValue)
                .Select(m => new CreateWorkoutMovementDto
                {
                    MovementDefinitionId = m.MovementDefinitionId!.Value,
                    SequenceOrder = m.SequenceOrder,
                    RepCount = m.RepCount,
                    LoadValue = m.LoadValue,
                    LoadUnit = m.LoadUnit,
                    DistanceValue = m.DistanceValue,
                    DistanceUnit = m.DistanceUnit,
                    Calories = m.Calories,
                    DurationSeconds = m.DurationSeconds,
                    Notes = m.Notes
                })
                .ToList()
        };
    }

    /// <summary>
    /// Creates a CreateWorkoutDto from a ParsedWorkoutResult.
    /// Returns null if the result is not usable.
    /// </summary>
    /// <param name="result">The parsed workout result.</param>
    /// <param name="name">Optional workout name to use.</param>
    /// <returns>A create workout DTO, or null if result is not usable.</returns>
    public static CreateWorkoutDto? ToCreateDto(this ParsedWorkoutResult result, string? name = null)
    {
        if (!result.IsUsable || result.ParsedWorkout == null)
        {
            return null;
        }

        var dto = result.ParsedWorkout.ToCreateDto();
        if (!string.IsNullOrWhiteSpace(name))
        {
            dto.Name = name;
        }
        return dto;
    }

    #region Formatting Helpers

    private static string? FormatSeconds(int? totalSeconds)
    {
        if (!totalSeconds.HasValue) return null;

        var minutes = totalSeconds.Value / 60;
        var seconds = totalSeconds.Value % 60;
        return $"{minutes}:{seconds:D2}";
    }

    private static string? FormatLoad(decimal? value, string? unit)
    {
        if (!value.HasValue || string.IsNullOrEmpty(unit)) return null;
        return $"{value.Value:G} {unit.ToLower()}";
    }

    private static string? FormatDistance(decimal? value, string? unit)
    {
        if (!value.HasValue || string.IsNullOrEmpty(unit)) return null;
        return $"{value.Value:G} {unit.ToLower()}";
    }

    #endregion
}
