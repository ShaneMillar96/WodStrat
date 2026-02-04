using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid strategy insights-related DTOs.
/// </summary>
public class StrategyInsightsCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Register DateOnly generator
        fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)));

        // Customize MovementPacingDto for strategy insights testing
        fixture.Customize<MovementPacingDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.PacingLevel, "Moderate")
            .With(x => x.AthletePercentile, 50m)
            .With(x => x.GuidanceText, "Maintain consistent pace.")
            .With(x => x.RecommendedSets, new[] { 10, 8, 7 })
            .With(x => x.BenchmarkUsed, "Front Squat 1RM")
            .With(x => x.HasPopulationData, true)
            .With(x => x.HasAthleteBenchmark, true));

        // Customize PacingDistributionDto
        fixture.Customize<PacingDistributionDto>(c => c
            .With(x => x.HeavyCount, 1)
            .With(x => x.ModerateCount, 1)
            .With(x => x.LightCount, 1)
            .With(x => x.TotalMovements, 3)
            .With(x => x.IncompleteDataCount, 0));

        // Customize WorkoutPacingResultDto
        fixture.Customize<WorkoutPacingResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.MovementPacing, () => fixture.CreateMany<MovementPacingDto>(3).ToList())
            .With(x => x.OverallStrategyNotes, "Test strategy notes.")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .With(x => x.Distribution, () => fixture.Create<PacingDistributionDto>())
            .With(x => x.IsComplete, true));

        // Customize MovementVolumeLoadDto
        fixture.Customize<MovementVolumeLoadDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.Weight, 43m)
            .With(x => x.Reps, 45)
            .With(x => x.Rounds, 1)
            .With(x => x.VolumeLoad, 1935m)
            .With(x => x.LoadClassification, "Moderate")
            .With(x => x.HasSufficientData, true));

        // Customize WorkoutVolumeLoadResultDto
        fixture.Customize<WorkoutVolumeLoadResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.TotalVolumeLoad, 3500m)
            .With(x => x.MovementVolumes, () => fixture.CreateMany<MovementVolumeLoadDto>(3).ToList())
            .With(x => x.CalculatedAt, DateTime.UtcNow));

        // Customize TimeEstimateResultDto
        fixture.Customize<TimeEstimateResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.EstimateType, "Time")
            .With(x => x.MinEstimate, 240)
            .With(x => x.MaxEstimate, 300)
            .With(x => x.ConfidenceLevel, "High")
            .With(x => x.CalculatedAt, DateTime.UtcNow));

        // Customize DifficultyBreakdownDto
        fixture.Customize<DifficultyBreakdownDto>(c => c
            .With(x => x.PacingFactor, 5.0m)
            .With(x => x.VolumeFactor, 5.0m)
            .With(x => x.TimeFactor, 5.0m)
            .With(x => x.ExperienceModifier, 1.0m)
            .With(x => x.BaseScore, 5.0m)
            .With(x => x.Explanation, "Based on balanced workout characteristics."));

        // Customize DifficultyScoreDto
        fixture.Customize<DifficultyScoreDto>(c => c
            .With(x => x.Score, 5)
            .With(x => x.Label, "Moderate")
            .With(x => x.Description, "Balanced challenge. Pace yourself and stay mentally engaged.")
            .With(x => x.Breakdown, () => fixture.Create<DifficultyBreakdownDto>()));

        // Customize StrategyConfidenceDto
        fixture.Customize<StrategyConfidenceDto>(c => c
            .With(x => x.Level, "High")
            .With(x => x.Percentage, 85)
            .With(x => x.Explanation, "Strong benchmark coverage across movements.")
            .With(x => x.MissingBenchmarks, new List<string>())
            .With(x => x.CoveredMovementCount, 3)
            .With(x => x.TotalMovementCount, 3));

        // Customize KeyFocusMovementDto
        fixture.Customize<KeyFocusMovementDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.Reason, "This is a relative weakness")
            .With(x => x.Recommendation, "Break into manageable sets.")
            .With(x => x.Priority, 1)
            .With(x => x.PacingLevel, "Light")
            .With(x => x.LoadClassification, "Moderate")
            .With(x => x.ScalingRecommended, false));

        // Customize RiskAlertDto
        fixture.Customize<RiskAlertDto>(c => c
            .With(x => x.AlertType, RiskAlertType.PacingMismatch)
            .With(x => x.Severity, AlertSeverity.Low)
            .With(x => x.Title, "Varied Movement Strengths")
            .With(x => x.Message, "This workout has a mix of movements.")
            .With(x => x.AffectedMovements, new List<string> { "Thruster", "Pull-up" })
            .With(x => x.SuggestedAction, "Pace to your weakest movement."));

        // Customize StrategyInsightsResultDto
        fixture.Customize<StrategyInsightsResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.DifficultyScore, () => fixture.Create<DifficultyScoreDto>())
            .With(x => x.StrategyConfidence, () => fixture.Create<StrategyConfidenceDto>())
            .With(x => x.KeyFocusMovements, () => fixture.CreateMany<KeyFocusMovementDto>(2).ToList())
            .With(x => x.RiskAlerts, () => fixture.CreateMany<RiskAlertDto>(1).ToList())
            .With(x => x.StrategySummary, "A balanced workout with manageable challenges.")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .Without(x => x.PacingAnalysis)
            .Without(x => x.VolumeLoadAnalysis)
            .Without(x => x.TimeEstimate));
    }
}
