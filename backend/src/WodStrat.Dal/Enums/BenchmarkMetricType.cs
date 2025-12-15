using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Metric type defining how benchmark results are measured.
/// </summary>
public enum BenchmarkMetricType
{
    /// <summary>
    /// Duration in seconds (lower is better).
    /// </summary>
    [Description("Duration in seconds")]
    [PgName("time")]
    Time,

    /// <summary>
    /// Count of repetitions (higher is better).
    /// </summary>
    [Description("Count of repetitions")]
    [PgName("reps")]
    Reps,

    /// <summary>
    /// Weight in kilograms (higher is better).
    /// </summary>
    [Description("Weight in kilograms")]
    [PgName("weight")]
    Weight,

    /// <summary>
    /// Pace in seconds per unit (lower is better).
    /// </summary>
    [Description("Pace in seconds per unit")]
    [PgName("pace")]
    Pace
}
