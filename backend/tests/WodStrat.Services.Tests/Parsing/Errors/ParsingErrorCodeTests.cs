using FluentAssertions;
using System.ComponentModel;
using System.Reflection;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for ParsingErrorCode enum.
/// Tests cover error code categorization, descriptions, and value ranges.
/// </summary>
public class ParsingErrorCodeTests
{
    #region Error Code Categorization Tests

    [Theory]
    [InlineData(ParsingErrorCode.EmptyInput, 100)]
    [InlineData(ParsingErrorCode.InputTooLong, 101)]
    [InlineData(ParsingErrorCode.InputTooShort, 102)]
    [InlineData(ParsingErrorCode.BinaryContent, 103)]
    [InlineData(ParsingErrorCode.InvalidCharacters, 104)]
    public void InputValidationErrors_ShouldBeInThe100Range(ParsingErrorCode code, int expectedValue)
    {
        // Assert
        ((int)code).Should().Be(expectedValue);
        ((int)code).Should().BeInRange(100, 199);
    }

    [Theory]
    [InlineData(ParsingErrorCode.NoWorkoutStructure, 200)]
    [InlineData(ParsingErrorCode.NoMovementsDetected, 201)]
    [InlineData(ParsingErrorCode.InvalidWorkoutType, 202)]
    [InlineData(ParsingErrorCode.AmbiguousWorkoutType, 203)]
    [InlineData(ParsingErrorCode.MissingDuration, 204)]
    [InlineData(ParsingErrorCode.MissingRoundCount, 205)]
    [InlineData(ParsingErrorCode.ContradictoryMetadata, 206)]
    public void StructuralErrors_ShouldBeInThe200Range(ParsingErrorCode code, int expectedValue)
    {
        // Assert
        ((int)code).Should().Be(expectedValue);
        ((int)code).Should().BeInRange(200, 299);
    }

    [Theory]
    [InlineData(ParsingErrorCode.UnknownMovement, 300)]
    [InlineData(ParsingErrorCode.AmbiguousMovement, 301)]
    [InlineData(ParsingErrorCode.InvalidRepCount, 302)]
    [InlineData(ParsingErrorCode.InvalidWeight, 303)]
    [InlineData(ParsingErrorCode.InvalidDistance, 304)]
    [InlineData(ParsingErrorCode.InvalidTime, 305)]
    [InlineData(ParsingErrorCode.InvalidCalories, 306)]
    [InlineData(ParsingErrorCode.EmptyMovementLine, 307)]
    [InlineData(ParsingErrorCode.UnrecognizedMovementFormat, 308)]
    public void MovementParsingErrors_ShouldBeInThe300Range(ParsingErrorCode code, int expectedValue)
    {
        // Assert
        ((int)code).Should().Be(expectedValue);
        ((int)code).Should().BeInRange(300, 399);
    }

    [Theory]
    [InlineData(ParsingErrorCode.DuplicateMovement, 400)]
    [InlineData(ParsingErrorCode.InconsistentUnits, 401)]
    [InlineData(ParsingErrorCode.ValueOutOfRange, 402)]
    public void DataConsistencyErrors_ShouldBeInThe400Range(ParsingErrorCode code, int expectedValue)
    {
        // Assert
        ((int)code).Should().Be(expectedValue);
        ((int)code).Should().BeInRange(400, 499);
    }

    [Theory]
    [InlineData(ParsingErrorCode.InternalError, 500)]
    [InlineData(ParsingErrorCode.Timeout, 501)]
    public void SystemErrors_ShouldBeInThe500Range(ParsingErrorCode code, int expectedValue)
    {
        // Assert
        ((int)code).Should().Be(expectedValue);
        ((int)code).Should().BeInRange(500, 599);
    }

    #endregion

    #region Description Attribute Tests

    [Fact]
    public void AllErrorCodes_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var errorCodes = Enum.GetValues<ParsingErrorCode>();

        // Act & Assert
        foreach (var code in errorCodes)
        {
            var memberInfo = typeof(ParsingErrorCode).GetMember(code.ToString()).FirstOrDefault();
            var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();

            descriptionAttribute.Should().NotBeNull(
                $"Error code {code} should have a Description attribute");
            descriptionAttribute!.Description.Should().NotBeNullOrWhiteSpace(
                $"Error code {code} should have a non-empty description");
        }
    }

    [Theory]
    [InlineData(ParsingErrorCode.EmptyInput, "Empty input")]
    [InlineData(ParsingErrorCode.UnknownMovement, "Unknown movement")]
    [InlineData(ParsingErrorCode.InvalidRepCount, "Invalid rep count")]
    [InlineData(ParsingErrorCode.NoMovementsDetected, "No movements detected")]
    public void SpecificErrorCodes_ShouldHaveExpectedDescriptions(ParsingErrorCode code, string expectedDescription)
    {
        // Arrange
        var memberInfo = typeof(ParsingErrorCode).GetMember(code.ToString()).FirstOrDefault();
        var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();

        // Assert
        descriptionAttribute.Should().NotBeNull();
        descriptionAttribute!.Description.Should().Be(expectedDescription);
    }

    #endregion

    #region Uniqueness Tests

    [Fact]
    public void AllErrorCodes_ShouldHaveUniqueValues()
    {
        // Arrange
        var errorCodes = Enum.GetValues<ParsingErrorCode>();
        var values = errorCodes.Cast<int>().ToList();

        // Assert
        values.Should().OnlyHaveUniqueItems(
            "Each error code should have a unique numeric value");
    }

    #endregion
}
