using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Workout API ViewModels and Service DTOs.
/// </summary>
public static class WorkoutMappingExtensions
{
    #region DTO to Response Mappings

    /// <summary>
    /// Maps ParsedWorkoutDto to ParsedWorkoutResponse.
    /// </summary>
    public static ParsedWorkoutResponse ToResponse(this ParsedWorkoutDto dto)
    {
        return new ParsedWorkoutResponse
        {
            OriginalText = dto.OriginalText,
            ParsedDescription = dto.ParsedDescription,
            WorkoutType = dto.WorkoutType.ToString(),
            TimeCapSeconds = dto.TimeCapSeconds,
            TimeCapFormatted = FormatSeconds(dto.TimeCapSeconds),
            RoundCount = dto.RoundCount,
            IntervalDurationSeconds = dto.IntervalDurationSeconds,
            IntervalDurationFormatted = FormatSeconds(dto.IntervalDurationSeconds),
            Movements = dto.Movements.Select(m => m.ToResponse()).ToList(),
            Errors = dto.Errors.Select(e => e.ToResponse()).ToList(),
            IsValid = dto.IsValid
        };
    }

    /// <summary>
    /// Maps ParsedMovementDto to ParsedMovementResponse.
    /// </summary>
    public static ParsedMovementResponse ToResponse(this ParsedMovementDto dto)
    {
        return new ParsedMovementResponse
        {
            SequenceOrder = dto.SequenceOrder,
            OriginalText = dto.OriginalText,
            MovementDefinitionId = dto.MovementDefinitionId,
            MovementName = dto.MovementName,
            MovementCanonicalName = dto.MovementCanonicalName,
            MovementCategory = dto.MovementCategory,
            RepCount = dto.RepCount,
            LoadValue = dto.LoadValue,
            LoadValueFemale = dto.LoadValueFemale,
            LoadUnit = dto.LoadUnit?.ToString(),
            LoadFormatted = FormatLoad(dto.LoadValue, dto.LoadValueFemale, dto.LoadUnit?.ToString()),
            DistanceValue = dto.DistanceValue,
            DistanceUnit = dto.DistanceUnit?.ToString(),
            DistanceFormatted = FormatDistance(dto.DistanceValue, dto.DistanceUnit?.ToString()),
            Calories = dto.Calories,
            CaloriesFemale = dto.CaloriesFemale,
            DurationSeconds = dto.DurationSeconds,
            DurationFormatted = FormatSeconds(dto.DurationSeconds),
            Notes = dto.Notes
        };
    }

    /// <summary>
    /// Maps ParsingErrorDto to ParsingErrorResponse.
    /// </summary>
    public static ParsingErrorResponse ToResponse(this ParsingErrorDto dto)
    {
        return new ParsingErrorResponse
        {
            ErrorType = dto.ErrorType,
            Message = dto.Message,
            LineNumber = dto.LineNumber,
            OriginalText = dto.OriginalText
        };
    }

    /// <summary>
    /// Maps WorkoutDto to WorkoutResponse.
    /// </summary>
    public static WorkoutResponse ToResponse(this WorkoutDto dto)
    {
        return new WorkoutResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            WorkoutType = dto.WorkoutType,
            OriginalText = dto.OriginalText,
            ParsedDescription = dto.ParsedDescription,
            TimeCapSeconds = dto.TimeCapSeconds,
            TimeCapFormatted = dto.TimeCapFormatted,
            RoundCount = dto.RoundCount,
            IntervalDurationSeconds = dto.IntervalDurationSeconds,
            IntervalDurationFormatted = dto.IntervalDurationFormatted,
            Movements = dto.Movements.Select(m => m.ToResponse()).ToList(),
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// Maps WorkoutMovementDto to WorkoutMovementResponse.
    /// </summary>
    public static WorkoutMovementResponse ToResponse(this WorkoutMovementDto dto)
    {
        return new WorkoutMovementResponse
        {
            Id = dto.Id,
            MovementDefinitionId = dto.MovementDefinitionId,
            MovementName = dto.MovementName,
            MovementCategory = dto.MovementCategory,
            SequenceOrder = dto.SequenceOrder,
            RepCount = dto.RepCount,
            LoadValue = dto.LoadValue,
            LoadUnit = dto.LoadUnit,
            LoadFormatted = dto.LoadFormatted,
            DistanceValue = dto.DistanceValue,
            DistanceUnit = dto.DistanceUnit,
            DistanceFormatted = dto.DistanceFormatted,
            Calories = dto.Calories,
            DurationSeconds = dto.DurationSeconds,
            DurationFormatted = dto.DurationFormatted,
            Notes = dto.Notes
        };
    }

    #endregion

    #region Request to DTO Mappings

    /// <summary>
    /// Maps CreateWorkoutRequest to CreateWorkoutDto.
    /// </summary>
    public static CreateWorkoutDto ToDto(this CreateWorkoutRequest request)
    {
        return new CreateWorkoutDto
        {
            Name = request.Name?.Trim(),
            WorkoutType = Enum.Parse<WorkoutType>(request.WorkoutType, ignoreCase: true),
            OriginalText = request.OriginalText,
            TimeCapSeconds = request.TimeCapSeconds,
            RoundCount = request.RoundCount,
            IntervalDurationSeconds = request.IntervalDurationSeconds,
            Movements = request.Movements.Select(m => m.ToDto()).ToList()
        };
    }

    /// <summary>
    /// Maps CreateWorkoutMovementRequest to CreateWorkoutMovementDto.
    /// </summary>
    public static CreateWorkoutMovementDto ToDto(this CreateWorkoutMovementRequest request)
    {
        return new CreateWorkoutMovementDto
        {
            MovementDefinitionId = request.MovementDefinitionId,
            SequenceOrder = request.SequenceOrder,
            RepCount = request.RepCount,
            LoadValue = request.LoadValue,
            LoadUnit = ParseLoadUnit(request.LoadUnit),
            DistanceValue = request.DistanceValue,
            DistanceUnit = ParseDistanceUnit(request.DistanceUnit),
            Calories = request.Calories,
            DurationSeconds = request.DurationSeconds,
            Notes = request.Notes?.Trim()
        };
    }

    /// <summary>
    /// Maps UpdateWorkoutRequest to UpdateWorkoutDto.
    /// </summary>
    public static UpdateWorkoutDto ToDto(this UpdateWorkoutRequest request)
    {
        return new UpdateWorkoutDto
        {
            Name = request.Name?.Trim(),
            WorkoutType = Enum.Parse<WorkoutType>(request.WorkoutType, ignoreCase: true),
            TimeCapSeconds = request.TimeCapSeconds,
            RoundCount = request.RoundCount,
            IntervalDurationSeconds = request.IntervalDurationSeconds,
            Movements = request.Movements?.Select(m => m.ToDto()).ToList()
        };
    }

    #endregion

    #region Helper Methods

    private static string? FormatSeconds(int? totalSeconds)
    {
        if (!totalSeconds.HasValue) return null;

        var minutes = totalSeconds.Value / 60;
        var seconds = totalSeconds.Value % 60;
        return $"{minutes}:{seconds:D2}";
    }

    private static string? FormatLoad(decimal? value, decimal? valueFemale, string? unit)
    {
        if (!value.HasValue) return null;

        var unitStr = string.IsNullOrEmpty(unit) ? "lb" : unit.ToLower();

        if (valueFemale.HasValue && valueFemale != value)
        {
            return $"{value.Value:G}/{valueFemale.Value:G} {unitStr}";
        }

        return $"{value.Value:G} {unitStr}";
    }

    private static string? FormatDistance(decimal? value, string? unit)
    {
        if (!value.HasValue || string.IsNullOrEmpty(unit)) return null;
        return $"{value.Value:G} {unit.ToLower()}";
    }

    private static LoadUnit? ParseLoadUnit(string? unit)
    {
        if (string.IsNullOrEmpty(unit)) return null;
        return Enum.TryParse<LoadUnit>(unit, ignoreCase: true, out var result) ? result : null;
    }

    private static DistanceUnit? ParseDistanceUnit(string? unit)
    {
        if (string.IsNullOrEmpty(unit)) return null;
        return Enum.TryParse<DistanceUnit>(unit, ignoreCase: true, out var result) ? result : null;
    }

    #endregion
}
