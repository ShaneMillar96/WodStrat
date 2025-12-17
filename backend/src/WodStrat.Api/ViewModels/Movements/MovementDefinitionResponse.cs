namespace WodStrat.Api.ViewModels.Movements;

/// <summary>
/// Response model for movement definition data.
/// </summary>
public class MovementDefinitionResponse
{
    /// <summary>
    /// Unique identifier for the movement definition.
    /// </summary>
    /// <example>15</example>
    public int Id { get; set; }

    /// <summary>
    /// Internal identifier for the movement.
    /// </summary>
    /// <example>pull_up</example>
    public string CanonicalName { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name.
    /// </summary>
    /// <example>Pull-Up</example>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Movement category.
    /// </summary>
    /// <example>Gymnastics</example>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the movement.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of known aliases for this movement.
    /// </summary>
    /// <example>["pullup", "pull-up", "pullups"]</example>
    public IReadOnlyList<string> Aliases { get; set; } = Array.Empty<string>();
}
