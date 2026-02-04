using WodStrat.Api.ViewModels.StrategyInsights;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Strategy Insights API ViewModels and Service DTOs.
/// </summary>
public static class StrategyInsightsMappingExtensions
{
    /// <summary>
    /// Maps StrategyInsightsResultDto to StrategyInsightsResponse.
    /// </summary>
    public static StrategyInsightsResponse ToResponse(this StrategyInsightsResultDto dto)
    {
        return new StrategyInsightsResponse
        {
            WorkoutId = dto.WorkoutId,
            WorkoutName = dto.WorkoutName,
            DifficultyScore = dto.DifficultyScore.ToResponse(),
            StrategyConfidence = dto.StrategyConfidence.ToResponse(),
            KeyFocusMovements = dto.KeyFocusMovements.Select(m => m.ToResponse()).ToList(),
            RiskAlerts = dto.RiskAlerts.Select(a => a.ToResponse()).ToList(),
            CalculatedAt = dto.CalculatedAt
        };
    }

    /// <summary>
    /// Maps DifficultyScoreDto to DifficultyScoreResponse.
    /// </summary>
    public static DifficultyScoreResponse ToResponse(this DifficultyScoreDto dto)
    {
        return new DifficultyScoreResponse
        {
            Score = dto.Score,
            Label = dto.Label,
            Description = dto.Description,
            Breakdown = dto.Breakdown.ToResponse()
        };
    }

    /// <summary>
    /// Maps DifficultyBreakdownDto to DifficultyBreakdownResponse.
    /// </summary>
    public static DifficultyBreakdownResponse ToResponse(this DifficultyBreakdownDto dto)
    {
        return new DifficultyBreakdownResponse
        {
            PacingFactor = dto.PacingFactor,
            VolumeFactor = dto.VolumeFactor,
            TimeFactor = dto.TimeFactor,
            ExperienceModifier = dto.ExperienceModifier
        };
    }

    /// <summary>
    /// Maps StrategyConfidenceDto to StrategyConfidenceResponse.
    /// </summary>
    public static StrategyConfidenceResponse ToResponse(this StrategyConfidenceDto dto)
    {
        return new StrategyConfidenceResponse
        {
            Level = dto.Level,
            Percentage = dto.Percentage,
            Explanation = dto.Explanation,
            MissingBenchmarks = dto.MissingBenchmarks.ToList()
        };
    }

    /// <summary>
    /// Maps KeyFocusMovementDto to KeyFocusMovementResponse.
    /// </summary>
    public static KeyFocusMovementResponse ToResponse(this KeyFocusMovementDto dto)
    {
        return new KeyFocusMovementResponse
        {
            MovementName = dto.MovementName,
            Reason = dto.Reason,
            Recommendation = dto.Recommendation,
            Priority = dto.Priority
        };
    }

    /// <summary>
    /// Maps RiskAlertDto to RiskAlertResponse.
    /// </summary>
    public static RiskAlertResponse ToResponse(this RiskAlertDto dto)
    {
        return new RiskAlertResponse
        {
            AlertType = dto.AlertType,
            Severity = dto.Severity,
            Title = dto.Title,
            Message = dto.Message,
            AffectedMovements = dto.AffectedMovements.ToList(),
            SuggestedAction = dto.SuggestedAction
        };
    }
}
