using WodStrat.Api.ViewModels.VolumeLoad;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Volume Load API ViewModels and Service DTOs.
/// </summary>
public static class VolumeLoadMappingExtensions
{
    /// <summary>
    /// Maps WorkoutVolumeLoadResultDto to WorkoutVolumeLoadResponse.
    /// </summary>
    public static WorkoutVolumeLoadResponse ToResponse(this WorkoutVolumeLoadResultDto dto)
    {
        return new WorkoutVolumeLoadResponse
        {
            WorkoutId = dto.WorkoutId,
            WorkoutName = dto.WorkoutName,
            MovementVolumes = dto.MovementVolumes.Select(m => m.ToResponse()).ToList(),
            TotalVolumeLoad = dto.TotalVolumeLoad,
            TotalVolumeLoadFormatted = dto.TotalVolumeLoadFormatted,
            OverallAssessment = dto.OverallAssessment,
            CalculatedAt = dto.CalculatedAt
        };
    }

    /// <summary>
    /// Maps MovementVolumeLoadDto to MovementVolumeLoadResponse.
    /// </summary>
    public static MovementVolumeLoadResponse ToResponse(this MovementVolumeLoadDto dto)
    {
        return new MovementVolumeLoadResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            MovementName = dto.MovementName,
            Weight = dto.Weight,
            WeightUnit = dto.WeightUnit,
            Reps = dto.Reps,
            Rounds = dto.Rounds,
            VolumeLoad = dto.VolumeLoad,
            VolumeLoadFormatted = dto.VolumeLoadFormatted,
            LoadClassification = dto.LoadClassification,
            BenchmarkUsed = string.IsNullOrEmpty(dto.BenchmarkUsed) ? null : dto.BenchmarkUsed,
            AthleteBenchmarkPercentile = dto.AthleteBenchmarkPercentile,
            Tip = dto.Tip,
            RecommendedWeight = dto.RecommendedWeight,
            RecommendedWeightFormatted = string.IsNullOrEmpty(dto.RecommendedWeightFormatted) ? null : dto.RecommendedWeightFormatted
        };
    }
}
