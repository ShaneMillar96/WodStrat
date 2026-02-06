using WodStrat.Api.ViewModels.Pacing;
using WodStrat.Api.ViewModels.Strategy;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between unified Strategy API ViewModels and Service DTOs.
/// </summary>
public static class StrategyMappingExtensions
{
    /// <summary>
    /// Maps UnifiedStrategyResultDto to WorkoutStrategyResponse.
    /// </summary>
    public static WorkoutStrategyResponse ToResponse(this UnifiedStrategyResultDto dto)
    {
        return new WorkoutStrategyResponse
        {
            Workout = dto.Context.ToWorkoutContextResponse(),
            MovementContexts = dto.Context.Movements.Select(m => m.ToResponse()).ToList(),
            Pacing = dto.Pacing.ToStrategyResponse(),
            VolumeLoad = dto.VolumeLoad.ToStrategyResponse(),
            TimeEstimate = dto.TimeEstimate.ToStrategyResponse(),
            Insights = dto.Insights.ToSummaryResponse(),
            CalculatedAt = dto.Context.CalculatedAt
        };
    }

    /// <summary>
    /// Maps WorkoutContextDto to WorkoutContextResponse.
    /// </summary>
    public static WorkoutContextResponse ToWorkoutContextResponse(this WorkoutContextDto dto)
    {
        return new WorkoutContextResponse
        {
            WorkoutId = dto.WorkoutId,
            WorkoutName = dto.WorkoutName,
            WorkoutType = dto.WorkoutType,
            Rounds = dto.RoundCount,
            TimeCap = dto.TimeCapSeconds
        };
    }

    /// <summary>
    /// Maps MovementContextDto to MovementContextResponse.
    /// </summary>
    public static MovementContextResponse ToResponse(this MovementContextDto dto)
    {
        return new MovementContextResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            MovementName = dto.MovementName,
            AthletePercentile = dto.AthletePercentile,
            BenchmarkUsed = string.IsNullOrEmpty(dto.BenchmarkUsed) || dto.BenchmarkUsed == "None"
                ? null
                : dto.BenchmarkUsed,
            HasBenchmarkData = dto.HasAthleteBenchmark
        };
    }

    /// <summary>
    /// Maps PacingAnalysisSummaryDto to StrategyPacingResponse.
    /// </summary>
    public static StrategyPacingResponse? ToStrategyResponse(this PacingAnalysisSummaryDto? dto)
    {
        if (dto is null)
            return null;

        return new StrategyPacingResponse
        {
            Movements = dto.Movements.Select(m => m.ToDetailResponse()).ToList(),
            OverallStrategyNotes = dto.OverallStrategyNotes,
            Distribution = dto.Distribution.ToResponse(),
            IsComplete = dto.IsComplete
        };
    }

    /// <summary>
    /// Maps MovementPacingAnalysisDto to MovementPacingDetailResponse.
    /// </summary>
    public static MovementPacingDetailResponse ToDetailResponse(this MovementPacingAnalysisDto dto)
    {
        return new MovementPacingDetailResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            PacingLevel = dto.PacingLevel,
            GuidanceText = dto.GuidanceText,
            RecommendedSets = dto.RecommendedSets.Length > 0 ? dto.RecommendedSets : null,
            IsCardio = dto.IsCardio,
            TargetPace = dto.TargetPace?.ToResponse()
        };
    }

    /// <summary>
    /// Maps PacingDistributionDto to PacingDistributionResponse.
    /// </summary>
    public static PacingDistributionResponse ToResponse(this PacingDistributionDto dto)
    {
        return new PacingDistributionResponse
        {
            HeavyCount = dto.HeavyCount,
            ModerateCount = dto.ModerateCount,
            LightCount = dto.LightCount,
            TotalMovements = dto.TotalMovements,
            IncompleteDataCount = dto.IncompleteDataCount
        };
    }

    /// <summary>
    /// Maps VolumeLoadAnalysisSummaryDto to StrategyVolumeLoadResponse.
    /// </summary>
    public static StrategyVolumeLoadResponse? ToStrategyResponse(this VolumeLoadAnalysisSummaryDto? dto)
    {
        if (dto is null)
            return null;

        return new StrategyVolumeLoadResponse
        {
            Movements = dto.Movements.Select(m => m.ToDetailResponse()).ToList(),
            TotalVolumeLoad = dto.TotalVolumeLoad,
            TotalVolumeLoadFormatted = dto.TotalVolumeLoadFormatted,
            OverallAssessment = dto.OverallAssessment,
            Distribution = dto.Distribution.ToResponse(),
            IsComplete = dto.IsComplete
        };
    }

    /// <summary>
    /// Maps MovementVolumeAnalysisDto to MovementVolumeLoadDetailResponse.
    /// </summary>
    public static MovementVolumeLoadDetailResponse ToDetailResponse(this MovementVolumeAnalysisDto dto)
    {
        return new MovementVolumeLoadDetailResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            Weight = dto.Weight,
            WeightUnit = dto.WeightUnit,
            Reps = dto.Reps,
            Rounds = dto.Rounds,
            VolumeLoad = dto.VolumeLoad,
            VolumeLoadFormatted = dto.VolumeLoadFormatted,
            LoadClassification = dto.LoadClassification,
            Tip = dto.Tip,
            RecommendedWeight = dto.RecommendedWeight,
            RecommendedWeightFormatted = dto.RecommendedWeightFormatted
        };
    }

    /// <summary>
    /// Maps VolumeLoadDistributionDto to VolumeLoadDistributionResponse.
    /// </summary>
    public static VolumeLoadDistributionResponse ToResponse(this VolumeLoadDistributionDto dto)
    {
        return new VolumeLoadDistributionResponse
        {
            HighCount = dto.HighCount,
            ModerateCount = dto.ModerateCount,
            LowCount = dto.LowCount,
            BodyweightCount = dto.BodyweightCount,
            TotalMovements = dto.TotalMovements,
            InsufficientDataCount = dto.InsufficientDataCount
        };
    }

    /// <summary>
    /// Maps TimeEstimateAnalysisSummaryDto to StrategyTimeEstimateResponse.
    /// </summary>
    public static StrategyTimeEstimateResponse? ToStrategyResponse(this TimeEstimateAnalysisSummaryDto? dto)
    {
        if (dto is null)
            return null;

        return new StrategyTimeEstimateResponse
        {
            EstimateType = dto.EstimateType,
            MinEstimate = dto.MinEstimate,
            MaxEstimate = dto.MaxEstimate,
            MinExtraReps = dto.MinExtraReps,
            MaxExtraReps = dto.MaxExtraReps,
            FormattedRange = dto.FormattedRange,
            ConfidenceLevel = dto.ConfidenceLevel,
            FactorsSummary = dto.FactorsSummary,
            RestRecommendations = dto.RestRecommendations.Select(r => r.ToDetailResponse()).ToList(),
            EmomFeasibility = dto.EmomFeasibility?.Select(e => e.ToDetailResponse()).ToList(),
            BenchmarkCoverageCount = dto.BenchmarkCoverageCount,
            TotalMovementCount = dto.TotalMovementCount,
            AveragePercentile = dto.AveragePercentile
        };
    }

    /// <summary>
    /// Maps RestRecommendationSummaryDto to RestRecommendationDetailResponse.
    /// </summary>
    public static RestRecommendationDetailResponse ToDetailResponse(this RestRecommendationSummaryDto dto)
    {
        return new RestRecommendationDetailResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            SuggestedRestSeconds = dto.SuggestedRestSeconds,
            RestRange = dto.RestRange,
            Reasoning = dto.Reasoning,
            PacingLevel = dto.PacingLevel
        };
    }

    /// <summary>
    /// Maps EmomFeasibilityDto to EmomMinuteDetailResponse.
    /// </summary>
    public static EmomMinuteDetailResponse ToDetailResponse(this EmomFeasibilityDto dto)
    {
        return new EmomMinuteDetailResponse
        {
            Minute = dto.Minute,
            PrescribedWork = dto.PrescribedWork,
            EstimatedCompletionSeconds = dto.EstimatedCompletionSeconds,
            IsFeasible = dto.IsFeasible,
            BufferSeconds = dto.BufferSeconds,
            Recommendation = dto.Recommendation,
            // Note: EmomFeasibilityDto has MovementNames (strings), we need to map to IDs
            // For now, returning empty array - the unified service should provide IDs
            MovementDefinitionIds = Array.Empty<int>()
        };
    }

    /// <summary>
    /// Maps StrategyInsightsSummaryDto to StrategyInsightsSummaryResponse.
    /// </summary>
    public static StrategyInsightsSummaryResponse? ToSummaryResponse(this StrategyInsightsSummaryDto? dto)
    {
        if (dto is null)
            return null;

        return new StrategyInsightsSummaryResponse
        {
            DifficultyScore = dto.DifficultyScore.ToDetailResponse(),
            StrategyConfidence = dto.StrategyConfidence.ToDetailResponse(),
            KeyFocusMovements = dto.KeyFocusMovements.Select(k => k.ToDetailResponse()).ToList(),
            RiskAlerts = dto.RiskAlerts.Select(r => r.ToDetailResponse()).ToList(),
            StrategySummary = dto.StrategySummary
        };
    }

    /// <summary>
    /// Maps DifficultyScoreDto to DifficultyScoreDetailResponse.
    /// </summary>
    public static DifficultyScoreDetailResponse ToDetailResponse(this DifficultyScoreDto dto)
    {
        return new DifficultyScoreDetailResponse
        {
            Score = dto.Score,
            Label = dto.Label,
            Description = dto.Description,
            Breakdown = dto.Breakdown.ToDetailResponse()
        };
    }

    /// <summary>
    /// Maps DifficultyBreakdownDto to DifficultyBreakdownDetailResponse.
    /// </summary>
    public static DifficultyBreakdownDetailResponse ToDetailResponse(this DifficultyBreakdownDto dto)
    {
        return new DifficultyBreakdownDetailResponse
        {
            PacingFactor = dto.PacingFactor,
            VolumeFactor = dto.VolumeFactor,
            TimeFactor = dto.TimeFactor,
            ExperienceModifier = dto.ExperienceModifier
        };
    }

    /// <summary>
    /// Maps StrategyConfidenceDto to StrategyConfidenceDetailResponse.
    /// </summary>
    public static StrategyConfidenceDetailResponse ToDetailResponse(this StrategyConfidenceDto dto)
    {
        return new StrategyConfidenceDetailResponse
        {
            Level = dto.Level,
            Percentage = dto.Percentage,
            Explanation = dto.Explanation,
            MissingBenchmarks = dto.MissingBenchmarks.ToList()
        };
    }

    /// <summary>
    /// Maps KeyFocusMovementSummaryDto to KeyFocusMovementDetailResponse.
    /// </summary>
    public static KeyFocusMovementDetailResponse ToDetailResponse(this KeyFocusMovementSummaryDto dto)
    {
        return new KeyFocusMovementDetailResponse
        {
            MovementDefinitionId = dto.MovementDefinitionId,
            Reason = dto.Reason,
            Recommendation = dto.Recommendation,
            Priority = dto.Priority
        };
    }

    /// <summary>
    /// Maps RiskAlertDto to RiskAlertDetailResponse.
    /// </summary>
    public static RiskAlertDetailResponse ToDetailResponse(this RiskAlertDto dto)
    {
        return new RiskAlertDetailResponse
        {
            AlertType = dto.AlertType,
            Severity = dto.Severity,
            Title = dto.Title,
            Message = dto.Message,
            // RiskAlertDto has AffectedMovements as strings, we map to empty for now
            // The unified service should be updated to provide movement IDs
            AffectedMovementIds = new List<int>(),
            SuggestedAction = dto.SuggestedAction
        };
    }
}
