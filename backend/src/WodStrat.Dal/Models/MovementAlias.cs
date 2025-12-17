namespace WodStrat.Dal.Models;

/// <summary>
/// Represents an alias or common variation for a canonical movement.
/// </summary>
public class MovementAlias
{
    /// <summary>
    /// Unique auto-incrementing identifier for the alias.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the canonical movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Alias text (e.g., "T2B", "toes-to-bar", "TTB").
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The canonical movement definition this alias maps to.
    /// </summary>
    public MovementDefinition MovementDefinition { get; set; } = null!;
}
