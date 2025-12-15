using System.ComponentModel;
using NpgsqlTypes;

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
    [PgName("cardio")]
    Cardio,

    /// <summary>
    /// Strength-based benchmarks (lifts).
    /// </summary>
    [Description("Strength-based benchmarks")]
    [PgName("strength")]
    Strength,

    /// <summary>
    /// Bodyweight/gymnastics movements.
    /// </summary>
    [Description("Bodyweight/gymnastics movements")]
    [PgName("gymnastics")]
    Gymnastics,

    /// <summary>
    /// Hero and Girl benchmark workouts.
    /// </summary>
    [Description("Hero and Girl benchmark workouts")]
    [PgName("hero_wod")]
    HeroWod
}
