using System.Text;
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
            Movements = dto.Movements.Select(m => m.ToResponse()).ToList()
        };
    }

    /// <summary>
    /// Maps ParsedWorkoutDto to ParsedWorkoutResponse with confidence score.
    /// </summary>
    public static ParsedWorkoutResponse ToResponse(this ParsedWorkoutDto dto, decimal? confidenceScore)
    {
        var response = dto.ToResponse();
        response.ParseConfidence = confidenceScore;
        return response;
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

    #region ParsedWorkoutResult Mappings

    /// <summary>
    /// Maps ParsedWorkoutResult to ParsedWorkoutResultResponse.
    /// </summary>
    public static ParsedWorkoutResultResponse ToResultResponse(this ParsedWorkoutResult result)
    {
        var confidenceScore = result.ConfidenceScore / 100m;

        return new ParsedWorkoutResultResponse
        {
            Success = result.Success,
            Errors = result.Errors.Select(e => e.ToErrorResponse()).ToList(),
            Warnings = result.Warnings.Select(w => w.ToWarningResponse()).ToList(),
            ParsedWorkout = result.Success ? result.ParsedWorkout?.ToResponse(confidenceScore) : null,
            PartialResult = !result.Success && result.ParsedWorkout != null
                ? result.ParsedWorkout.ToResponse(confidenceScore)
                : null,
            ParseConfidence = confidenceScore,
            ConfidenceLevel = result.ConfidenceLevel,
            ConfidenceDetails = result.ConfidenceDetails?.ToResponse(),
            IsUsable = result.IsUsable
        };
    }

    /// <summary>
    /// Maps ParsingErrorDto to ParsingErrorResponse with enhanced fields.
    /// </summary>
    public static ParsingErrorResponse ToErrorResponse(this ParsingErrorDto dto)
    {
        return new ParsingErrorResponse
        {
            Code = ConvertToScreamingSnakeCase(dto.ErrorType),
            Message = dto.Message,
            Suggestion = dto.Suggestion ?? GenerateErrorSuggestion(dto.ErrorType),
            Line = dto.LineNumber > 0 ? dto.LineNumber : null,
            Severity = "error",
            OriginalText = dto.OriginalText
        };
    }

    /// <summary>
    /// Maps ParsingWarningDto to ParsingWarningResponse.
    /// </summary>
    public static ParsingWarningResponse ToWarningResponse(this ParsingWarningDto dto)
    {
        return new ParsingWarningResponse
        {
            Code = ConvertToScreamingSnakeCase(dto.WarningType),
            Message = dto.Message,
            Suggestion = dto.Suggestion,
            Line = dto.LineNumber > 0 ? dto.LineNumber : null,
            Severity = "warning",
            OriginalText = dto.OriginalText
        };
    }

    /// <summary>
    /// Maps ConfidenceBreakdown to ConfidenceBreakdownResponse.
    /// </summary>
    public static ConfidenceBreakdownResponse ToResponse(this ConfidenceBreakdown breakdown)
    {
        return new ConfidenceBreakdownResponse
        {
            WorkoutTypeConfidence = breakdown.WorkoutTypeConfidence / 100m,
            TimeDomainConfidence = breakdown.TimeDomainConfidence / 100m,
            MovementIdentificationConfidence = breakdown.MovementIdentificationConfidence / 100m,
            MovementsIdentified = breakdown.MovementsIdentified,
            TotalMovementLines = breakdown.TotalMovementLines,
            MovementsWithCompleteData = breakdown.MovementsWithCompleteData,
            MovementIdentificationRate = breakdown.MovementIdentificationRate
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

    #region Helper Methods for Error Handling

    /// <summary>
    /// Converts PascalCase/camelCase to SCREAMING_SNAKE_CASE.
    /// </summary>
    private static string ConvertToScreamingSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToUpperInvariant(c));
        }
        return result.ToString();
    }

    /// <summary>
    /// Generates a helpful suggestion based on error type.
    /// </summary>
    private static string? GenerateErrorSuggestion(string errorType)
    {
        return errorType switch
        {
            "EmptyInput" => "Enter workout text with movements on separate lines (e.g., '10 Pull-ups').",
            "NoMovements" or "NoMovementsDetected" =>
                "Make sure each movement is on its own line with a rep count (e.g., '10 Pull-ups').",
            "UnrecognizedMovement" or "UnknownMovement" =>
                "Check the spelling of the movement name. Common movements include Pull-ups, Push-ups, Squats, etc.",
            "AmbiguousWorkoutType" =>
                "Try specifying the workout type explicitly (e.g., 'AMRAP 20 min', 'For Time', '5 Rounds').",
            "InvalidRepCount" or "MissingRepCount" =>
                "Each movement needs a rep count at the start (e.g., '10 Pull-ups') or use 'Max' for max reps.",
            "InvalidTimeDomain" =>
                "Specify time in minutes (e.g., '20 min AMRAP') or MM:SS format (e.g., '20:00 time cap').",
            _ => null
        };
    }

    #endregion
}
