using System.ComponentModel;

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
    Beginner,

    /// <summary>
    /// 1-3 years of functional fitness experience.
    /// </summary>
    [Description("1-3 years of experience")]
    Intermediate,

    /// <summary>
    /// 3+ years of experience with competition background.
    /// </summary>
    [Description("3+ years, competition experience")]
    Advanced
}
