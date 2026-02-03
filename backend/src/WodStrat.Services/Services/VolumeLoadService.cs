using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for calculating personalized volume load analysis
/// based on athlete benchmark performance and experience level.
/// </summary>
public class VolumeLoadService : IVolumeLoadService
{
    // Load classification thresholds (percentage of 1RM)
    private const decimal HighLoadThreshold = 70m;  // >= 70% of 1RM = High
    private const decimal ModerateLoadThreshold = 50m;  // >= 50% of 1RM = Moderate
    // Below 50% = Low

    // Scaling factors based on percentile and classification
    private const decimal ScaleFactorLowPercentile = 0.80m;  // 80% of RX for low percentile + high load
    private const decimal ScaleFactorMediumPercentile = 0.90m;  // 90% of RX for medium percentile + high load

    // Percentile thresholds for scaling decisions
    private const decimal LowPercentileThreshold = 60m;
    private const decimal MediumPercentileThreshold = 80m;

    // Default round count when not specified
    private const int DefaultRoundCount = 1;

    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;

    /// <summary>
    /// Initializes a new instance of the VolumeLoadService.
    /// </summary>
    /// <param name="database">Database access interface.</param>
    /// <param name="athleteService">Athlete service for current user operations.</param>
    public VolumeLoadService(
        IWodStratDatabase database,
        IAthleteService athleteService)
    {
        _database = database;
        _athleteService = athleteService;
    }

    /// <inheritdoc />
    public async Task<WorkoutVolumeLoadResultDto?> CalculateWorkoutVolumeLoadAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Load workout with movements
        var workout = await _database.Get<Workout>()
            .Include(w => w.Movements)
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            return null;
        }

        // Determine round count (default to 1 for AMRAP/ForTime, use RoundCount for rounds-based)
        int rounds = workout.RoundCount ?? DefaultRoundCount;

        // Calculate volume load for each weighted movement
        var movementVolumeList = new List<MovementVolumeLoadDto>();

        foreach (var movement in workout.Movements.OrderBy(m => m.SequenceOrder))
        {
            var movementVolumeLoad = await CalculateMovementVolumeLoadAsync(
                athleteId,
                movement,
                rounds,
                cancellationToken);

            movementVolumeList.Add(movementVolumeLoad);
        }

        // Calculate totals and generate assessment
        return workout.ToWorkoutVolumeLoadResultDto(movementVolumeList);
    }

    /// <inheritdoc />
    public async Task<MovementVolumeLoadDto> CalculateMovementVolumeLoadAsync(
        int athleteId,
        WorkoutMovement movement,
        int rounds,
        CancellationToken cancellationToken = default)
    {
        var movementDefinition = movement.MovementDefinition;
        var weight = movement.LoadValue ?? 0m;
        var reps = movement.RepCount ?? 0;
        var unit = movement.LoadUnit?.ToString() ?? "kg";

        // Calculate volume load
        var volumeLoad = CalculateVolumeLoad(weight, reps, rounds);

        // Initialize with defaults
        var result = new MovementVolumeLoadDto
        {
            MovementDefinitionId = movementDefinition.Id,
            MovementName = movementDefinition.DisplayName,
            Weight = weight,
            WeightUnit = unit.ToLower(),
            Reps = reps,
            Rounds = rounds,
            VolumeLoad = volumeLoad,
            VolumeLoadFormatted = FormatVolumeLoad(volumeLoad, unit.ToLower()),
            LoadClassification = "N/A",
            BenchmarkUsed = "None",
            AthleteBenchmarkPercentile = null,
            Tip = string.Empty,
            RecommendedWeight = null,
            RecommendedWeightFormatted = null,
            HasSufficientData = false
        };

        // Skip classification for bodyweight movements or movements without load
        if (movementDefinition.IsBodyweight || weight <= 0)
        {
            result.LoadClassification = "Bodyweight";
            result.Tip = movementDefinition.IsBodyweight
                ? $"Focus on movement efficiency and consistent rep cadence for {movementDefinition.DisplayName}."
                : $"No load specified for {movementDefinition.DisplayName}. Check workout prescription.";
            return result;
        }

        // Find relevant benchmark mapping for this movement
        var benchmarkMapping = await _database.Get<BenchmarkMovementMapping>()
            .Include(m => m.BenchmarkDefinition)
            .Where(m => m.MovementDefinitionId == movementDefinition.Id)
            .OrderByDescending(m => m.RelevanceFactor)
            .FirstOrDefaultAsync(cancellationToken);

        if (benchmarkMapping == null)
        {
            result.Tip = $"No benchmark mapping found for {movementDefinition.DisplayName}. Record relevant benchmarks for personalized guidance.";
            return result;
        }

        result.BenchmarkUsed = benchmarkMapping.BenchmarkDefinition.Name;

        // Get athlete's benchmark value for 1RM calculation
        var athleteBenchmark = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId
                && ab.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId
                && !ab.IsDeleted)
            .OrderByDescending(ab => ab.RecordedAt)  // Use most recent benchmark
            .FirstOrDefaultAsync(cancellationToken);

        if (athleteBenchmark == null)
        {
            result.Tip = $"Record your {benchmarkMapping.BenchmarkDefinition.Name} to get personalized {movementDefinition.DisplayName} recommendations.";
            return result;
        }

        // Get athlete for experience level
        var athlete = await _database.Get<Athlete>()
            .Where(a => a.Id == athleteId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var experienceLevel = athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate;
        var athleteGender = athlete?.Gender;

        // Get population percentile data for calculating athlete's percentile
        var populationData = await _database.Get<PopulationBenchmarkPercentile>()
            .Where(p => p.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId
                && (p.Gender == null || p.Gender == athleteGender))
            .FirstOrDefaultAsync(cancellationToken);

        // Calculate athlete's percentile if population data exists
        decimal? athletePercentile = null;
        if (populationData != null)
        {
            athletePercentile = CalculatePercentileForWeight(
                athleteBenchmark.Value,
                populationData);
        }

        // Classify the load
        result.LoadClassification = ClassifyLoad(weight, athleteBenchmark.Value, experienceLevel);
        result.AthleteBenchmarkPercentile = athletePercentile;
        result.HasSufficientData = true;

        // Generate tip
        result.Tip = GenerateTip(result.LoadClassification, athletePercentile, weight, movementDefinition.DisplayName);

        // Calculate recommended weight if scaling is suggested
        result.RecommendedWeight = CalculateRecommendedWeight(weight, athletePercentile, result.LoadClassification);
        if (result.RecommendedWeight.HasValue)
        {
            var percentOfRx = result.RecommendedWeight.Value / weight * 100;
            result.RecommendedWeightFormatted = $"{result.RecommendedWeight:F0} {unit.ToLower()} ({percentOfRx:F0}% of RX)";
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<WorkoutVolumeLoadResultDto?> CalculateCurrentUserWorkoutVolumeLoadAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await CalculateWorkoutVolumeLoadAsync(athlete.Id, workoutId, cancellationToken);
    }

    /// <inheritdoc />
    public decimal CalculateVolumeLoad(decimal weight, int reps, int rounds)
    {
        if (weight <= 0 || reps <= 0 || rounds <= 0)
        {
            return 0m;
        }

        return weight * reps * rounds;
    }

    /// <inheritdoc />
    public string ClassifyLoad(decimal workoutWeight, decimal athlete1RM, ExperienceLevel experience)
    {
        if (athlete1RM <= 0)
        {
            return "Moderate"; // Default when no benchmark data
        }

        decimal percentOf1RM = (workoutWeight / athlete1RM) * 100m;

        // Adjust thresholds based on experience level
        // Beginners should be more conservative, advanced can push harder
        decimal highThreshold = experience switch
        {
            ExperienceLevel.Beginner => HighLoadThreshold - 5m,  // 65%
            ExperienceLevel.Advanced => HighLoadThreshold + 5m,  // 75%
            _ => HighLoadThreshold  // 70%
        };

        decimal moderateThreshold = experience switch
        {
            ExperienceLevel.Beginner => ModerateLoadThreshold - 5m,  // 45%
            ExperienceLevel.Advanced => ModerateLoadThreshold + 5m,  // 55%
            _ => ModerateLoadThreshold  // 50%
        };

        if (percentOf1RM >= highThreshold)
        {
            return "High";
        }
        else if (percentOf1RM >= moderateThreshold)
        {
            return "Moderate";
        }
        else
        {
            return "Low";
        }
    }

    /// <inheritdoc />
    public decimal? CalculateRecommendedWeight(decimal rxWeight, decimal? athletePercentile, string loadClassification)
    {
        // Only suggest scaling for High load when athlete is below high percentile
        if (loadClassification != "High" || !athletePercentile.HasValue)
        {
            return null;  // RX is appropriate
        }

        decimal percentile = athletePercentile.Value;

        if (percentile < LowPercentileThreshold)
        {
            // Scale down 20% for athletes below 60th percentile
            return Math.Round(rxWeight * ScaleFactorLowPercentile, 1);
        }
        else if (percentile < MediumPercentileThreshold)
        {
            // Scale down 10% for athletes between 60th-80th percentile
            return Math.Round(rxWeight * ScaleFactorMediumPercentile, 1);
        }

        return null;  // RX is appropriate for 80th+ percentile athletes
    }

    /// <inheritdoc />
    public string GenerateTip(string loadClassification, decimal? athletePercentile, decimal rxWeight, string movementName)
    {
        if (!athletePercentile.HasValue)
        {
            return $"Record your benchmark to get personalized {movementName} recommendations.";
        }

        var percentile = athletePercentile.Value;
        var recommendedWeight = CalculateRecommendedWeight(rxWeight, athletePercentile, loadClassification);

        return loadClassification switch
        {
            "High" when recommendedWeight.HasValue =>
                $"Consider scaling {movementName} to {recommendedWeight:F0} kg ({(percentile < LowPercentileThreshold ? "80" : "90")}% of RX) to maintain movement quality and intensity throughout the workout.",

            "High" when percentile >= MediumPercentileThreshold =>
                $"This is a heavy load for {movementName}, but you're above the 80th percentile. Go RX but manage your rest strategically.",

            "Moderate" =>
                $"This {movementName} weight is moderate relative to your strength. Focus on consistent pacing and efficient movement.",

            "Low" =>
                $"This is a light load for {movementName}. You can push the pace here and aim for larger unbroken sets.",

            _ => $"Maintain good form on {movementName} throughout the workout."
        };
    }

    /// <inheritdoc />
    public string FormatVolumeLoad(decimal volumeLoad, string unit)
    {
        return $"{volumeLoad:N0} {unit}";  // Uses culture-specific thousands separator
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates athlete's percentile for weight-based benchmarks (higher is better).
    /// </summary>
    /// <param name="athleteValue">The athlete's benchmark value.</param>
    /// <param name="populationData">Population percentile reference data.</param>
    /// <returns>Percentile value (0-100).</returns>
    private static decimal CalculatePercentileForWeight(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData)
    {
        var brackets = new (decimal percentile, decimal value)[]
        {
            (20m, populationData.Percentile20),
            (40m, populationData.Percentile40),
            (60m, populationData.Percentile60),
            (80m, populationData.Percentile80),
            (95m, populationData.Percentile95)
        };

        // For weight benchmarks, higher is better
        // Check if athlete is better than the best bracket
        if (athleteValue >= brackets[4].value)
        {
            var extraRange = brackets[4].value - brackets[3].value;
            if (extraRange <= 0) return 95m;
            return Math.Min(100m, 95m + (5m * (athleteValue - brackets[4].value) / extraRange));
        }

        // Check if athlete is worse than the lowest bracket
        if (athleteValue <= brackets[0].value)
        {
            if (brackets[0].value <= 0) return 0m;
            return Math.Max(0m, 20m * athleteValue / brackets[0].value);
        }

        // Find which bracket the athlete falls into and interpolate
        for (int i = 0; i < brackets.Length - 1; i++)
        {
            var lowerBracket = brackets[i];
            var higherBracket = brackets[i + 1];

            if (athleteValue >= lowerBracket.value && athleteValue <= higherBracket.value)
            {
                var range = higherBracket.value - lowerBracket.value;
                if (range <= 0) return lowerBracket.percentile;

                var position = athleteValue - lowerBracket.value;
                var percentileRange = higherBracket.percentile - lowerBracket.percentile;

                return lowerBracket.percentile + (percentileRange * position / range);
            }
        }

        return 50m;  // Fallback
    }

    #endregion
}
