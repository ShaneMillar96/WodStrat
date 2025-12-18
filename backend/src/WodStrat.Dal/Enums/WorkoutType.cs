using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Classification of workout structure/format.
/// </summary>
public enum WorkoutType
{
    /// <summary>
    /// As Many Rounds As Possible - timed workout to complete maximum rounds.
    /// </summary>
    [Description("As Many Rounds As Possible")]
    [PgName("amrap")]
    Amrap,

    /// <summary>
    /// For Time - complete work as fast as possible.
    /// </summary>
    [Description("For Time")]
    [PgName("for_time")]
    ForTime,

    /// <summary>
    /// Every Minute On the Minute - interval-based with work each minute.
    /// </summary>
    [Description("Every Minute On the Minute")]
    [PgName("emom")]
    Emom,

    /// <summary>
    /// Interval training with work/rest periods.
    /// </summary>
    [Description("Interval training")]
    [PgName("intervals")]
    Intervals,

    /// <summary>
    /// Fixed rounds completed for quality (not timed).
    /// </summary>
    [Description("Fixed rounds for quality")]
    [PgName("rounds")]
    Rounds,

    /// <summary>
    /// Tabata protocol - 20 seconds work, 10 seconds rest, 8 rounds.
    /// </summary>
    [Description("Tabata protocol")]
    [PgName("tabata")]
    Tabata
}
