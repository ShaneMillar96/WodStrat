namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Request model for parsing workout text.
/// </summary>
public class ParseWorkoutRequest
{
    /// <summary>
    /// The raw workout text to parse (e.g., "20 min AMRAP\n10 Pull-ups\n15 Push-ups\n20 Air Squats").
    /// </summary>
    /// <example>20 min AMRAP
    /// 10 Pull-ups
    /// 15 Push-ups
    /// 20 Air Squats</example>
    public string Text { get; set; } = string.Empty;
}
