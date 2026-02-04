using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for calculating comprehensive workout strategy insights
/// by combining pacing, volume load, and time estimate analyses.
/// </summary>
public class StrategyInsightsService : IStrategyInsightsService
{
    // Weights for difficulty score calculation
    private const decimal PacingWeight = 0.4m;
    private const decimal VolumeWeight = 0.3m;
    private const decimal TimeWeight = 0.3m;

    // Thresholds for risk alerts
    private const int ScalingRecommendedMovementThreshold = 2;
    private const decimal TimeCapRiskPercentage = 0.90m;
    private const int HighDifficultyThreshold = 8;
    private const int PacingMismatchThreshold = 2;
    private const decimal KeyMovementVolumeThreshold = 0.25m;

    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly IPacingService _pacingService;
    private readonly IVolumeLoadService _volumeLoadService;
    private readonly ITimeEstimateService _timeEstimateService;

    /// <summary>
    /// Initializes a new instance of the StrategyInsightsService.
    /// </summary>
    public StrategyInsightsService(
        IWodStratDatabase database,
        IAthleteService athleteService,
        IPacingService pacingService,
        IVolumeLoadService volumeLoadService,
        ITimeEstimateService timeEstimateService)
    {
        _database = database;
        _athleteService = athleteService;
        _pacingService = pacingService;
        _volumeLoadService = volumeLoadService;
        _timeEstimateService = timeEstimateService;
    }

    /// <inheritdoc />
    public async Task<StrategyInsightsResultDto?> CalculateStrategyInsightsAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Execute dependent service calls sequentially to avoid DbContext concurrency issues
        // Note: These services share the same scoped DbContext, so parallel execution is not supported
        var pacingResult = await _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, cancellationToken);
        var volumeResult = await _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, cancellationToken);
        var timeResult = await _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, cancellationToken);

        // If any result is null, the workout doesn't exist or data is missing
        if (pacingResult == null || volumeResult == null || timeResult == null)
        {
            return null;
        }

        // Get athlete information for experience level
        var athlete = await _athleteService.GetByIdAsync(athleteId, cancellationToken);
        var experienceLevel = athlete?.ExperienceLevel ?? "Intermediate";

        // Get workout for time cap information
        var workout = await _database.Get<Workout>()
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var timeCapSeconds = workout?.TimeCapSeconds;

        // Calculate all insight components
        var difficultyScore = CalculateDifficultyScore(pacingResult, volumeResult, timeResult, experienceLevel);
        var keyFocusMovements = IdentifyKeyFocusMovements(pacingResult, volumeResult);
        var riskAlerts = GenerateRiskAlerts(pacingResult, volumeResult, timeResult, difficultyScore.Score, timeCapSeconds);
        var strategyConfidence = CalculateStrategyConfidence(pacingResult, volumeResult, timeResult);

        // Generate strategy summary
        var strategySummary = StrategyInsightsMappingExtensions.GenerateStrategySummary(
            difficultyScore.Score,
            keyFocusMovements,
            riskAlerts,
            pacingResult.WorkoutType);

        return new StrategyInsightsResultDto
        {
            WorkoutId = workoutId,
            WorkoutName = pacingResult.WorkoutName,
            WorkoutType = pacingResult.WorkoutType,
            DifficultyScore = difficultyScore,
            StrategyConfidence = strategyConfidence,
            KeyFocusMovements = keyFocusMovements,
            RiskAlerts = riskAlerts,
            StrategySummary = strategySummary,
            CalculatedAt = DateTime.UtcNow,
            PacingAnalysis = pacingResult,
            VolumeLoadAnalysis = volumeResult,
            TimeEstimate = timeResult
        };
    }

    /// <inheritdoc />
    public async Task<StrategyInsightsResultDto?> CalculateCurrentUserStrategyInsightsAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await CalculateStrategyInsightsAsync(athlete.Id, workoutId, cancellationToken);
    }

    /// <inheritdoc />
    public DifficultyScoreDto CalculateDifficultyScore(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult,
        string experienceLevel)
    {
        // 1. Calculate Pacing Factor (weighted average of movement pacing difficulty)
        var pacingFactor = CalculatePacingFactor(pacingResult);

        // 2. Calculate Volume Factor (based on load classifications)
        var volumeFactor = CalculateVolumeFactor(volumeResult);

        // 3. Calculate Time Factor (based on estimated duration)
        var timeFactor = StrategyInsightsMappingExtensions.CalculateTimeFactor(
            timeResult.MaxEstimate,
            timeResult.WorkoutType);

        // 4. Calculate base score
        var baseScore = (pacingFactor * PacingWeight) + (volumeFactor * VolumeWeight) + (timeFactor * TimeWeight);

        // 5. Apply experience modifier
        var expLevel = StrategyInsightsMappingExtensions.ParseExperienceLevel(experienceLevel);
        var experienceModifier = expLevel.GetExperienceModifier();

        // 6. Calculate final score (clamped 1-10)
        var finalScoreRaw = baseScore * experienceModifier;
        var finalScore = (int)Math.Clamp(Math.Round(finalScoreRaw), 1, 10);

        // Generate explanation
        var explanation = GenerateDifficultyExplanation(pacingFactor, volumeFactor, timeFactor, experienceModifier);

        return new DifficultyScoreDto
        {
            Score = finalScore,
            Label = DifficultyLabel.GetLabel(finalScore),
            Description = DifficultyLabel.GetDescription(finalScore),
            Breakdown = new DifficultyBreakdownDto
            {
                PacingFactor = Math.Round(pacingFactor, 2),
                VolumeFactor = Math.Round(volumeFactor, 2),
                TimeFactor = Math.Round(timeFactor, 2),
                ExperienceModifier = experienceModifier,
                BaseScore = Math.Round(baseScore, 2),
                Explanation = explanation
            }
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<KeyFocusMovementDto> IdentifyKeyFocusMovements(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        int maxMovements = 3)
    {
        var focusMovements = new List<KeyFocusMovementDto>();

        // Create lookup for volume data (handle duplicates by taking first occurrence)
        var volumeLookup = volumeResult.MovementVolumes
            .GroupBy(v => v.MovementDefinitionId)
            .ToDictionary(g => g.Key, g => g.First());

        // Score each movement based on pacing (Light = weakness) and volume load
        var scoredMovements = new List<(MovementPacingDto pacing, MovementVolumeLoadDto? volume, int score)>();

        foreach (var pacing in pacingResult.MovementPacing)
        {
            volumeLookup.TryGetValue(pacing.MovementDefinitionId, out var volume);

            var score = 0;

            // Light pacing (weakness) scores higher for focus
            if (pacing.PacingLevel == "Light")
            {
                score += 3;
            }
            else if (pacing.PacingLevel == "Moderate")
            {
                score += 1;
            }

            // High volume load adds to focus score
            if (volume != null)
            {
                if (volume.LoadClassification == "High")
                {
                    score += 3;
                }
                else if (volume.LoadClassification == "Moderate")
                {
                    score += 1;
                }
            }

            // Missing benchmark data is notable
            if (!pacing.HasAthleteBenchmark || !pacing.HasPopulationData)
            {
                score += 1;
            }

            scoredMovements.Add((pacing, volume, score));
        }

        // Sort by score (highest first) and take top movements
        var topMovements = scoredMovements
            .OrderByDescending(m => m.score)
            .Take(maxMovements)
            .ToList();

        var priority = 1;
        foreach (var (pacing, volume, score) in topMovements)
        {
            if (score <= 0) continue; // Skip movements with no notable factors

            var reason = GenerateFocusReason(pacing, volume);
            var recommendation = GenerateFocusRecommendation(pacing, volume);
            var scalingRecommended = pacing.PacingLevel == "Light" &&
                (volume?.LoadClassification == "High" || volume?.LoadClassification == "Moderate");

            focusMovements.Add(new KeyFocusMovementDto
            {
                MovementDefinitionId = pacing.MovementDefinitionId,
                MovementName = pacing.MovementName,
                Reason = reason,
                Recommendation = recommendation,
                Priority = priority++,
                PacingLevel = pacing.PacingLevel,
                LoadClassification = volume?.LoadClassification ?? "N/A",
                ScalingRecommended = scalingRecommended
            });
        }

        return focusMovements;
    }

    /// <inheritdoc />
    public IReadOnlyList<RiskAlertDto> GenerateRiskAlerts(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult,
        int difficultyScore,
        int? timeCapSeconds)
    {
        var alerts = new List<RiskAlertDto>();

        // Rule 1: ScalingRecommended - High volume + Light pacing on 2+ movements
        var scalingCandidates = GetScalingCandidates(pacingResult, volumeResult);
        if (scalingCandidates.Count >= ScalingRecommendedMovementThreshold)
        {
            alerts.Add(new RiskAlertDto
            {
                AlertType = RiskAlertType.ScalingRecommended,
                Severity = AlertSeverity.Medium,
                Title = "Consider Scaling",
                Message = $"Found {scalingCandidates.Count} movements where you have Light pacing (weakness) combined with significant volume. Consider scaling weights to maintain intensity.",
                AffectedMovements = scalingCandidates,
                SuggestedAction = "Scale weights down 10-20% on these movements to maintain workout intensity and movement quality."
            });
        }

        // Rule 2: TimeCapRisk - Estimated max time > 90% of time cap (ForTime workouts)
        if (timeCapSeconds.HasValue && timeResult.EstimateType == "Time")
        {
            var threshold = (int)(timeCapSeconds.Value * TimeCapRiskPercentage);
            if (timeResult.MaxEstimate > threshold)
            {
                alerts.Add(new RiskAlertDto
                {
                    AlertType = RiskAlertType.TimeCapRisk,
                    Severity = AlertSeverity.High,
                    Title = "Time Cap Risk",
                    Message = $"Your estimated finish time ({FormatTime(timeResult.MaxEstimate)}) is approaching the time cap ({FormatTime(timeCapSeconds.Value)}). You may not finish the workout as prescribed.",
                    AffectedMovements = Array.Empty<string>(),
                    SuggestedAction = "Consider scaling the workout or adjusting your pacing strategy to ensure completion."
                });
            }
        }

        // Rule 3: RecoveryImpact - Difficulty >= 8 AND High total volume
        var hasHighVolume = volumeResult.MovementVolumes.Any(v => v.LoadClassification == "High");
        if (difficultyScore >= HighDifficultyThreshold && hasHighVolume)
        {
            alerts.Add(new RiskAlertDto
            {
                AlertType = RiskAlertType.RecoveryImpact,
                Severity = AlertSeverity.Low,
                Title = "Recovery Impact",
                Message = "This is a demanding workout with high volume loads. Expect extended recovery time.",
                AffectedMovements = volumeResult.MovementVolumes
                    .Where(v => v.LoadClassification == "High")
                    .Select(v => v.MovementName)
                    .ToList(),
                SuggestedAction = "Plan for adequate rest and recovery. Consider reducing training intensity in the following days."
            });
        }

        // Rule 4: PacingMismatch - Mix of Heavy (2+) and Light (2+) pacing
        var heavyCount = pacingResult.MovementPacing.Count(p => p.PacingLevel == "Heavy");
        var lightCount = pacingResult.MovementPacing.Count(p => p.PacingLevel == "Light");
        if (heavyCount >= PacingMismatchThreshold && lightCount >= PacingMismatchThreshold)
        {
            var heavyMovements = pacingResult.MovementPacing.Where(p => p.PacingLevel == "Heavy").Select(p => p.MovementName);
            var lightMovements = pacingResult.MovementPacing.Where(p => p.PacingLevel == "Light").Select(p => p.MovementName);

            alerts.Add(new RiskAlertDto
            {
                AlertType = RiskAlertType.PacingMismatch,
                Severity = AlertSeverity.Low,
                Title = "Varied Movement Strengths",
                Message = $"This workout has a mix of movements where you excel ({string.Join(", ", heavyMovements)}) and movements that challenge you ({string.Join(", ", lightMovements)}). Strategy adjustments recommended.",
                AffectedMovements = lightMovements.Concat(heavyMovements).ToList(),
                SuggestedAction = "Push hard on your strength movements while being conservative on weaker movements. Use rest strategically."
            });
        }

        // Rule 5: BenchmarkGap - Key movement (>25% of workout volume) has no benchmark mapping
        var movementsWithoutBenchmarks = GetMovementsWithBenchmarkGaps(pacingResult, volumeResult);
        if (movementsWithoutBenchmarks.Count > 0)
        {
            alerts.Add(new RiskAlertDto
            {
                AlertType = RiskAlertType.BenchmarkGap,
                Severity = AlertSeverity.Low,
                Title = "Missing Benchmark Data",
                Message = "Some movements lack benchmark data, which affects the accuracy of these recommendations.",
                AffectedMovements = movementsWithoutBenchmarks,
                SuggestedAction = "Record benchmarks for these movements to improve future strategy recommendations."
            });
        }

        // Sort by severity (High first)
        return alerts
            .OrderBy(a => a.Severity == AlertSeverity.High ? 0 : a.Severity == AlertSeverity.Medium ? 1 : 2)
            .ToList();
    }

    /// <inheritdoc />
    public StrategyConfidenceDto CalculateStrategyConfidence(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult)
    {
        // Count movements with and without benchmark coverage
        var totalMovements = pacingResult.MovementPacing.Count;
        var coveredMovements = pacingResult.MovementPacing.Count(p => p.HasAthleteBenchmark && p.HasPopulationData);

        // Get list of missing benchmarks
        var missingBenchmarks = pacingResult.MovementPacing
            .Where(p => !p.HasAthleteBenchmark || !p.HasPopulationData)
            .Select(p => p.MovementName)
            .ToList();

        // Calculate coverage percentage
        var coveragePercent = totalMovements > 0
            ? (int)Math.Round((decimal)coveredMovements / totalMovements * 100)
            : 0;

        // Determine confidence level
        var (level, explanation) = DetermineConfidenceLevel(coveragePercent, timeResult);

        return new StrategyConfidenceDto
        {
            Level = level,
            Percentage = coveragePercent,
            Explanation = explanation,
            MissingBenchmarks = missingBenchmarks,
            CoveredMovementCount = coveredMovements,
            TotalMovementCount = totalMovements
        };
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates the pacing factor based on movement pacing levels.
    /// </summary>
    private static decimal CalculatePacingFactor(WorkoutPacingResultDto pacingResult)
    {
        if (pacingResult.MovementPacing.Count == 0)
        {
            return 5m; // Default moderate
        }

        var totalPoints = pacingResult.MovementPacing
            .Sum(p => p.PacingLevel.ToPacingDifficultyPoints());

        return totalPoints / pacingResult.MovementPacing.Count;
    }

    /// <summary>
    /// Calculates the volume factor based on load classifications.
    /// </summary>
    private static decimal CalculateVolumeFactor(WorkoutVolumeLoadResultDto volumeResult)
    {
        if (volumeResult.MovementVolumes.Count == 0)
        {
            return 5m; // Default moderate
        }

        var totalPoints = volumeResult.MovementVolumes
            .Sum(v => v.LoadClassification.ToVolumeDifficultyPoints());

        return totalPoints / volumeResult.MovementVolumes.Count;
    }

    /// <summary>
    /// Generates explanation text for difficulty breakdown.
    /// </summary>
    private static string GenerateDifficultyExplanation(
        decimal pacingFactor,
        decimal volumeFactor,
        decimal timeFactor,
        decimal experienceModifier)
    {
        var parts = new List<string>();

        if (pacingFactor >= 7)
        {
            parts.Add("Many movements target your weaknesses");
        }
        else if (pacingFactor <= 3)
        {
            parts.Add("Most movements play to your strengths");
        }

        if (volumeFactor >= 7)
        {
            parts.Add("heavy relative loads");
        }
        else if (volumeFactor <= 3)
        {
            parts.Add("manageable loads");
        }

        if (timeFactor >= 7)
        {
            parts.Add("longer duration workout");
        }
        else if (timeFactor <= 3)
        {
            parts.Add("shorter workout");
        }

        var modifierNote = experienceModifier switch
        {
            > 1m => "adjusted up for beginner experience",
            < 1m => "adjusted down for advanced experience",
            _ => ""
        };

        var baseExplanation = parts.Count > 0
            ? $"Based on {string.Join(", ", parts)}"
            : "Based on balanced workout characteristics";

        return !string.IsNullOrEmpty(modifierNote)
            ? $"{baseExplanation}, {modifierNote}."
            : $"{baseExplanation}.";
    }

    /// <summary>
    /// Generates the reason text for a focus movement.
    /// </summary>
    private static string GenerateFocusReason(MovementPacingDto pacing, MovementVolumeLoadDto? volume)
    {
        var reasons = new List<string>();

        if (pacing.PacingLevel == "Light")
        {
            reasons.Add("this is a relative weakness");
        }

        if (volume?.LoadClassification == "High")
        {
            reasons.Add("high load relative to your 1RM");
        }

        if (!pacing.HasAthleteBenchmark)
        {
            reasons.Add("no benchmark data available");
        }

        return reasons.Count > 0
            ? string.Join("; ", reasons).First().ToString().ToUpper() + string.Join("; ", reasons).Substring(1)
            : "Requires strategic attention.";
    }

    /// <summary>
    /// Generates the recommendation text for a focus movement.
    /// </summary>
    private static string GenerateFocusRecommendation(MovementPacingDto pacing, MovementVolumeLoadDto? volume)
    {
        if (pacing.PacingLevel == "Light" && (volume?.LoadClassification == "High" || volume?.LoadClassification == "Moderate"))
        {
            return "Consider scaling weight and break into smaller sets to maintain quality.";
        }

        if (pacing.PacingLevel == "Light")
        {
            return "Break into manageable sets and protect your energy for other movements.";
        }

        if (volume?.LoadClassification == "High")
        {
            return "Manage your effort carefully with strategic rest between sets.";
        }

        return "Stay focused and maintain consistent effort throughout.";
    }

    /// <summary>
    /// Gets movements that are candidates for scaling.
    /// </summary>
    private static List<string> GetScalingCandidates(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult)
    {
        var volumeLookup = volumeResult.MovementVolumes
            .GroupBy(v => v.MovementDefinitionId)
            .ToDictionary(g => g.Key, g => g.First());
        var candidates = new List<string>();

        foreach (var pacing in pacingResult.MovementPacing)
        {
            if (pacing.PacingLevel != "Light") continue;

            if (volumeLookup.TryGetValue(pacing.MovementDefinitionId, out var volume))
            {
                if (volume.LoadClassification == "High" || volume.LoadClassification == "Moderate")
                {
                    candidates.Add(pacing.MovementName);
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Gets movements with benchmark gaps.
    /// </summary>
    private static List<string> GetMovementsWithBenchmarkGaps(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult)
    {
        var movements = new List<string>();

        // Check pacing results for missing athlete benchmarks
        foreach (var pacing in pacingResult.MovementPacing)
        {
            if (!pacing.HasAthleteBenchmark || !pacing.HasPopulationData)
            {
                movements.Add(pacing.MovementName);
            }
        }

        // Also check volume results for movements without sufficient data
        foreach (var volume in volumeResult.MovementVolumes)
        {
            if (!volume.HasSufficientData && !movements.Contains(volume.MovementName))
            {
                movements.Add(volume.MovementName);
            }
        }

        return movements.Distinct().ToList();
    }

    /// <summary>
    /// Determines confidence level based on coverage percentage.
    /// </summary>
    private static (string level, string explanation) DetermineConfidenceLevel(
        int coveragePercent,
        TimeEstimateResultDto timeResult)
    {
        // Also consider time estimate confidence
        var timeConfidence = timeResult.ConfidenceLevel;

        if (coveragePercent >= 80 && timeConfidence == "High")
        {
            return ("High", "Strong benchmark coverage across movements. Recommendations are highly personalized.");
        }

        if (coveragePercent >= 80)
        {
            return ("High", $"Strong benchmark coverage ({coveragePercent}%). Some time estimate uncertainty exists.");
        }

        if (coveragePercent >= 50)
        {
            return ("Medium", $"Partial benchmark coverage ({coveragePercent}%). Recommendations are moderately personalized with some default assumptions.");
        }

        return ("Low", $"Limited benchmark coverage ({coveragePercent}%). Record more benchmarks for better personalization.");
    }

    /// <summary>
    /// Formats seconds as MM:SS string.
    /// </summary>
    private static string FormatTime(int totalSeconds)
    {
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }

    #endregion
}
