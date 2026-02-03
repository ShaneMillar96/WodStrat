using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Utilities;

/// <summary>
/// Generates overall workout strategy notes based on pacing analysis.
/// </summary>
public static class WorkoutStrategyGenerator
{
    /// <summary>
    /// Generates strategy notes for a workout based on movement pacing distribution.
    /// </summary>
    /// <param name="workoutType">The type of workout.</param>
    /// <param name="movementPacing">The list of movement pacing recommendations.</param>
    /// <returns>A string containing overall strategy notes.</returns>
    public static string GenerateStrategyNotes(
        WorkoutType workoutType,
        IReadOnlyList<MovementPacingDto> movementPacing)
    {
        if (movementPacing.Count == 0)
        {
            return "No movements to analyze.";
        }

        var heavyCount = movementPacing.Count(m => m.PacingLevel == "Heavy");
        var lightCount = movementPacing.Count(m => m.PacingLevel == "Light");
        var total = movementPacing.Count;

        var notes = new List<string>();

        // Overall workout type strategy
        notes.Add(GetWorkoutTypeStrategy(workoutType));

        // Distribution-based recommendations
        if (heavyCount == total)
        {
            notes.Add("All movements are relative strengths - push the pace throughout.");
        }
        else if (lightCount == total)
        {
            notes.Add("Focus on consistency - break early and often to maintain steady output.");
        }
        else if (heavyCount > lightCount)
        {
            notes.Add("Leverage your strengths on heavy-paced movements to build time/rep cushion.");
        }
        else
        {
            notes.Add("Manage your limiters carefully - don't let light-paced movements derail your workout.");
        }

        // Specific movement callouts
        var limiters = movementPacing.Where(m => m.PacingLevel == "Light").ToList();
        if (limiters.Any())
        {
            var limiterNames = string.Join(", ", limiters.Select(m => m.MovementName));
            notes.Add($"Key limiters to manage: {limiterNames}");
        }

        return string.Join(" ", notes);
    }

    /// <summary>
    /// Gets a strategy message based on the workout type.
    /// </summary>
    private static string GetWorkoutTypeStrategy(WorkoutType workoutType) => workoutType switch
    {
        WorkoutType.Amrap => "For this AMRAP, maintain sustainable intensity across rounds.",
        WorkoutType.ForTime => "For this For Time workout, balance speed with smart pacing to avoid early burnout.",
        WorkoutType.Emom => "For this EMOM, ensure you complete work with adequate rest each minute.",
        WorkoutType.Intervals => "For this interval workout, push hard during work periods and use rest effectively.",
        WorkoutType.Rounds => "For this rounds-based workout, aim for consistent round times.",
        _ => "Pace yourself according to the movements below."
    };
}
