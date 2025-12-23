namespace WodStrat.Services.Extensions;

using WodStrat.Services.Dtos;
using WodStrat.Services.Parsing.Errors;

/// <summary>
/// Extension methods for ParsingError conversions.
/// </summary>
public static class ParsingErrorExtensions
{
    /// <summary>
    /// Converts a ParsingError to ParsingErrorDto.
    /// </summary>
    public static ParsingErrorDto ToErrorDto(this ParsingError error)
    {
        return new ParsingErrorDto
        {
            ErrorType = error.Code.ToString(),
            ErrorCode = (int)error.Code,
            Message = error.Message,
            LineNumber = error.LineNumber ?? 0,
            OriginalText = error.Context,
            Suggestion = error.Suggestion,
            SimilarNames = error.SimilarNames
        };
    }

    /// <summary>
    /// Converts a ParsingError to ParsingWarningDto.
    /// </summary>
    public static ParsingWarningDto ToWarningDto(this ParsingError error)
    {
        return new ParsingWarningDto
        {
            WarningType = error.Code.ToString(),
            WarningCode = (int)error.Code,
            Message = error.Message,
            LineNumber = error.LineNumber ?? 0,
            OriginalText = error.Context,
            Suggestion = error.Suggestion,
            SimilarNames = error.SimilarNames
        };
    }

    /// <summary>
    /// Converts a collection of ParsingError to appropriate DTOs.
    /// Errors become ParsingErrorDto, Warnings/Info become ParsingWarningDto.
    /// </summary>
    public static (List<ParsingErrorDto> Errors, List<ParsingWarningDto> Warnings) ToDtos(
        this IEnumerable<ParsingError> errors)
    {
        var errorDtos = new List<ParsingErrorDto>();
        var warningDtos = new List<ParsingWarningDto>();

        foreach (var error in errors)
        {
            if (error.Severity == ParsingErrorSeverity.Error)
            {
                errorDtos.Add(error.ToErrorDto());
            }
            else
            {
                warningDtos.Add(error.ToWarningDto());
            }
        }

        return (errorDtos, warningDtos);
    }
}
