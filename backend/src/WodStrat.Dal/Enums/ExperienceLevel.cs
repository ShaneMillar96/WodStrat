using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Represents the athlete's experience level in functional fitness.
/// </summary>
public enum ExperienceLevel
{
    /// <summary>
    /// Less than 1 year of functional fitness experience.
    /// </summary>
    [Description("Less than 1 year of functional fitness")]
    [PgName("beginner")]
    Beginner,

    /// <summary>
    /// 1-3 years of functional fitness experience.
    /// </summary>
    [Description("1-3 years of experience")]
    [PgName("intermediate")]
    Intermediate,

    /// <summary>
    /// 3+ years of experience with competition background.
    /// </summary>
    [Description("3+ years, competition experience")]
    [PgName("advanced")]
    Advanced
}
