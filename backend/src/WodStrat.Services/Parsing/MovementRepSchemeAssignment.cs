namespace WodStrat.Services.Parsing;

/// <summary>
/// Associates a rep scheme with a specific movement index.
/// </summary>
public sealed record MovementRepSchemeAssignment(
    int MovementIndex,
    RepScheme RepScheme
);
