namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for movement definition responses.
/// </summary>
public class MovementDefinitionDto
{
    /// <summary>
    /// Unique identifier for the movement definition.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Internal identifier for the movement (e.g., "toes_to_bar").
    /// </summary>
    public string CanonicalName { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name (e.g., "Toes-to-Bar").
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Movement category (Weightlifting/Gymnastics/Cardio/Strongman).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the movement.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of known aliases for this movement.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; set; } = Array.Empty<string>();
}
