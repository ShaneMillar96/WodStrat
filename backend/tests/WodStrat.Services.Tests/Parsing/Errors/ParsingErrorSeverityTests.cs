using FluentAssertions;
using System.ComponentModel;
using System.Reflection;
using WodStrat.Services.Parsing.Errors;
using Xunit;

namespace WodStrat.Services.Tests.Parsing.Errors;

/// <summary>
/// Unit tests for ParsingErrorSeverity enum.
/// Tests cover severity levels, ordering, and descriptions.
/// </summary>
public class ParsingErrorSeverityTests
{
    #region Value Tests

    [Fact]
    public void Error_ShouldBeValue0()
    {
        // Assert
        ((int)ParsingErrorSeverity.Error).Should().Be(0);
    }

    [Fact]
    public void Warning_ShouldBeValue1()
    {
        // Assert
        ((int)ParsingErrorSeverity.Warning).Should().Be(1);
    }

    [Fact]
    public void Info_ShouldBeValue2()
    {
        // Assert
        ((int)ParsingErrorSeverity.Info).Should().Be(2);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public void Error_ShouldBeMoreSevereThanWarning()
    {
        // Assert - Error (0) should have a lower value than Warning (1), indicating higher severity
        ((int)ParsingErrorSeverity.Error).Should().BeLessThan((int)ParsingErrorSeverity.Warning);
    }

    [Fact]
    public void Warning_ShouldBeMoreSevereThanInfo()
    {
        // Assert - Warning (1) should have a lower value than Info (2), indicating higher severity
        ((int)ParsingErrorSeverity.Warning).Should().BeLessThan((int)ParsingErrorSeverity.Info);
    }

    [Fact]
    public void SortingBySeverity_ShouldPlaceErrorsFirst()
    {
        // Arrange
        var severities = new[]
        {
            ParsingErrorSeverity.Info,
            ParsingErrorSeverity.Error,
            ParsingErrorSeverity.Warning,
            ParsingErrorSeverity.Error,
            ParsingErrorSeverity.Info
        };

        // Act
        var sorted = severities.OrderBy(s => s).ToList();

        // Assert
        sorted[0].Should().Be(ParsingErrorSeverity.Error);
        sorted[1].Should().Be(ParsingErrorSeverity.Error);
        sorted[2].Should().Be(ParsingErrorSeverity.Warning);
        sorted[3].Should().Be(ParsingErrorSeverity.Info);
        sorted[4].Should().Be(ParsingErrorSeverity.Info);
    }

    #endregion

    #region Description Attribute Tests

    [Fact]
    public void AllSeverityLevels_ShouldHaveDescriptionAttribute()
    {
        // Arrange
        var severities = Enum.GetValues<ParsingErrorSeverity>();

        // Act & Assert
        foreach (var severity in severities)
        {
            var memberInfo = typeof(ParsingErrorSeverity).GetMember(severity.ToString()).FirstOrDefault();
            var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();

            descriptionAttribute.Should().NotBeNull(
                $"Severity {severity} should have a Description attribute");
        }
    }

    [Theory]
    [InlineData(ParsingErrorSeverity.Error, "Error")]
    [InlineData(ParsingErrorSeverity.Warning, "Warning")]
    [InlineData(ParsingErrorSeverity.Info, "Info")]
    public void SeverityLevels_ShouldHaveExpectedDescriptions(ParsingErrorSeverity severity, string expectedDescription)
    {
        // Arrange
        var memberInfo = typeof(ParsingErrorSeverity).GetMember(severity.ToString()).FirstOrDefault();
        var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();

        // Assert
        descriptionAttribute.Should().NotBeNull();
        descriptionAttribute!.Description.Should().Be(expectedDescription);
    }

    #endregion

    #region Completeness Tests

    [Fact]
    public void SeverityEnum_ShouldHaveExactlyThreeLevels()
    {
        // Arrange
        var severities = Enum.GetValues<ParsingErrorSeverity>();

        // Assert
        severities.Should().HaveCount(3,
            "ParsingErrorSeverity should have exactly three levels: Error, Warning, Info");
    }

    #endregion
}
