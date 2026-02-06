using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping strategy result DTOs to slim summary DTOs.
/// </summary>
public static class UnifiedStrategyMappingExtensions
{
    /// <summary>
    /// Converts WorkoutPacingResultDto to slim summary for unified response.
    /// </summary>
    /// <param name="result">The full pacing result.</param>
    /// <returns>A slim PacingAnalysisSummaryDto.</returns>
    public static PacingAnalysisSummaryDto ToPacingSummary(this WorkoutPacingResultDto result)
    {
        var movements = result.MovementPacing
            .Select(m => new MovementPacingAnalysisDto
            {
                MovementDefinitionId = m.MovementDefinitionId,
                PacingLevel = m.PacingLevel,
                GuidanceText = m.GuidanceText,
                RecommendedSets = m.RecommendedSets
            })
            .ToList();

        return new PacingAnalysisSummaryDto
        {
            Movements = movements,
            OverallStrategyNotes = result.OverallStrategyNotes,
            Distribution = result.Distribution,
            IsComplete = result.IsComplete
        };
    }

    /// <summary>
    /// Converts WorkoutVolumeLoadResultDto to slim summary for unified response.
    /// </summary>
    /// <param name="result">The full volume load result.</param>
    /// <returns>A slim VolumeLoadAnalysisSummaryDto.</returns>
    public static VolumeLoadAnalysisSummaryDto ToVolumeLoadSummary(this WorkoutVolumeLoadResultDto result)
    {
        var movements = result.MovementVolumes
            .Select(m => new MovementVolumeAnalysisDto
            {
                MovementDefinitionId = m.MovementDefinitionId,
                Weight = m.Weight,
                WeightUnit = m.WeightUnit,
                Reps = m.Reps,
                Rounds = m.Rounds,
                VolumeLoad = m.VolumeLoad,
                VolumeLoadFormatted = m.VolumeLoadFormatted,
                LoadClassification = m.LoadClassification,
                Tip = m.Tip,
                RecommendedWeight = m.RecommendedWeight,
                RecommendedWeightFormatted = m.RecommendedWeightFormatted
            })
            .ToList();

        return new VolumeLoadAnalysisSummaryDto
        {
            Movements = movements,
            TotalVolumeLoad = result.TotalVolumeLoad,
            TotalVolumeLoadFormatted = result.TotalVolumeLoadFormatted,
            OverallAssessment = result.OverallAssessment,
            Distribution = result.Distribution,
            IsComplete = result.IsComplete
        };
    }

    /// <summary>
    /// Converts TimeEstimateResultDto to slim summary for unified response.
    /// </summary>
    /// <param name="result">The full time estimate result.</param>
    /// <returns>A slim TimeEstimateAnalysisSummaryDto.</returns>
    public static TimeEstimateAnalysisSummaryDto ToTimeEstimateSummary(this TimeEstimateResultDto result)
    {
        var restRecommendations = result.RestRecommendations
            .Select(r => new RestRecommendationSummaryDto
            {
                MovementDefinitionId = r.MovementDefinitionId,
                SuggestedRestSeconds = r.SuggestedRestSeconds,
                RestRange = r.RestRange,
                Reasoning = r.Reasoning,
                PacingLevel = r.PacingLevel
            })
            .ToList();

        return new TimeEstimateAnalysisSummaryDto
        {
            EstimateType = result.EstimateType,
            MinEstimate = result.MinEstimate,
            MaxEstimate = result.MaxEstimate,
            MinExtraReps = result.MinExtraReps,
            MaxExtraReps = result.MaxExtraReps,
            FormattedRange = result.FormattedRange,
            ConfidenceLevel = result.ConfidenceLevel,
            FactorsSummary = result.FactorsSummary,
            RestRecommendations = restRecommendations,
            EmomFeasibility = result.EmomFeasibility,
            BenchmarkCoverageCount = result.BenchmarkCoverageCount,
            TotalMovementCount = result.TotalMovementCount,
            AveragePercentile = result.AveragePercentile
        };
    }

    /// <summary>
    /// Converts StrategyInsightsResultDto to slim summary for unified response.
    /// </summary>
    /// <param name="result">The full strategy insights result.</param>
    /// <returns>A slim StrategyInsightsSummaryDto.</returns>
    public static StrategyInsightsSummaryDto ToInsightsSummary(this StrategyInsightsResultDto result)
    {
        var keyFocusMovements = result.KeyFocusMovements
            .Select(m => new KeyFocusMovementSummaryDto
            {
                MovementDefinitionId = m.MovementDefinitionId,
                Reason = m.Reason,
                Recommendation = m.Recommendation,
                Priority = m.Priority,
                PacingLevel = m.PacingLevel,
                LoadClassification = m.LoadClassification,
                ScalingRecommended = m.ScalingRecommended
            })
            .ToList();

        return new StrategyInsightsSummaryDto
        {
            DifficultyScore = result.DifficultyScore,
            StrategyConfidence = result.StrategyConfidence,
            KeyFocusMovements = keyFocusMovements,
            RiskAlerts = result.RiskAlerts,
            StrategySummary = result.StrategySummary
        };
    }
}
