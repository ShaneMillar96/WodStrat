namespace WodStrat.Services.Parsing;

/// <summary>
/// Represents a gender-differentiated calorie pair.
/// </summary>
/// <param name="Male">The male (higher) calorie target.</param>
/// <param name="Female">The female (lower) calorie target.</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record CaloriePair(
    int Male,
    int Female,
    string OriginalText
);
