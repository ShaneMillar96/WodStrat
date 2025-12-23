namespace WodStrat.Services.Parsing.Errors;

using System.ComponentModel;

/// <summary>
/// Enumeration of all possible parsing error codes.
/// Each code maps to a specific error message template.
/// </summary>
public enum ParsingErrorCode
{
    // Input validation errors (1xx)
    [Description("Empty input")]
    EmptyInput = 100,

    [Description("Input too long")]
    InputTooLong = 101,

    [Description("Input too short")]
    InputTooShort = 102,

    [Description("Binary or encoded content detected")]
    BinaryContent = 103,

    [Description("Invalid characters detected")]
    InvalidCharacters = 104,

    // Structural errors (2xx)
    [Description("No workout structure detected")]
    NoWorkoutStructure = 200,

    [Description("No movements detected")]
    NoMovementsDetected = 201,

    [Description("Invalid workout type")]
    InvalidWorkoutType = 202,

    [Description("Ambiguous workout type")]
    AmbiguousWorkoutType = 203,

    [Description("Missing duration for timed workout")]
    MissingDuration = 204,

    [Description("Missing round count")]
    MissingRoundCount = 205,

    [Description("Contradictory metadata")]
    ContradictoryMetadata = 206,

    // Movement parsing errors (3xx)
    [Description("Unknown movement")]
    UnknownMovement = 300,

    [Description("Ambiguous movement match")]
    AmbiguousMovement = 301,

    [Description("Invalid rep count")]
    InvalidRepCount = 302,

    [Description("Invalid weight")]
    InvalidWeight = 303,

    [Description("Invalid distance")]
    InvalidDistance = 304,

    [Description("Invalid time value")]
    InvalidTime = 305,

    [Description("Invalid calories")]
    InvalidCalories = 306,

    [Description("Empty movement line")]
    EmptyMovementLine = 307,

    [Description("Unrecognized movement format")]
    UnrecognizedMovementFormat = 308,

    // Data consistency errors (4xx)
    [Description("Duplicate movement in sequence")]
    DuplicateMovement = 400,

    [Description("Inconsistent units")]
    InconsistentUnits = 401,

    [Description("Value out of reasonable range")]
    ValueOutOfRange = 402,

    // System/internal errors (5xx)
    [Description("Internal parsing error")]
    InternalError = 500,

    [Description("Parsing timeout")]
    Timeout = 501
}
