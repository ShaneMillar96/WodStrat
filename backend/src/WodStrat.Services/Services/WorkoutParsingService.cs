using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Parsing;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for parsing workout text into structured data.
/// Orchestrates the parsing pipeline: preprocess -> detect type -> parse movements -> validate.
/// </summary>
public class WorkoutParsingService : IWorkoutParsingService
{
    private readonly IPatternMatchingService _patternMatchingService;
    private readonly IMovementDefinitionService _movementDefinitionService;

    // Pipeline components (created per-call for thread safety)
    private WorkoutTypeDetector? _typeDetector;
    private MovementLineParser? _movementParser;

    public WorkoutParsingService(
        IPatternMatchingService patternMatchingService,
        IMovementDefinitionService movementDefinitionService)
    {
        _patternMatchingService = patternMatchingService;
        _movementDefinitionService = movementDefinitionService;
    }

    /// <inheritdoc />
    public async Task<ParsedWorkoutDto> ParseWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default)
    {
        // Use the full parsing pipeline and extract the DTO
        var result = await ParseWorkoutAsync(workoutText, cancellationToken);
        return result.ParsedWorkout ?? CreateEmptyResult(workoutText, result.Errors);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParsingErrorDto>> ValidateWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default)
    {
        // Quick validation first
        var quickErrors = ParsedWorkoutValidator.QuickValidate(workoutText);
        if (quickErrors.Count > 0)
        {
            return quickErrors;
        }

        // Full parse for complete validation
        var result = await ParseWorkoutAsync(workoutText, cancellationToken);
        return result.Errors.ToList();
    }

    /// <inheritdoc />
    public async Task<ParsedWorkoutResult> ParseWorkoutAsync(string workoutText, CancellationToken cancellationToken = default)
    {
        // Initialize pipeline components
        EnsurePipelineInitialized();

        // Stage 1: Preprocess text
        var preprocessed = WorkoutTextPreprocessor.Preprocess(workoutText);

        if (preprocessed.IsEmpty)
        {
            return new ParsedWorkoutResult
            {
                Success = false,
                ConfidenceScore = 0,
                Errors = new List<ParsingErrorDto>
                {
                    new()
                    {
                        ErrorType = "EmptyInput",
                        Message = "Workout text cannot be empty.",
                        LineNumber = 0
                    }
                }
            };
        }

        // Stage 2: Detect workout type
        var typeDetection = _typeDetector!.Detect(preprocessed);

        // Stage 3: Parse movement lines
        var movementResults = await _movementParser!.ParseAllLinesAsync(preprocessed, cancellationToken);

        // Stage 4: Build the parsed workout DTO
        var parsedWorkout = BuildParsedWorkoutDto(workoutText, preprocessed, typeDetection, movementResults);

        // Stage 5: Validate and calculate confidence
        var result = ParsedWorkoutValidator.Validate(parsedWorkout, typeDetection, movementResults);

        return result;
    }

    /// <summary>
    /// Ensures pipeline components are initialized.
    /// </summary>
    private void EnsurePipelineInitialized()
    {
        _typeDetector ??= new WorkoutTypeDetector(_patternMatchingService);
        _movementParser ??= new MovementLineParser(_patternMatchingService, _movementDefinitionService);
    }

    /// <summary>
    /// Builds the ParsedWorkoutDto from pipeline results.
    /// </summary>
    private static ParsedWorkoutDto BuildParsedWorkoutDto(
        string originalText,
        PreprocessedWorkoutText preprocessed,
        WorkoutTypeDetectionResult typeDetection,
        IReadOnlyList<MovementParseResult> movementResults)
    {
        var movements = movementResults
            .Where(r => r.Success && r.Movement != null)
            .Select(r => r.Movement!)
            .ToList();

        // Apply rep schemes to movements that don't have explicit reps
        ApplyRepSchemes(movements, preprocessed, typeDetection);

        var errors = movementResults
            .Where(r => r.Error != null)
            .Select(r => r.Error!)
            .ToList();

        if (typeDetection.Error != null)
        {
            errors.Insert(0, typeDetection.Error);
        }

        // Determine the effective workout-level rep scheme
        // Only use workout-level rep scheme if there are no movement-specific schemes
        RepScheme? effectiveRepScheme = null;
        if (preprocessed.MovementRepSchemes.Count == 0)
        {
            effectiveRepScheme = preprocessed.WorkoutRepScheme ?? typeDetection.RepScheme;
        }
        else
        {
            effectiveRepScheme = preprocessed.WorkoutRepScheme;
        }

        var workout = new ParsedWorkoutDto
        {
            OriginalText = originalText,
            WorkoutType = typeDetection.Type,
            TimeCapSeconds = typeDetection.TimeCapSeconds,
            RoundCount = typeDetection.RoundCount,
            IntervalDurationSeconds = typeDetection.IntervalSeconds,
            Movements = movements,
            Errors = errors,
            RepSchemeReps = effectiveRepScheme?.Reps.ToArray(),
            RepSchemeType = effectiveRepScheme?.Type.ToString()
        };

        // Generate description
        workout.ParsedDescription = GenerateDescription(workout, preprocessed.WorkoutName);

        return workout;
    }

    /// <summary>
    /// Applies rep schemes to movements that don't have explicit rep counts.
    /// </summary>
    private static void ApplyRepSchemes(
        List<ParsedMovementDto> movements,
        PreprocessedWorkoutText preprocessed,
        WorkoutTypeDetectionResult typeDetection)
    {
        // Get the workout-level rep scheme as fallback
        var workoutRepScheme = preprocessed.WorkoutRepScheme ?? typeDetection.RepScheme;

        for (int i = 0; i < movements.Count; i++)
        {
            var movement = movements[i];

            // Skip if movement already has explicit rep count
            if (movement.RepCount.HasValue)
                continue;

            // Check for movement-specific rep scheme first (complex case)
            if (preprocessed.MovementRepSchemes.TryGetValue(i, out var specificScheme))
            {
                movement.RepSchemeReps = specificScheme.Reps.ToArray();
                movement.RepSchemeType = specificScheme.Type.ToString();
                continue;
            }

            // Fall back to workout-level rep scheme (simple case)
            if (workoutRepScheme != null)
            {
                movement.RepSchemeReps = workoutRepScheme.Reps.ToArray();
                movement.RepSchemeType = workoutRepScheme.Type.ToString();
            }
        }
    }

    /// <summary>
    /// Generates a human-readable description of the parsed workout.
    /// </summary>
    private static string GenerateDescription(ParsedWorkoutDto workout, string? workoutName)
    {
        var parts = new List<string>();

        // Add workout name if present
        if (!string.IsNullOrWhiteSpace(workoutName))
        {
            parts.Add($"\"{workoutName}\"");
        }

        // Add workout type
        parts.Add(workout.WorkoutType.ToString().ToUpperInvariant());

        // Add time domain
        if (workout.TimeCapSeconds.HasValue)
        {
            var minutes = workout.TimeCapSeconds.Value / 60;
            var seconds = workout.TimeCapSeconds.Value % 60;
            parts.Add(seconds > 0 ? $"{minutes}:{seconds:D2}" : $"{minutes} min");
        }

        if (workout.RoundCount.HasValue)
        {
            parts.Add($"{workout.RoundCount} rounds");
        }

        // Add movement count
        parts.Add($"{workout.Movements.Count} movement(s)");

        return string.Join(" - ", parts);
    }

    /// <summary>
    /// Creates an empty result with errors.
    /// </summary>
    private static ParsedWorkoutDto CreateEmptyResult(string originalText, IList<ParsingErrorDto> errors)
    {
        return new ParsedWorkoutDto
        {
            OriginalText = originalText,
            Movements = new List<ParsedMovementDto>(),
            Errors = errors.ToList()
        };
    }
}
