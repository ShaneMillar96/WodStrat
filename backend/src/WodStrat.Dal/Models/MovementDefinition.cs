using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Represents a canonical movement in the workout parsing library.
/// </summary>
public class MovementDefinition
{
    /// <summary>
    /// Unique auto-incrementing identifier for the movement definition.
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
    /// Movement category (weightlifting/gymnastics/cardio/strongman/accessory).
    /// </summary>
    public MovementCategory Category { get; set; }

    /// <summary>
    /// Optional description of the movement.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Array of required equipment (e.g., ["pull_up_bar", "rings"]).
    /// </summary>
    public string[]? Equipment { get; set; }

    /// <summary>
    /// Default unit for weight-based movements (kg/lb/pood).
    /// </summary>
    public LoadUnit? DefaultLoadUnit { get; set; }

    /// <summary>
    /// Whether this is a bodyweight movement (no external load required).
    /// </summary>
    public bool IsBodyweight { get; set; }

    /// <summary>
    /// Whether this movement has standard RX weights in CrossFit.
    /// </summary>
    public bool HasRxWeights { get; set; }

    /// <summary>
    /// JSON object containing scaling options.
    /// </summary>
    public string? ScalingOptions { get; set; }

    /// <summary>
    /// Whether this movement is currently available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete flag (true = deleted).
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Collection of aliases for this movement.
    /// </summary>
    public ICollection<MovementAlias> Aliases { get; set; } = new List<MovementAlias>();

    /// <summary>
    /// Collection of workout movements referencing this definition.
    /// </summary>
    public ICollection<WorkoutMovement> WorkoutMovements { get; set; } = new List<WorkoutMovement>();

    /// <summary>
    /// Collection of benchmark mappings for this movement.
    /// </summary>
    public ICollection<BenchmarkMovementMapping> BenchmarkMappings { get; set; } = new List<BenchmarkMovementMapping>();
}
