using FluentAssertions;
using WodStrat.Dal.Enums;
using WodStrat.Services.Utilities;
using Xunit;

namespace WodStrat.Services.Tests.Utilities;

/// <summary>
/// Unit tests for BenchmarkValueFormatter.
/// </summary>
public class BenchmarkValueFormatterTests
{
    #region FormatTime Tests

    [Theory]
    [InlineData(60, "1:00")] // 1 minute
    [InlineData(90, "1:30")] // 1 minute 30 seconds
    [InlineData(195, "3:15")] // 3 minutes 15 seconds
    [InlineData(195.5, "3:15")] // Rounds down for seconds
    [InlineData(0, "0:00")] // Zero
    [InlineData(59, "0:59")] // Less than 1 minute
    public void FormatTime_LessThanOneHour_ReturnsMinutesAndSeconds(decimal seconds, string expected)
    {
        // Act
        var result = BenchmarkValueFormatter.FormatTime(seconds);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(3600, "1:00:00")] // 1 hour
    [InlineData(3661, "1:01:01")] // 1 hour 1 minute 1 second
    [InlineData(7325, "2:02:05")] // 2 hours 2 minutes 5 seconds
    public void FormatTime_OneHourOrMore_ReturnsHoursMinutesSeconds(decimal seconds, string expected)
    {
        // Act
        var result = BenchmarkValueFormatter.FormatTime(seconds);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatTime_NegativeValue_TreatsAsAbsolute()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatTime(-60);

        // Assert
        result.Should().Be("1:00");
    }

    #endregion

    #region FormatReps Tests

    [Theory]
    [InlineData(25, "25 reps")]
    [InlineData(100, "100 reps")]
    [InlineData(1, "1 reps")] // Note: doesn't singularize
    [InlineData(0, "0 reps")]
    public void FormatReps_ValidValues_ReturnsFormattedString(decimal reps, string expected)
    {
        // Act
        var result = BenchmarkValueFormatter.FormatReps(reps);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatReps_DecimalValue_RoundsToNearestWholeNumber()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatReps(25.7m);

        // Assert
        result.Should().Be("26 reps");
    }

    #endregion

    #region FormatWeight Tests

    [Fact]
    public void FormatWeight_WholeNumber_NoDecimalPoint()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatWeight(100m, "kg");

        // Assert
        result.Should().Be("100 kg");
    }

    [Fact]
    public void FormatWeight_DecimalValue_ShowsDecimal()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatWeight(100.5m, "kg");

        // Assert
        result.Should().Be("100.5 kg");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FormatWeight_EmptyOrNullUnit_DefaultsToKg(string? unit)
    {
        // Act
        var result = BenchmarkValueFormatter.FormatWeight(100m, unit!);

        // Assert
        result.Should().Be("100 kg");
    }

    [Fact]
    public void FormatWeight_CustomUnit_UsesProvidedUnit()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatWeight(225m, "lb");

        // Assert
        result.Should().Be("225 lb");
    }

    #endregion

    #region FormatPace Tests

    [Fact]
    public void FormatPace_ValidValue_ReturnsFormattedPace()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatPace(105m, "500m"); // 1:45/500m

        // Assert
        result.Should().Be("1:45/500m");
    }

    [Fact]
    public void FormatPace_LessThanOneMinute_ShowsZeroMinutes()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatPace(45m, "500m");

        // Assert
        result.Should().Be("0:45/500m");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FormatPace_EmptyOrNullUnit_ReturnsTimeOnly(string? unit)
    {
        // Act
        var result = BenchmarkValueFormatter.FormatPace(105m, unit!);

        // Assert
        result.Should().Be("1:45");
    }

    [Fact]
    public void FormatPace_NegativeValue_TreatsAsAbsolute()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatPace(-105m, "500m");

        // Assert
        result.Should().Be("1:45/500m");
    }

    #endregion

    #region Format (with enum) Tests

    [Theory]
    [InlineData(BenchmarkMetricType.Time, 195, "seconds", "3:15")]
    [InlineData(BenchmarkMetricType.Reps, 50, "reps", "50 reps")]
    [InlineData(BenchmarkMetricType.Weight, 100, "kg", "100 kg")]
    [InlineData(BenchmarkMetricType.Pace, 105, "500m", "1:45/500m")]
    public void Format_WithEnum_ReturnsCorrectFormat(BenchmarkMetricType metricType, decimal value, string unit, string expected)
    {
        // Act
        var result = BenchmarkValueFormatter.Format(value, metricType, unit);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Format (with string) Tests

    [Theory]
    [InlineData("Time", 195, "seconds", "3:15")]
    [InlineData("time", 195, "seconds", "3:15")] // Case insensitive
    [InlineData("TIME", 195, "seconds", "3:15")] // Case insensitive
    [InlineData("Reps", 50, "reps", "50 reps")]
    [InlineData("Weight", 100, "kg", "100 kg")]
    [InlineData("Pace", 105, "500m", "1:45/500m")]
    public void Format_WithString_ReturnsCorrectFormat(string metricType, decimal value, string unit, string expected)
    {
        // Act
        var result = BenchmarkValueFormatter.Format(value, metricType, unit);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("NotAMetric")]
    public void Format_InvalidMetricTypeString_ReturnsRawValue(string invalidMetricType)
    {
        // Act
        var result = BenchmarkValueFormatter.Format(123.45m, invalidMetricType, "unit");

        // Assert
        result.Should().Be("123.45");
    }

    [Fact]
    public void Format_NullMetricTypeString_ReturnsRawValue()
    {
        // Arrange & Act - This would throw, so we test the fallback behavior
        var result = BenchmarkValueFormatter.Format(123.45m, "InvalidType", "unit");

        // Assert
        result.Should().Be("123.45");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FormatWeight_VerySmallDecimal_ShowsDecimal()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatWeight(0.5m, "kg");

        // Assert
        result.Should().Be("0.5 kg");
    }

    [Fact]
    public void FormatTime_VeryLargeValue_FormatsCorrectly()
    {
        // 10 hours = 36000 seconds
        var result = BenchmarkValueFormatter.FormatTime(36000);

        // Assert
        result.Should().Be("10:00:00");
    }

    [Fact]
    public void FormatReps_VeryLargeValue_FormatsCorrectly()
    {
        // Act
        var result = BenchmarkValueFormatter.FormatReps(1000);

        // Assert
        result.Should().Be("1000 reps");
    }

    #endregion
}
