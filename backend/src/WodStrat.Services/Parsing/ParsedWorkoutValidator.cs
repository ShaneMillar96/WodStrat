using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Validates parsed workout data and calculates confidence scores.
/// </summary>
public static class ParsedWorkoutValidator
{
    /// <summary>
    /// Validates a parsed workout and generates confidence scores.
    /// </summary>
    /// <param name="workout">The parsed workout DTO.</param>
    /// <param name="typeDetection">The workout type detection result.</param>
    /// <param name="movementResults">The movement parsing results.</param>
    /// <returns>Validation result with confidence breakdown.</returns>
    public static ParsedWorkoutResult Validate(
        ParsedWorkoutDto workout,
        WorkoutTypeDetectionResult typeDetection,
        IReadOnlyList<MovementParseResult> movementResults)
    {
        var errors = new List<ParsingErrorDto>();
        var warnings = new List<ParsingWarningDto>();

        // Collect errors and warnings from type detection
        if (typeDetection.Error != null)
        {
            errors.Add(typeDetection.Error);
        }
        if (typeDetection.Warning != null)
        {
            warnings.Add(typeDetection.Warning);
        }

        // Collect errors and warnings from movement parsing
        foreach (var result in movementResults)
        {
            if (result.Error != null)
            {
                errors.Add(result.Error);
            }
            if (result.Warning != null)
            {
                warnings.Add(result.Warning);
            }
        }

        // Add workout-level validation errors
        if (workout.Movements.Count == 0)
        {
            errors.Add(new ParsingErrorDto
            {
                ErrorType = "NoMovements",
                Message = "No movements could be parsed from the workout text.",
                LineNumber = 0
            });
        }

        // Validate time domain consistency
        ValidateTimeDomain(workout, warnings);

        // Calculate confidence breakdown
        var breakdown = CalculateConfidenceBreakdown(workout, typeDetection, movementResults);

        // Calculate overall confidence
        var overallConfidence = CalculateOverallConfidence(breakdown, errors.Count, warnings.Count);

        return new ParsedWorkoutResult
        {
            Success = errors.Count == 0,
            ParsedWorkout = workout,
            ConfidenceScore = overallConfidence,
            Errors = errors,
            Warnings = warnings,
            ConfidenceDetails = breakdown
        };
    }

    /// <summary>
    /// Validates time domain consistency.
    /// </summary>
    private static void ValidateTimeDomain(ParsedWorkoutDto workout, List<ParsingWarningDto> warnings)
    {
        switch (workout.WorkoutType)
        {
            case WorkoutType.Amrap when !workout.TimeCapSeconds.HasValue:
                warnings.Add(new ParsingWarningDto
                {
                    WarningType = "MissingTimeCap",
                    Message = "AMRAP workout without a time cap specified.",
                    Suggestion = "Consider adding a time cap (e.g., '20 min AMRAP')"
                });
                break;

            case WorkoutType.Emom when !workout.IntervalDurationSeconds.HasValue:
                warnings.Add(new ParsingWarningDto
                {
                    WarningType = "MissingInterval",
                    Message = "EMOM workout without interval duration specified.",
                    Suggestion = "Interval defaults to 1 minute"
                });
                break;

            case WorkoutType.Rounds when !workout.RoundCount.HasValue:
                warnings.Add(new ParsingWarningDto
                {
                    WarningType = "MissingRounds",
                    Message = "Rounds-based workout without round count specified.",
                    Suggestion = "Consider specifying the number of rounds"
                });
                break;
        }
    }

    /// <summary>
    /// Calculates the confidence breakdown.
    /// </summary>
    private static ConfidenceBreakdown CalculateConfidenceBreakdown(
        ParsedWorkoutDto workout,
        WorkoutTypeDetectionResult typeDetection,
        IReadOnlyList<MovementParseResult> movementResults)
    {
        var identifiedCount = movementResults.Count(r => r.Success && r.Movement?.MovementDefinitionId.HasValue == true);
        var totalLines = movementResults.Count;
        var successfulResults = movementResults.Where(r => r.Success).ToList();

        // Calculate movement identification confidence
        var movementConfidence = successfulResults.Count > 0
            ? (int)successfulResults.Average(r => r.Confidence)
            : 0;

        // Calculate time domain confidence
        var timeDomainConfidence = 100;
        if (workout.WorkoutType == WorkoutType.Amrap && !workout.TimeCapSeconds.HasValue)
            timeDomainConfidence = 50;
        else if (workout.WorkoutType == WorkoutType.Emom && !workout.IntervalDurationSeconds.HasValue)
            timeDomainConfidence = 70;
        else if (workout.WorkoutType == WorkoutType.Rounds && !workout.RoundCount.HasValue)
            timeDomainConfidence = 60;

        // Count movements with complete data
        var completeDataCount = workout.Movements.Count(m =>
            (m.RepCount.HasValue || m.DistanceValue.HasValue || m.Calories.HasValue || m.DurationSeconds.HasValue) &&
            m.MovementDefinitionId.HasValue);

        return new ConfidenceBreakdown
        {
            WorkoutTypeConfidence = typeDetection.Confidence,
            TimeDomainConfidence = timeDomainConfidence,
            MovementIdentificationConfidence = movementConfidence,
            MovementsIdentified = identifiedCount,
            TotalMovementLines = totalLines,
            MovementsWithCompleteData = completeDataCount
        };
    }

    /// <summary>
    /// Calculates the overall confidence score.
    /// </summary>
    private static int CalculateOverallConfidence(
        ConfidenceBreakdown breakdown,
        int errorCount,
        int warningCount)
    {
        if (errorCount > 0)
        {
            // Errors significantly reduce confidence
            return Math.Max(0, 40 - (errorCount * 10));
        }

        // Weight the different factors
        var weightedScore =
            (breakdown.WorkoutTypeConfidence * 0.2) +
            (breakdown.TimeDomainConfidence * 0.15) +
            (breakdown.MovementIdentificationConfidence * 0.5) +
            ((double)breakdown.MovementIdentificationRate * 0.15);

        // Reduce for warnings
        var warningPenalty = Math.Min(20, warningCount * 5);
        var finalScore = (int)weightedScore - warningPenalty;

        return Math.Clamp(finalScore, 0, 100);
    }

    /// <summary>
    /// Quick validation for lightweight checking.
    /// </summary>
    public static IReadOnlyList<ParsingErrorDto> QuickValidate(string workoutText)
    {
        var errors = new List<ParsingErrorDto>();

        if (string.IsNullOrWhiteSpace(workoutText))
        {
            errors.Add(new ParsingErrorDto
            {
                ErrorType = "EmptyInput",
                Message = "Workout text cannot be empty.",
                LineNumber = 0
            });
            return errors;
        }

        // Check minimum length
        if (workoutText.Trim().Length < 5)
        {
            errors.Add(new ParsingErrorDto
            {
                ErrorType = "TooShort",
                Message = "Workout text is too short to contain valid workout data.",
                LineNumber = 0,
                OriginalText = workoutText
            });
        }

        // Check for at least one number (reps, time, etc.)
        if (!System.Text.RegularExpressions.Regex.IsMatch(workoutText, @"\d"))
        {
            errors.Add(new ParsingErrorDto
            {
                ErrorType = "NoNumbers",
                Message = "Workout text should contain at least one number (reps, duration, etc.).",
                LineNumber = 0,
                OriginalText = workoutText
            });
        }

        return errors;
    }
}
