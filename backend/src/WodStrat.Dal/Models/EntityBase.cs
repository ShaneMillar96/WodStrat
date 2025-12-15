namespace WodStrat.Dal.Models;

/// <summary>
/// Base class for all entities providing common audit fields.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Unique auto-incrementing identifier for the entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Indicates whether this entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
