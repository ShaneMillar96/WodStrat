namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for movement normalization results.
/// Used when the API needs to return both the canonical name and full movement details.
/// </summary>
public class NormalizeMovementResultDto
{
    /// <summary>
    /// The input that was normalized.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// The canonical name if a match was found; otherwise null.
    /// </summary>
    public string? CanonicalName { get; set; }

    /// <summary>
    /// Indicates if the input was successfully normalized to a canonical movement.
    /// </summary>
    public bool IsMatch { get; set; }

    /// <summary>
    /// The full movement definition if a match was found; otherwise null.
    /// </summary>
    public MovementDefinitionDto? Movement { get; set; }
}
