namespace WodStrat.Services.Dtos;

/// <summary>
/// Difficulty score labels and descriptions.
/// </summary>
public static class DifficultyLabel
{
    /// <summary>
    /// Gets the label for a difficulty score.
    /// </summary>
    /// <param name="score">The difficulty score (1-10).</param>
    /// <returns>Human-readable difficulty label.</returns>
    public static string GetLabel(int score) => score switch
    {
        <= 2 => "Very Easy",
        <= 4 => "Easy",
        <= 6 => "Moderate",
        <= 8 => "Hard",
        _ => "Very Hard"
    };

    /// <summary>
    /// Gets the description for a difficulty score.
    /// </summary>
    /// <param name="score">The difficulty score (1-10).</param>
    /// <returns>Detailed description of the difficulty level.</returns>
    public static string GetDescription(int score) => score switch
    {
        <= 2 => "This workout plays to your strengths. Push the pace and aim for a PR.",
        <= 4 => "Manageable workout with room to push. Focus on consistent effort.",
        <= 6 => "Balanced challenge. Pace yourself and stay mentally engaged.",
        <= 8 => "Demanding workout. Strategic breaks and pacing are essential.",
        _ => "Extremely challenging. Consider scaling and prioritize completion over pace."
    };
}
