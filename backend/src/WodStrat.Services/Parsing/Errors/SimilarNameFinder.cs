namespace WodStrat.Services.Parsing.Errors;

/// <summary>
/// Finds similar movement names using string distance algorithms.
/// Used to provide helpful suggestions when a movement is not recognized.
/// </summary>
public static class SimilarNameFinder
{
    /// <summary>
    /// Maximum Levenshtein distance to consider a match.
    /// </summary>
    private const int MaxDistance = 3;

    /// <summary>
    /// Finds similar names from a list of known names.
    /// </summary>
    /// <param name="input">The unrecognized input.</param>
    /// <param name="knownNames">List of valid movement names.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>List of similar names ordered by similarity.</returns>
    public static IReadOnlyList<string> FindSimilar(
        string input,
        IEnumerable<string> knownNames,
        int maxSuggestions = 3)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var inputLower = input.ToLowerInvariant();

        var matches = knownNames
            .Select(name => new
            {
                Name = name,
                Distance = LevenshteinDistance(inputLower, name.ToLowerInvariant())
            })
            .Where(x => x.Distance <= MaxDistance)
            .OrderBy(x => x.Distance)
            .Take(maxSuggestions)
            .Select(x => x.Name)
            .ToList();

        return matches;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;

        var distance = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
            distance[i, 0] = i;

        for (var j = 0; j <= targetLength; j++)
            distance[0, j] = j;

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(
                        distance[i - 1, j] + 1,      // deletion
                        distance[i, j - 1] + 1),     // insertion
                    distance[i - 1, j - 1] + cost);  // substitution
            }
        }

        return distance[sourceLength, targetLength];
    }
}
