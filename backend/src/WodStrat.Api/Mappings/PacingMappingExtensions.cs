using WodStrat.Api.ViewModels.Pacing;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Pacing API ViewModels and Service DTOs.
/// </summary>
public static class PacingMappingExtensions
{
    /// <summary>
    /// Maps WorkoutPacingResultDto to WorkoutPacingResponse.
    /// </summary>
    public static WorkoutPacingResponse ToResponse(this WorkoutPacingResultDto dto)
    {
        return new WorkoutPacingResponse
        {
            WorkoutId = dto.WorkoutId,
            WorkoutName = dto.WorkoutName,
            MovementPacing = dto.MovementPacing.Select(m => m.ToResponse()).ToList(),
            OverallStrategyNotes = dto.OverallStrategyNotes,
            CalculatedAt = dto.CalculatedAt
        };
    }

    /// <summary>
    /// Maps MovementPacingDto to MovementPacingResponse.
    /// </summary>
    public static MovementPacingResponse ToResponse(this MovementPacingDto dto)
    {
        return new MovementPacingResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            MovementName = dto.MovementName,
            PacingLevel = dto.PacingLevel,
            AthletePercentile = dto.AthletePercentile,
            GuidanceText = dto.GuidanceText,
            RecommendedSets = dto.RecommendedSets.Length > 0 ? dto.RecommendedSets : null,
            BenchmarkUsed = string.IsNullOrEmpty(dto.BenchmarkUsed) ? null : dto.BenchmarkUsed,
            IsCardio = dto.IsCardio,
            TargetPace = dto.TargetPace?.ToResponse()
        };
    }

    /// <summary>
    /// Maps CardioPaceDto to CardioPaceResponse.
    /// </summary>
    public static CardioPaceResponse ToResponse(this CardioPaceDto dto)
    {
        return new CardioPaceResponse
        {
            DisplayPrimary = dto.DisplayPrimary,
            DisplaySecondary = dto.DisplaySecondary,
            ValuePerUnit = dto.ValuePerUnit,
            PaceUnit = dto.PaceUnit
        };
    }
}
