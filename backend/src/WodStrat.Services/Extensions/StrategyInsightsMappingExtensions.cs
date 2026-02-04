using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping strategy insights data.
/// </summary>
public static class StrategyInsightsMappingExtensions
{
    /// <summary>
    /// Gets the experience modifier for difficulty calculation.
    /// </summary>
    /// <param name="level">The athlete's experience level.</param>
    /// <returns>Modifier value (Beginner=1.2, Intermediate=1.0, Advanced=0.85).</returns>
    public static decimal GetExperienceModifier(this ExperienceLevel level) => level switch
    {
        ExperienceLevel.Beginner => 1.2m,
        ExperienceLevel.Intermediate => 1.0m,
        ExperienceLevel.Advanced => 0.85m,
        _ => 1.0m
    };

    /// <summary>
    /// Converts pacing level string to difficulty points.
    /// Light (weakness) = 8 points, Moderate = 5, Heavy (strength) = 2.
    /// </summary>
    /// <param name="pacingLevel">The pacing level string.</param>
    /// <returns>Difficulty points (0-10 scale).</returns>
    public static decimal ToPacingDifficultyPoints(this string pacingLevel) => pacingLevel switch
    {
        "Light" => 8m,
        "Moderate" => 5m,
        "Heavy" => 2m,
        _ => 5m
    };

    /// <summary>
    /// Converts load classification string to difficulty points.
    /// High = 8 points, Moderate = 5, Low = 2, Bodyweight = 4.
    /// </summary>
    /// <param name="loadClassification">The load classification string.</param>
    /// <returns>Difficulty points (0-10 scale).</returns>
    public static decimal ToVolumeDifficultyPoints(this string loadClassification) => loadClassification switch
    {
        "High" => 8m,
        "Moderate" => 5m,
        "Low" => 2m,
        "Bodyweight" => 4m,
        "N/A" => 5m,
        _ => 5m
    };

    /// <summary>
    /// Calculates time factor difficulty points based on workout duration.
    /// </summary>
    /// <param name="estimatedMaxSeconds">Maximum estimated time in seconds.</param>
    /// <param name="workoutType">Type of workout (ForTime, AMRAP, etc.).</param>
    /// <returns>Time factor difficulty points (0-10 scale).</returns>
    public static decimal CalculateTimeFactor(int estimatedMaxSeconds, string workoutType)
    {
        // Convert to minutes for easier reasoning
        var minutes = estimatedMaxSeconds / 60.0;

        return workoutType switch
        {
            "ForTime" or "Rounds" => minutes switch
            {
                < 10 => 4m,   // Short workout
                < 15 => 6m,   // Medium workout
                < 20 => 7m,   // Longer workout
                < 30 => 8m,   // Long workout
                _ => 9m       // Very long workout
            },
            "Amrap" => 5m,    // Fixed time - moderate by default
            "Emom" => 5m,     // Fixed time - moderate by default
            "Intervals" => 5m, // Fixed time - moderate by default
            "Tabata" => 6m,   // High intensity interval - slightly harder
            _ => 5m           // Default moderate
        };
    }

    /// <summary>
    /// Generates a strategy summary based on difficulty and key factors.
    /// </summary>
    /// <param name="difficultyScore">The calculated difficulty score (1-10).</param>
    /// <param name="focusMovements">List of key focus movements.</param>
    /// <param name="riskAlerts">List of risk alerts.</param>
    /// <param name="workoutType">Type of workout.</param>
    /// <returns>Human-readable strategy summary text.</returns>
    public static string GenerateStrategySummary(
        int difficultyScore,
        IReadOnlyList<KeyFocusMovementDto> focusMovements,
        IReadOnlyList<RiskAlertDto> riskAlerts,
        string workoutType)
    {
        var summaryParts = new List<string>();

        // Difficulty-based opener
        var opener = difficultyScore switch
        {
            <= 3 => "This workout favors your strengths.",
            <= 5 => "A balanced workout with manageable challenges.",
            <= 7 => "This workout will test you.",
            _ => "Expect a significant challenge."
        };
        summaryParts.Add(opener);

        // Focus movement advice
        if (focusMovements.Count > 0)
        {
            var topFocus = focusMovements[0];
            summaryParts.Add($"Pay special attention to {topFocus.MovementName}: {topFocus.Recommendation}");
        }

        // Risk-based advice
        if (riskAlerts.Any(a => a.AlertType == RiskAlertType.ScalingRecommended))
        {
            summaryParts.Add("Consider scaling weights to maintain intensity throughout.");
        }
        else if (riskAlerts.Any(a => a.AlertType == RiskAlertType.TimeCapRisk))
        {
            summaryParts.Add("Watch your pace to avoid the time cap.");
        }

        return string.Join(" ", summaryParts);
    }

    /// <summary>
    /// Parses experience level string to enum.
    /// </summary>
    /// <param name="experienceLevel">Experience level string.</param>
    /// <returns>ExperienceLevel enum value.</returns>
    public static ExperienceLevel ParseExperienceLevel(string experienceLevel)
    {
        return experienceLevel.ToLowerInvariant() switch
        {
            "beginner" => ExperienceLevel.Beginner,
            "intermediate" => ExperienceLevel.Intermediate,
            "advanced" => ExperienceLevel.Advanced,
            _ => ExperienceLevel.Intermediate
        };
    }
}
