using System.Text.RegularExpressions;
using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for parsing workout text into structured data.
/// </summary>
public partial class WorkoutParsingService : IWorkoutParsingService
{
    private readonly IMovementDefinitionService _movementDefinitionService;

    // Cached alias lookup for parsing efficiency
    private IReadOnlyDictionary<string, int>? _aliasLookup;

    public WorkoutParsingService(IMovementDefinitionService movementDefinitionService)
    {
        _movementDefinitionService = movementDefinitionService;
    }

    #region Regex Patterns (Generated)

    // AMRAP patterns: "20 min AMRAP", "AMRAP 20", "15 minute AMRAP", "AMRAP in 20 minutes"
    [GeneratedRegex(@"(?:(\d+)\s*(?:min(?:ute)?s?)\s*AMRAP|AMRAP\s*(?:in\s*)?(\d+)\s*(?:min(?:ute)?s?)?|AMRAP)", RegexOptions.IgnoreCase)]
    private static partial Regex AmrapPatternRegex();

    // For Time patterns: "For Time", "3 Rounds For Time", "21-15-9"
    [GeneratedRegex(@"(?:(\d+)\s*(?:Rounds?\s*)?For\s*Time|For\s*Time|^(\d+-)+\d+$)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ForTimePatternRegex();

    // Time cap patterns: "Time Cap: 20", "20 min cap", "Cap: 15:00", "TC: 12"
    [GeneratedRegex(@"(?:Time\s*Cap|Cap|TC)\s*[:=]?\s*(\d+)(?::(\d+))?(?:\s*(?:min(?:ute)?s?)?)?", RegexOptions.IgnoreCase)]
    private static partial Regex TimeCapPatternRegex();

    // EMOM patterns: "EMOM", "E2MOM", "Every 2 minutes", "EMOM 20", "10 min EMOM"
    [GeneratedRegex(@"(?:E(\d*)MOM|Every\s*(\d+)?\s*(?:min(?:ute)?s?)\s*(?:on\s*the\s*(?:min(?:ute)?)?)?|(\d+)\s*(?:min(?:ute)?s?)\s*EMOM|EMOM\s*(\d+)?)", RegexOptions.IgnoreCase)]
    private static partial Regex EmomPatternRegex();

    // Rounds pattern: "5 Rounds", "5 RFT", "3 Sets"
    [GeneratedRegex(@"(\d+)\s*(?:Rounds?|RFT|Sets?)", RegexOptions.IgnoreCase)]
    private static partial Regex RoundsPatternRegex();

    // Movement with reps: "10 Pull-ups", "21 Thrusters", "15 Box Jumps (24 in)"
    [GeneratedRegex(@"^(\d+)\s+(.+?)(?:\s*\(([^)]+)\))?$", RegexOptions.Multiline)]
    private static partial Regex MovementWithRepsRegex();

    // Load pattern: "135 lb", "60 kg", "1.5 pood", "(95/65 lb)"
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*(lb|kg|pood)s?", RegexOptions.IgnoreCase)]
    private static partial Regex LoadPatternRegex();

    // Male/Female load pattern: "(135/95 lb)", "95/65"
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*/\s*(\d+(?:\.\d+)?)\s*(lb|kg|pood)?", RegexOptions.IgnoreCase)]
    private static partial Regex GenderLoadPatternRegex();

    // Distance pattern: "400m Run", "1 mile run", "500 m Row"
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*(m|km|ft|mi(?:le)?)\s*", RegexOptions.IgnoreCase)]
    private static partial Regex DistancePatternRegex();

    // Calorie pattern: "15 Cal Row", "15/12 Cal Bike", "20 calories"
    [GeneratedRegex(@"(\d+)(?:\s*/\s*(\d+))?\s*(?:Cal(?:orie)?s?)", RegexOptions.IgnoreCase)]
    private static partial Regex CaloriePatternRegex();

    // Duration pattern: "30 sec hold", ":30 L-sit", "1 min plank"
    [GeneratedRegex(@"(?:(\d+)\s*(?:sec(?:ond)?s?|:)|:(\d+)|(\d+)\s*min(?:ute)?s?)", RegexOptions.IgnoreCase)]
    private static partial Regex DurationPatternRegex();

    // Rep scheme pattern for chipper-style: "21-15-9", "10-9-8-7-6-5-4-3-2-1"
    [GeneratedRegex(@"^(\d+(?:-\d+)+)$")]
    private static partial Regex RepSchemePatternRegex();

    #endregion

    /// <inheritdoc />
    public async Task<ParsedWorkoutDto> ParseWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default)
    {
        var result = new ParsedWorkoutDto
        {
            OriginalText = workoutText,
            Movements = new List<ParsedMovementDto>(),
            Errors = new List<ParsingErrorDto>()
        };

        if (string.IsNullOrWhiteSpace(workoutText))
        {
            result.Errors.Add(new ParsingErrorDto
            {
                ErrorType = "EmptyInput",
                Message = "Workout text cannot be empty.",
                LineNumber = 0
            });
            return result;
        }

        // Ensure alias lookup is loaded
        _aliasLookup ??= await _movementDefinitionService.GetAliasLookupAsync(cancellationToken);

        // Detect workout type and time domain
        DetectWorkoutType(workoutText, result);

        // Parse movements
        await ParseMovementsAsync(workoutText, result, cancellationToken);

        // Generate parsed description
        result.ParsedDescription = GenerateParsedDescription(result);

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParsingErrorDto>> ValidateWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default)
    {
        var parsed = await ParseWorkoutTextAsync(workoutText, cancellationToken);
        return parsed.Errors.ToList().AsReadOnly();
    }

    #region Private Parsing Methods

    private void DetectWorkoutType(string text, ParsedWorkoutDto result)
    {
        // Check for AMRAP
        var amrapMatch = AmrapPatternRegex().Match(text);
        if (amrapMatch.Success)
        {
            result.WorkoutType = WorkoutType.Amrap;

            // Extract duration
            var durationStr = amrapMatch.Groups[1].Success ? amrapMatch.Groups[1].Value :
                              amrapMatch.Groups[2].Success ? amrapMatch.Groups[2].Value : null;

            if (int.TryParse(durationStr, out var duration))
            {
                result.TimeCapSeconds = duration * 60; // Convert minutes to seconds
            }
            return;
        }

        // Check for EMOM
        var emomMatch = EmomPatternRegex().Match(text);
        if (emomMatch.Success)
        {
            result.WorkoutType = WorkoutType.Emom;

            // Extract interval duration (default 1 minute)
            var intervalStr = emomMatch.Groups[1].Success ? emomMatch.Groups[1].Value :
                              emomMatch.Groups[2].Success ? emomMatch.Groups[2].Value : "1";

            if (int.TryParse(intervalStr, out var interval))
            {
                result.IntervalDurationSeconds = interval * 60;
            }
            else
            {
                result.IntervalDurationSeconds = 60; // Default 1 minute
            }

            // Check for total duration
            var durationStr = emomMatch.Groups[3].Success ? emomMatch.Groups[3].Value :
                              emomMatch.Groups[4].Success ? emomMatch.Groups[4].Value : null;

            if (int.TryParse(durationStr, out var totalDuration))
            {
                result.TimeCapSeconds = totalDuration * 60;
            }
            return;
        }

        // Check for For Time
        var forTimeMatch = ForTimePatternRegex().Match(text);
        if (forTimeMatch.Success)
        {
            result.WorkoutType = WorkoutType.ForTime;

            // Check for rounds
            if (forTimeMatch.Groups[1].Success && int.TryParse(forTimeMatch.Groups[1].Value, out var rounds))
            {
                result.RoundCount = rounds;
            }

            // Check for time cap
            var timeCapMatch = TimeCapPatternRegex().Match(text);
            if (timeCapMatch.Success)
            {
                var minutes = int.Parse(timeCapMatch.Groups[1].Value);
                var seconds = timeCapMatch.Groups[2].Success ? int.Parse(timeCapMatch.Groups[2].Value) : 0;
                result.TimeCapSeconds = (minutes * 60) + seconds;
            }
            return;
        }

        // Check for rounds without "For Time" (rounds for quality)
        var roundsMatch = RoundsPatternRegex().Match(text);
        if (roundsMatch.Success)
        {
            result.WorkoutType = WorkoutType.Rounds;
            result.RoundCount = int.Parse(roundsMatch.Groups[1].Value);
            return;
        }

        // Default to ForTime if movements are detected but no type specified
        result.WorkoutType = WorkoutType.ForTime;
    }

    private async Task ParseMovementsAsync(string text, ParsedWorkoutDto result, CancellationToken cancellationToken)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var sequenceOrder = 0;

        // Check for chipper-style rep scheme (21-15-9, etc.)
        string[]? repScheme = null;
        foreach (var line in lines)
        {
            var repSchemeMatch = RepSchemePatternRegex().Match(line.Trim());
            if (repSchemeMatch.Success)
            {
                repScheme = repSchemeMatch.Groups[1].Value.Split('-');
                break;
            }
        }

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and header lines (workout type indicators)
            if (string.IsNullOrWhiteSpace(trimmedLine) ||
                AmrapPatternRegex().IsMatch(trimmedLine) ||
                ForTimePatternRegex().IsMatch(trimmedLine) ||
                EmomPatternRegex().IsMatch(trimmedLine) ||
                TimeCapPatternRegex().IsMatch(trimmedLine) ||
                RepSchemePatternRegex().IsMatch(trimmedLine))
            {
                continue;
            }

            var movement = await ParseMovementLineAsync(trimmedLine, ++sequenceOrder, cancellationToken);
            if (movement != null)
            {
                result.Movements.Add(movement);
            }
            else
            {
                // Add error for unrecognized movement
                result.Errors.Add(new ParsingErrorDto
                {
                    ErrorType = "UnrecognizedMovement",
                    Message = $"Could not parse movement: '{trimmedLine}'",
                    LineNumber = sequenceOrder,
                    OriginalText = trimmedLine
                });
            }
        }
    }

    private async Task<ParsedMovementDto?> ParseMovementLineAsync(string line, int sequenceOrder, CancellationToken cancellationToken)
    {
        var movement = new ParsedMovementDto
        {
            SequenceOrder = sequenceOrder,
            OriginalText = line
        };

        // Try to match movement with reps pattern: "10 Pull-ups"
        var movementMatch = MovementWithRepsRegex().Match(line);
        if (movementMatch.Success)
        {
            // Extract rep count
            if (int.TryParse(movementMatch.Groups[1].Value, out var reps))
            {
                movement.RepCount = reps;
            }

            var movementText = movementMatch.Groups[2].Value.Trim();
            var modifiersText = movementMatch.Groups[3].Success ? movementMatch.Groups[3].Value : "";

            // Try to identify the movement
            var identified = await IdentifyMovementAsync(movementText, movement, cancellationToken);

            // Parse modifiers (load, distance, etc.)
            ParseModifiers(line, movement);

            return identified ? movement : null;
        }

        // Try distance-based movement: "400m Run"
        var distanceMatch = DistancePatternRegex().Match(line);
        if (distanceMatch.Success)
        {
            movement.DistanceValue = decimal.Parse(distanceMatch.Groups[1].Value);
            movement.DistanceUnit = ParseDistanceUnit(distanceMatch.Groups[2].Value);

            // Extract movement name after distance
            var remainingText = DistancePatternRegex().Replace(line, "").Trim();
            if (!string.IsNullOrEmpty(remainingText))
            {
                var identified = await IdentifyMovementAsync(remainingText, movement, cancellationToken);
                return identified ? movement : null;
            }
        }

        // Try calorie-based movement: "15 Cal Row"
        var calorieMatch = CaloriePatternRegex().Match(line);
        if (calorieMatch.Success)
        {
            movement.Calories = int.Parse(calorieMatch.Groups[1].Value);
            if (calorieMatch.Groups[2].Success)
            {
                movement.CaloriesFemale = int.Parse(calorieMatch.Groups[2].Value);
            }

            // Extract movement name
            var remainingText = CaloriePatternRegex().Replace(line, "").Trim();
            if (!string.IsNullOrEmpty(remainingText))
            {
                var identified = await IdentifyMovementAsync(remainingText, movement, cancellationToken);
                return identified ? movement : null;
            }
        }

        return null;
    }

    private async Task<bool> IdentifyMovementAsync(string text, ParsedMovementDto movement, CancellationToken cancellationToken)
    {
        // Normalize text for lookup
        var normalizedText = text.ToLowerInvariant()
            .Replace("-", " ")
            .Replace("_", " ")
            .Trim();

        // Remove common suffixes
        normalizedText = normalizedText
            .Replace(" s", "s") // "pull up s" -> "pull ups"
            .TrimEnd('s'); // Remove trailing 's' for plural handling

        // Try direct lookup first
        if (_aliasLookup!.TryGetValue(normalizedText, out var movementId))
        {
            movement.MovementDefinitionId = movementId;
            var movementDef = await _movementDefinitionService.FindMovementByAliasAsync(text, cancellationToken);
            if (movementDef != null)
            {
                movement.MovementName = movementDef.DisplayName;
                movement.MovementCanonicalName = movementDef.CanonicalName;
                movement.MovementCategory = movementDef.Category;
            }
            return true;
        }

        // Try partial matching - find the longest matching alias
        var bestMatch = _aliasLookup.Keys
            .Where(alias => normalizedText.Contains(alias) || alias.Contains(normalizedText))
            .OrderByDescending(alias => alias.Length)
            .FirstOrDefault();

        if (bestMatch != null)
        {
            movement.MovementDefinitionId = _aliasLookup[bestMatch];
            var movementDef = await _movementDefinitionService.FindMovementByAliasAsync(bestMatch, cancellationToken);
            if (movementDef != null)
            {
                movement.MovementName = movementDef.DisplayName;
                movement.MovementCanonicalName = movementDef.CanonicalName;
                movement.MovementCategory = movementDef.Category;
            }
            return true;
        }

        return false;
    }

    private void ParseModifiers(string line, ParsedMovementDto movement)
    {
        // Parse load (weight)
        var genderLoadMatch = GenderLoadPatternRegex().Match(line);
        if (genderLoadMatch.Success)
        {
            movement.LoadValue = decimal.Parse(genderLoadMatch.Groups[1].Value);
            movement.LoadValueFemale = decimal.Parse(genderLoadMatch.Groups[2].Value);
            if (genderLoadMatch.Groups[3].Success)
            {
                movement.LoadUnit = ParseLoadUnit(genderLoadMatch.Groups[3].Value);
            }
            else
            {
                movement.LoadUnit = LoadUnit.Lb; // Default to pounds
            }
        }
        else
        {
            var loadMatch = LoadPatternRegex().Match(line);
            if (loadMatch.Success)
            {
                movement.LoadValue = decimal.Parse(loadMatch.Groups[1].Value);
                movement.LoadUnit = ParseLoadUnit(loadMatch.Groups[2].Value);
            }
        }

        // Parse distance if not already set
        if (!movement.DistanceValue.HasValue)
        {
            var distanceMatch = DistancePatternRegex().Match(line);
            if (distanceMatch.Success)
            {
                movement.DistanceValue = decimal.Parse(distanceMatch.Groups[1].Value);
                movement.DistanceUnit = ParseDistanceUnit(distanceMatch.Groups[2].Value);
            }
        }

        // Parse duration
        var durationMatch = DurationPatternRegex().Match(line);
        if (durationMatch.Success)
        {
            var seconds = durationMatch.Groups[1].Success ? int.Parse(durationMatch.Groups[1].Value) :
                          durationMatch.Groups[2].Success ? int.Parse(durationMatch.Groups[2].Value) : 0;
            var minutes = durationMatch.Groups[3].Success ? int.Parse(durationMatch.Groups[3].Value) : 0;
            movement.DurationSeconds = (minutes * 60) + seconds;
        }
    }

    private static LoadUnit ParseLoadUnit(string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "kg" => LoadUnit.Kg,
            "lb" or "lbs" => LoadUnit.Lb,
            "pood" or "poods" => LoadUnit.Pood,
            _ => LoadUnit.Lb
        };
    }

    private static DistanceUnit ParseDistanceUnit(string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "m" => DistanceUnit.M,
            "km" => DistanceUnit.Km,
            "ft" => DistanceUnit.Ft,
            "mi" or "mile" or "miles" => DistanceUnit.Mi,
            "cal" => DistanceUnit.Cal,
            _ => DistanceUnit.M
        };
    }

    private static string GenerateParsedDescription(ParsedWorkoutDto result)
    {
        var parts = new List<string>();

        // Add workout type
        parts.Add(result.WorkoutType.ToString().ToUpperInvariant());

        // Add time domain
        if (result.TimeCapSeconds.HasValue)
        {
            var minutes = result.TimeCapSeconds.Value / 60;
            parts.Add($"{minutes} min");
        }

        if (result.RoundCount.HasValue)
        {
            parts.Add($"{result.RoundCount} rounds");
        }

        // Add movement count
        parts.Add($"{result.Movements.Count} movement(s)");

        return string.Join(" - ", parts);
    }

    #endregion
}
