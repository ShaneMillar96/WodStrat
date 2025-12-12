using System.ComponentModel;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Category classification for benchmark types.
/// </summary>
public enum BenchmarkCategory
{
    /// <summary>
    /// Cardiovascular/endurance benchmarks.
    /// </summary>
    [Description("Cardiovascular/endurance benchmarks")]
    Cardio,

    /// <summary>
    /// Strength-based benchmarks (lifts).
    /// </summary>
    [Description("Strength-based benchmarks")]
    Strength,

    /// <summary>
    /// Bodyweight/gymnastics movements.
    /// </summary>
    [Description("Bodyweight/gymnastics movements")]
    Gymnastics,

    /// <summary>
    /// Hero and Girl benchmark workouts.
    /// </summary>
    [Description("Hero and Girl benchmark workouts")]
    HeroWod
}
