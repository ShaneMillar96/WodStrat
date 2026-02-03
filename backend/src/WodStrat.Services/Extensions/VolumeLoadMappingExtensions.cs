using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping volume load data.
/// </summary>
public static class VolumeLoadMappingExtensions
{
    /// <summary>
    /// Creates a WorkoutVolumeLoadResultDto from calculation results.
    /// </summary>
    /// <param name="workout">The workout entity.</param>
    /// <param name="movementVolumes">The list of movement volume load DTOs.</param>
    /// <returns>A new WorkoutVolumeLoadResultDto instance.</returns>
    public static WorkoutVolumeLoadResultDto ToWorkoutVolumeLoadResultDto(
        this Workout workout,
        IReadOnlyList<MovementVolumeLoadDto> movementVolumes)
    {
        var totalVolumeLoad = movementVolumes.Sum(m => m.VolumeLoad);

        var distribution = new VolumeLoadDistributionDto
        {
            HighCount = movementVolumes.Count(m => m.LoadClassification == "High"),
            ModerateCount = movementVolumes.Count(m => m.LoadClassification == "Moderate"),
            LowCount = movementVolumes.Count(m => m.LoadClassification == "Low"),
            BodyweightCount = movementVolumes.Count(m => m.LoadClassification == "Bodyweight"),
            TotalMovements = movementVolumes.Count,
            InsufficientDataCount = movementVolumes.Count(m => !m.HasSufficientData && m.LoadClassification != "Bodyweight")
        };

        var overallAssessment = GenerateOverallAssessment(movementVolumes, totalVolumeLoad);

        return new WorkoutVolumeLoadResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            MovementVolumes = movementVolumes,
            TotalVolumeLoad = totalVolumeLoad,
            TotalVolumeLoadFormatted = $"{totalVolumeLoad:N0} kg",
            OverallAssessment = overallAssessment,
            CalculatedAt = DateTime.UtcNow,
            Distribution = distribution,
            IsComplete = distribution.InsufficientDataCount == 0
        };
    }

    /// <summary>
    /// Generates an overall assessment of the workout volume load.
    /// </summary>
    /// <param name="movements">The list of movement volume load DTOs.</param>
    /// <param name="totalVolumeLoad">The total volume load for the workout.</param>
    /// <returns>Human-readable assessment string.</returns>
    private static string GenerateOverallAssessment(
        IReadOnlyList<MovementVolumeLoadDto> movements,
        decimal totalVolumeLoad)
    {
        var highCount = movements.Count(m => m.LoadClassification == "High");
        var lowCount = movements.Count(m => m.LoadClassification == "Low");
        var weightedMovements = movements.Count(m =>
            m.LoadClassification != "Bodyweight" &&
            m.LoadClassification != "N/A");

        if (weightedMovements == 0)
        {
            return "This workout has no weighted movements. Focus on movement quality and pacing.";
        }

        if (highCount > weightedMovements / 2)
        {
            return $"High volume workout with {totalVolumeLoad:N0} kg total. Consider scaling weights to maintain intensity throughout. Multiple movements are at high load relative to your benchmarks.";
        }
        else if (lowCount > weightedMovements / 2)
        {
            return $"Moderate volume workout with {totalVolumeLoad:N0} kg total. Weights are manageable relative to your strength. Push the pace on weighted movements.";
        }
        else
        {
            return $"Balanced volume workout with {totalVolumeLoad:N0} kg total. Mix of challenging and manageable loads. Pace yourself on high-load movements.";
        }
    }
}
