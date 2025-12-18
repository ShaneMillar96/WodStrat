using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Parses individual movement lines and identifies movements from the dictionary.
/// </summary>
public class MovementLineParser
{
    private readonly IPatternMatchingService _patternMatchingService;
    private readonly IMovementDefinitionService _movementDefinitionService;

    public MovementLineParser(
        IPatternMatchingService patternMatchingService,
        IMovementDefinitionService movementDefinitionService)
    {
        _patternMatchingService = patternMatchingService;
        _movementDefinitionService = movementDefinitionService;
    }

    /// <summary>
    /// Parses a movement line and attempts to identify the movement.
    /// </summary>
    /// <param name="line">The movement line text.</param>
    /// <param name="sequenceOrder">The sequence order (1-indexed).</param>
    /// <param name="lineNumber">The original line number in input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Movement parse result with identification status.</returns>
    public async Task<MovementParseResult> ParseLineAsync(
        string line,
        int sequenceOrder,
        int lineNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return new MovementParseResult
            {
                Success = false,
                Confidence = 0,
                LineNumber = lineNumber,
                OriginalText = line ?? string.Empty,
                Error = new ParsingErrorDto
                {
                    ErrorType = "EmptyLine",
                    Message = "Movement line is empty.",
                    LineNumber = lineNumber,
                    OriginalText = line
                }
            };
        }

        // Use pattern matching service to extract components
        var parsed = _patternMatchingService.ParseMovementLine(line);

        // Create base movement DTO
        var movement = new ParsedMovementDto
        {
            SequenceOrder = sequenceOrder,
            OriginalText = line,
            RepCount = parsed.Reps,
            DurationSeconds = parsed.DurationSeconds,
            Notes = parsed.Modifiers
        };

        // Handle weight
        if (parsed.WeightPair != null)
        {
            movement.LoadValue = parsed.WeightPair.Male.Value;
            movement.LoadValueFemale = parsed.WeightPair.Female.Value;
            movement.LoadUnit = parsed.WeightPair.Male.Unit;
        }
        else if (parsed.Weight != null)
        {
            movement.LoadValue = parsed.Weight.Value;
            movement.LoadUnit = parsed.Weight.Unit;
        }

        // Handle distance
        if (parsed.Distance != null)
        {
            movement.DistanceValue = parsed.Distance.Value;
            movement.DistanceUnit = parsed.Distance.Unit;
        }

        // Handle calories
        if (parsed.CaloriePair != null)
        {
            movement.Calories = parsed.CaloriePair.Male;
            movement.CaloriesFemale = parsed.CaloriePair.Female;
        }
        else if (parsed.Calories.HasValue)
        {
            movement.Calories = parsed.Calories.Value;
        }

        // Try to identify the movement from the dictionary
        var (identified, confidence, warning) = await IdentifyMovementAsync(
            parsed.MovementText, movement, cancellationToken);

        if (!identified && !parsed.HasQuantity)
        {
            // Completely unrecognized line
            return new MovementParseResult
            {
                Success = false,
                Confidence = 0,
                LineNumber = lineNumber,
                OriginalText = line,
                Error = new ParsingErrorDto
                {
                    ErrorType = "UnrecognizedMovement",
                    Message = $"Could not parse movement: '{line}'",
                    LineNumber = lineNumber,
                    OriginalText = line
                }
            };
        }

        return new MovementParseResult
        {
            Success = true,
            Movement = movement,
            Confidence = confidence,
            LineNumber = lineNumber,
            OriginalText = line,
            Warning = warning
        };
    }

    /// <summary>
    /// Identifies a movement from text using the movement dictionary.
    /// </summary>
    private async Task<(bool Identified, int Confidence, ParsingWarningDto? Warning)> IdentifyMovementAsync(
        string movementText,
        ParsedMovementDto movement,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(movementText))
        {
            return (false, 0, null);
        }

        // Try to normalize the movement name
        var canonicalName = await _movementDefinitionService.NormalizeMovementNameAsync(
            movementText, cancellationToken);

        if (canonicalName != null)
        {
            // Found exact or close match
            var definition = await _movementDefinitionService.GetMovementByCanonicalNameAsync(
                canonicalName, cancellationToken);

            if (definition != null)
            {
                movement.MovementDefinitionId = definition.Id;
                movement.MovementName = definition.DisplayName;
                movement.MovementCanonicalName = definition.CanonicalName;
                movement.MovementCategory = definition.Category;
                return (true, 100, null);
            }
        }

        // Try fuzzy search
        var searchResults = await _movementDefinitionService.SearchMovementsAsync(
            movementText, cancellationToken);

        if (searchResults.Count > 0)
        {
            var bestMatch = searchResults[0];
            movement.MovementDefinitionId = bestMatch.Id;
            movement.MovementName = bestMatch.DisplayName;
            movement.MovementCanonicalName = bestMatch.CanonicalName;
            movement.MovementCategory = bestMatch.Category;

            // Lower confidence for fuzzy match
            var confidence = 80;
            ParsingWarningDto? warning = null;

            if (searchResults.Count > 1)
            {
                confidence = 70;
                warning = new ParsingWarningDto
                {
                    WarningType = "AmbiguousMovement",
                    Message = $"'{movementText}' matched to '{bestMatch.DisplayName}' but other matches exist.",
                    OriginalText = movementText,
                    Suggestion = $"Verify this is the intended movement. Other options: {string.Join(", ", searchResults.Skip(1).Take(2).Select(m => m.DisplayName))}"
                };
            }

            return (true, confidence, warning);
        }

        // No match found - movement not in dictionary
        return (false, 30, new ParsingWarningDto
        {
            WarningType = "UnknownMovement",
            Message = $"Movement '{movementText}' not found in dictionary. Using as-is.",
            OriginalText = movementText,
            Suggestion = "Consider adding this movement to the dictionary or checking spelling."
        });
    }

    /// <summary>
    /// Parses all movement lines from preprocessed text.
    /// </summary>
    public async Task<IReadOnlyList<MovementParseResult>> ParseAllLinesAsync(
        PreprocessedWorkoutText preprocessed,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MovementParseResult>();
        var sequenceOrder = 0;

        foreach (var line in preprocessed.MovementLines)
        {
            sequenceOrder++;
            var result = await ParseLineAsync(line, sequenceOrder, sequenceOrder, cancellationToken);
            results.Add(result);
        }

        return results;
    }
}
