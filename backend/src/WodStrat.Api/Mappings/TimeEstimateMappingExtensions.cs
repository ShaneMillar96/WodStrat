using WodStrat.Api.ViewModels.TimeEstimate;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Time Estimate API ViewModels and Service DTOs.
/// </summary>
public static class TimeEstimateMappingExtensions
{
    /// <summary>
    /// Maps TimeEstimateResultDto to TimeEstimateResponse.
    /// </summary>
    public static TimeEstimateResponse ToResponse(this TimeEstimateResultDto dto)
    {
        return new TimeEstimateResponse
        {
            WorkoutId = dto.WorkoutId,
            WorkoutName = dto.WorkoutName,
            WorkoutType = dto.WorkoutType,
            EstimateType = dto.EstimateType,
            MinEstimate = dto.MinEstimate,
            MaxEstimate = dto.MaxEstimate,
            MinExtraReps = dto.MinExtraReps,
            MaxExtraReps = dto.MaxExtraReps,
            FormattedRange = dto.FormattedRange,
            ConfidenceLevel = dto.ConfidenceLevel,
            FactorsSummary = dto.FactorsSummary,
            RestRecommendations = dto.RestRecommendations.Select(r => r.ToResponse()).ToList(),
            EmomFeasibility = dto.EmomFeasibility?.Select(e => e.ToResponse()).ToList(),
            CalculatedAt = dto.CalculatedAt,
            BenchmarkCoverageCount = dto.BenchmarkCoverageCount,
            TotalMovementCount = dto.TotalMovementCount,
            AveragePercentile = dto.AveragePercentile
        };
    }

    /// <summary>
    /// Maps RestRecommendationDto to RestRecommendationResponse.
    /// </summary>
    public static RestRecommendationResponse ToResponse(this RestRecommendationDto dto)
    {
        return new RestRecommendationResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            AfterMovement = dto.AfterMovement,
            SuggestedRestSeconds = dto.SuggestedRestSeconds,
            RestRange = dto.RestRange,
            Reasoning = dto.Reasoning,
            PacingLevel = dto.PacingLevel
        };
    }

    /// <summary>
    /// Maps EmomFeasibilityDto to EmomMinuteResponse.
    /// </summary>
    public static EmomMinuteResponse ToResponse(this EmomFeasibilityDto dto)
    {
        return new EmomMinuteResponse
        {
            Minute = dto.Minute,
            PrescribedWork = dto.PrescribedWork,
            EstimatedCompletionSeconds = dto.EstimatedCompletionSeconds,
            IsFeasible = dto.IsFeasible,
            BufferSeconds = dto.BufferSeconds,
            Recommendation = dto.Recommendation,
            MovementNames = dto.MovementNames
        };
    }

    /// <summary>
    /// Maps a list of EmomFeasibilityDto to EmomFeasibilityResponse with aggregate data.
    /// </summary>
    public static EmomFeasibilityResponse ToEmomFeasibilityResponse(
        this IReadOnlyList<EmomFeasibilityDto> feasibilityList,
        int workoutId,
        string? workoutName)
    {
        var overallFeasible = feasibilityList.All(f => f.IsFeasible);
        var avgBuffer = feasibilityList.Count > 0
            ? feasibilityList.Average(f => f.BufferSeconds)
            : 0;

        var assessment = overallFeasible
            ? avgBuffer >= 15
                ? "This EMOM is feasible with comfortable buffers. Maintain consistent pacing."
                : avgBuffer >= 10
                    ? "This EMOM is feasible with adequate buffers. Stay focused on transitions."
                    : "This EMOM is feasible but tight. Consider pacing conservatively early."
            : "Some minutes may be challenging. Consider scaling movements or reps for a sustainable pace.";

        return new EmomFeasibilityResponse
        {
            WorkoutId = workoutId,
            WorkoutName = workoutName,
            TotalMinutes = feasibilityList.Count,
            OverallFeasible = overallFeasible,
            OverallAssessment = assessment,
            MinuteBreakdown = feasibilityList.Select(f => f.ToResponse()).ToList(),
            CalculatedAt = DateTime.UtcNow
        };
    }
}
