namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for movement alias information.
/// Provides detailed alias metadata for API responses.
/// </summary>
public class MovementAliasDto
{
    /// <summary>
    /// Unique identifier for the alias.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// The alias text (e.g., "T2B", "TTB", "toes-to-bar").
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the alias was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
