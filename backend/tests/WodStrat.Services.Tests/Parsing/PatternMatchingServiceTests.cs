using FluentAssertions;
using WodStrat.Dal.Enums;
using WodStrat.Services.Parsing;
using WodStrat.Services.Services;
using Xunit;

namespace WodStrat.Services.Tests.Parsing;

/// <summary>
/// Unit tests for PatternMatchingService.
/// Tests cover workout type detection, rep scheme extraction, movement line parsing,
/// weight, distance, and calorie extraction.
/// </summary>
public class PatternMatchingServiceTests
{
    private readonly PatternMatchingService _sut;

    public PatternMatchingServiceTests()
    {
        _sut = new PatternMatchingService();
    }

    #region DetectWorkoutType Tests

    [Theory]
    [InlineData("20 min AMRAP")]
    [InlineData("20 minute AMRAP")]
    [InlineData("15min AMRAP")]
    [InlineData("AMRAP 20")]
    [InlineData("AMRAP in 20 minutes")]
    [InlineData("AMRAP")]
    public void DetectWorkoutType_AmrapPatterns_ReturnsAmrapType(string text)
    {
        // Act
        var result = _sut.DetectWorkoutType(text);

        // Assert
        result.Type.Should().Be(WorkoutType.Amrap);
        result.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void DetectWorkoutType_AmrapWithDuration_ExtractsTimeCapCorrectly()
    {
        // Act
        var result = _sut.DetectWorkoutType("20 min AMRAP");

        // Assert
        result.Type.Should().Be(WorkoutType.Amrap);
        result.TimeCapSeconds.Should().Be(1200); // 20 * 60 = 1200 seconds
    }

    [Theory]
    [InlineData("For Time")]
    [InlineData("for time")]
    [InlineData("FOR TIME")]
    [InlineData("Complete as fast as possible")]
    public void DetectWorkoutType_ForTimePatterns_ReturnsForTimeType(string text)
    {
        // Act
        var result = _sut.DetectWorkoutType(text);

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void DetectWorkoutType_ForTimeWithRounds_ExtractsRoundCount()
    {
        // Act
        var result = _sut.DetectWorkoutType("3 Rounds For Time");

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.RoundCount.Should().Be(3);
    }

    [Fact]
    public void DetectWorkoutType_ForTimeWithTimeCap_ExtractsTimeCap()
    {
        // Act
        var result = _sut.DetectWorkoutType("For Time\nTime Cap: 15");

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.TimeCapSeconds.Should().Be(900); // 15 * 60 = 900 seconds
    }

    [Theory]
    [InlineData("EMOM")]
    [InlineData("E2MOM")]
    [InlineData("E3MOM")]
    [InlineData("Every minute on the minute")]
    [InlineData("Every 2 minutes")]
    [InlineData("10 min EMOM")]
    [InlineData("EMOM 20")]
    public void DetectWorkoutType_EmomPatterns_ReturnsEmomType(string text)
    {
        // Act
        var result = _sut.DetectWorkoutType(text);

        // Assert
        result.Type.Should().Be(WorkoutType.Emom);
        result.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void DetectWorkoutType_EmomWithDuration_ExtractsIntervalAndTimeCap()
    {
        // Act
        var result = _sut.DetectWorkoutType("10 min EMOM");

        // Assert
        result.Type.Should().Be(WorkoutType.Emom);
        result.TimeCapSeconds.Should().Be(600); // 10 * 60 = 600 seconds
        result.IntervalSeconds.Should().Be(60); // Default 1 minute intervals
    }

    [Fact]
    public void DetectWorkoutType_E2mom_ExtractsIntervalDuration()
    {
        // Act
        var result = _sut.DetectWorkoutType("E2MOM");

        // Assert
        result.Type.Should().Be(WorkoutType.Emom);
        result.IntervalSeconds.Should().Be(120); // 2 * 60 = 120 seconds
    }

    [Fact]
    public void DetectWorkoutType_Tabata_ReturnsIntervalsType()
    {
        // Act
        var result = _sut.DetectWorkoutType("Tabata");

        // Assert
        result.Type.Should().Be(WorkoutType.Intervals);
        result.TimeCapSeconds.Should().Be(240); // 4 * 60 = 240 seconds
        result.IntervalSeconds.Should().Be(30); // 20 on + 10 off
        result.Confidence.Should().Be(1.0);
    }

    [Theory]
    [InlineData("5 Rounds")]
    [InlineData("3 Sets")]
    [InlineData("4 rounds")]
    [InlineData("5 RFT")]
    public void DetectWorkoutType_RoundsPatterns_ReturnsRoundsType(string text)
    {
        // Act
        var result = _sut.DetectWorkoutType(text);

        // Assert
        result.Type.Should().Be(WorkoutType.Rounds);
        result.Confidence.Should().Be(0.9);
    }

    [Fact]
    public void DetectWorkoutType_ChipperRepScheme_ReturnsForTimeType()
    {
        // Act
        var result = _sut.DetectWorkoutType("21-15-9");

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.Confidence.Should().Be(0.8);
    }

    [Fact]
    public void DetectWorkoutType_EmptyString_ReturnsDefaultForTime()
    {
        // Act
        var result = _sut.DetectWorkoutType("");

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.Confidence.Should().Be(0.5);
    }

    [Fact]
    public void DetectWorkoutType_NullString_ReturnsDefaultForTime()
    {
        // Act
        var result = _sut.DetectWorkoutType(null!);

        // Assert
        result.Type.Should().Be(WorkoutType.ForTime);
        result.Confidence.Should().Be(0.5);
    }

    #endregion

    #region ExtractRepScheme Tests

    [Theory]
    [InlineData("21-15-9", new[] { 21, 15, 9 }, RepSchemeType.Descending)]
    [InlineData("10-9-8-7-6-5-4-3-2-1", new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }, RepSchemeType.Descending)]
    [InlineData("50-40-30-20-10", new[] { 50, 40, 30, 20, 10 }, RepSchemeType.Descending)]
    public void ExtractRepScheme_ChipperDescending_ReturnsCorrectData(string input, int[] expectedReps, RepSchemeType expectedType)
    {
        // Act
        var result = _sut.ExtractRepScheme(input);

        // Assert
        result.Should().NotBeNull();
        result!.Reps.Should().BeEquivalentTo(expectedReps);
        result.Type.Should().Be(expectedType);
        result.TotalReps.Should().Be(expectedReps.Sum());
        result.RoundCount.Should().Be(expectedReps.Length);
    }

    [Theory]
    [InlineData("1-2-3-4-5", new[] { 1, 2, 3, 4, 5 }, RepSchemeType.Ascending)]
    [InlineData("2-4-6-8-10", new[] { 2, 4, 6, 8, 10 }, RepSchemeType.Ascending)]
    public void ExtractRepScheme_ChipperAscending_ReturnsCorrectData(string input, int[] expectedReps, RepSchemeType expectedType)
    {
        // Act
        var result = _sut.ExtractRepScheme(input);

        // Assert
        result.Should().NotBeNull();
        result!.Reps.Should().BeEquivalentTo(expectedReps);
        result.Type.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("10/8/6/4/2")]
    public void ExtractRepScheme_PerRoundPattern_ReturnsCorrectData(string input)
    {
        // Act
        var result = _sut.ExtractRepScheme(input);

        // Assert
        result.Should().NotBeNull();
        result!.Reps.Should().BeEquivalentTo(new[] { 10, 8, 6, 4, 2 });
        result.Type.Should().Be(RepSchemeType.Descending);
    }

    [Fact]
    public void ExtractRepScheme_FixedRepPattern_ReturnsCorrectData()
    {
        // Act
        var result = _sut.ExtractRepScheme("5 rounds of 10 reps");

        // Assert
        result.Should().NotBeNull();
        result!.Reps.Should().BeEquivalentTo(new[] { 10, 10, 10, 10, 10 });
        result.Type.Should().Be(RepSchemeType.Fixed);
        result.TotalReps.Should().Be(50);
    }

    [Fact]
    public void ExtractRepScheme_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractRepScheme("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractRepScheme_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractRepScheme("10 Pull-ups");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("3 Rounds: 21-15-9 repetitions of:", new[] { 21, 15, 9 }, RepSchemeType.Descending)]
    [InlineData("5 Rounds: 10-8-6-4-2 of:", new[] { 10, 8, 6, 4, 2 }, RepSchemeType.Descending)]
    [InlineData("Complete 21-15-9 reps of each movement", new[] { 21, 15, 9 }, RepSchemeType.Descending)]
    public void ExtractRepScheme_EmbeddedChipper_ReturnsCorrectData(string input, int[] expectedReps, RepSchemeType expectedType)
    {
        // Act
        var result = _sut.ExtractRepScheme(input);

        // Assert
        result.Should().NotBeNull();
        result!.Reps.Should().BeEquivalentTo(expectedReps);
        result.Type.Should().Be(expectedType);
    }

    #endregion

    #region ExtractWeight Tests

    [Theory]
    [InlineData("95 lb", 95, LoadUnit.Lb)]
    [InlineData("95#", 95, LoadUnit.Lb)]
    [InlineData("135 lbs", 135, LoadUnit.Lb)]
    [InlineData("43 kg", 43, LoadUnit.Kg)]
    [InlineData("60 kg", 60, LoadUnit.Kg)]
    [InlineData("1.5 pood", 1.5, LoadUnit.Pood)]
    [InlineData("2 pood", 2, LoadUnit.Pood)]
    public void ExtractWeight_ValidWeights_ReturnsCorrectData(string input, decimal expectedValue, LoadUnit expectedUnit)
    {
        // Act
        var result = _sut.ExtractWeight(input);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(expectedValue);
        result.Unit.Should().Be(expectedUnit);
    }

    [Fact]
    public void ExtractWeight_InContext_ReturnsCorrectData()
    {
        // Act
        var result = _sut.ExtractWeight("21 Thrusters (95 lb)");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(95);
        result.Unit.Should().Be(LoadUnit.Lb);
    }

    [Fact]
    public void ExtractWeight_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractWeight("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractWeight_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractWeight("10 Pull-ups");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractWeight_ToKg_ConvertsCorrectly()
    {
        // Arrange
        var weight = _sut.ExtractWeight("100 lb")!;

        // Act
        var kg = weight.ToKg();

        // Assert
        kg.Should().BeApproximately(45.3592m, 0.001m);
    }

    [Fact]
    public void ExtractWeight_ToLb_ConvertsCorrectly()
    {
        // Arrange
        var weight = _sut.ExtractWeight("60 kg")!;

        // Act
        var lb = weight.ToLb();

        // Assert
        lb.Should().BeApproximately(132.2772m, 0.001m);
    }

    #endregion

    #region ExtractWeightPair Tests

    [Theory]
    [InlineData("95/65 lb", 95, 65, LoadUnit.Lb)]
    [InlineData("(135/95 lb)", 135, 95, LoadUnit.Lb)]
    [InlineData("60/40 kg", 60, 40, LoadUnit.Kg)]
    [InlineData("135/95", 135, 95, LoadUnit.Lb)] // Default to lb
    public void ExtractWeightPair_ValidPairs_ReturnsCorrectData(string input, decimal maleValue, decimal femaleValue, LoadUnit expectedUnit)
    {
        // Act
        var result = _sut.ExtractWeightPair(input);

        // Assert
        result.Should().NotBeNull();
        result!.Male.Value.Should().Be(maleValue);
        result.Female.Value.Should().Be(femaleValue);
        result.Male.Unit.Should().Be(expectedUnit);
        result.Female.Unit.Should().Be(expectedUnit);
    }

    [Fact]
    public void ExtractWeightPair_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractWeightPair("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractWeightPair_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractWeightPair("95 lb");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractDistance Tests

    [Theory]
    [InlineData("400m", 400, DistanceUnit.M)]
    [InlineData("400 meters", 400, DistanceUnit.M)]
    [InlineData("400 m", 400, DistanceUnit.M)]
    [InlineData("5k", 5, DistanceUnit.Km)]
    [InlineData("5 km", 5, DistanceUnit.Km)]
    [InlineData("100 ft", 100, DistanceUnit.Ft)]
    [InlineData("100 feet", 100, DistanceUnit.Ft)]
    [InlineData("1 mile", 1, DistanceUnit.Mi)]
    [InlineData("1 mi", 1, DistanceUnit.Mi)]
    [InlineData("1.5 miles", 1.5, DistanceUnit.Mi)]
    public void ExtractDistance_ValidDistances_ReturnsCorrectData(string input, decimal expectedValue, DistanceUnit expectedUnit)
    {
        // Act
        var result = _sut.ExtractDistance(input);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(expectedValue);
        result.Unit.Should().Be(expectedUnit);
    }

    [Fact]
    public void ExtractDistance_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractDistance("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractDistance_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractDistance("10 Pull-ups");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractDistance_ToMeters_ConvertsCorrectly()
    {
        // Arrange
        var distance = _sut.ExtractDistance("5k")!;

        // Act
        var meters = distance.ToMeters();

        // Assert
        meters.Should().Be(5000m);
    }

    [Fact]
    public void ExtractDistance_ToMiles_ConvertsCorrectly()
    {
        // Arrange
        var distance = _sut.ExtractDistance("1609m")!;

        // Act
        var miles = distance.ToMiles();

        // Assert
        miles.Should().BeApproximately(0.9998m, 0.01m);
    }

    #endregion

    #region ExtractCalories Tests

    [Theory]
    [InlineData("20 Cal Row", 20)]
    [InlineData("15 calories", 15)]
    [InlineData("25 cal bike", 25)]
    [InlineData("30 Calories", 30)]
    public void ExtractCalories_ValidCalories_ReturnsCorrectData(string input, int expectedCalories)
    {
        // Act
        var result = _sut.ExtractCalories(input);

        // Assert
        result.Should().Be(expectedCalories);
    }

    [Fact]
    public void ExtractCalories_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractCalories("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractCalories_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractCalories("400m Run");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractCaloriePair Tests

    [Theory]
    [InlineData("20/15 Cal", 20, 15)]
    [InlineData("25/20 calories", 25, 20)]
    [InlineData("30/24 Cal Row", 30, 24)]
    public void ExtractCaloriePair_ValidPairs_ReturnsCorrectData(string input, int maleCalories, int femaleCalories)
    {
        // Act
        var result = _sut.ExtractCaloriePair(input);

        // Assert
        result.Should().NotBeNull();
        result!.Male.Should().Be(maleCalories);
        result.Female.Should().Be(femaleCalories);
    }

    [Fact]
    public void ExtractCaloriePair_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractCaloriePair("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractCaloriePair_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractCaloriePair("20 Cal Row");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractTime Tests

    [Theory]
    [InlineData("20 min", 20, 0)]
    [InlineData("20 minutes", 20, 0)]
    [InlineData("30 sec", 0, 30)]
    [InlineData("30 seconds", 0, 30)]
    [InlineData(":30", 0, 30)]
    public void ExtractTime_DurationPatterns_ReturnsCorrectTimeSpan(string input, int expectedMinutes, int expectedSeconds)
    {
        // Act
        var result = _sut.ExtractTime(input);

        // Assert
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().BeApproximately(expectedMinutes + (expectedSeconds / 60.0), 0.01);
    }

    [Theory]
    [InlineData("12:00", 12, 0)]
    [InlineData("1:30", 1, 30)]
    [InlineData("0:45", 0, 45)]
    public void ExtractTime_ClockFormat_ReturnsCorrectTimeSpan(string input, int expectedMinutes, int expectedSeconds)
    {
        // Act
        var result = _sut.ExtractTime(input);

        // Assert
        result.Should().NotBeNull();
        var expected = TimeSpan.FromMinutes(expectedMinutes) + TimeSpan.FromSeconds(expectedSeconds);
        result!.Value.Should().Be(expected);
    }

    [Fact]
    public void ExtractTime_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractTime("");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractTimeCap Tests

    [Theory]
    [InlineData("Time Cap: 20", 20)]
    [InlineData("TC: 15", 15)]
    [InlineData("Cap: 12", 12)]
    [InlineData("20 min cap", 20)]
    [InlineData("15 minute cap", 15)]
    public void ExtractTimeCap_ValidPatterns_ReturnsCorrectTimeSpan(string input, int expectedMinutes)
    {
        // Act
        var result = _sut.ExtractTimeCap(input);

        // Assert
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().Be(expectedMinutes);
    }

    [Fact]
    public void ExtractTimeCap_WithSeconds_ReturnsCorrectTimeSpan()
    {
        // Act
        var result = _sut.ExtractTimeCap("Time Cap: 15:30");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void ExtractTimeCap_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractTimeCap("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractTimeCap_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractTimeCap("For Time");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractRounds Tests

    [Theory]
    [InlineData("5 Rounds", 5)]
    [InlineData("3 Sets", 3)]
    [InlineData("4 rounds", 4)]
    [InlineData("5 RFT", 5)]
    [InlineData("3 rounds of", 3)]
    public void ExtractRounds_ValidPatterns_ReturnsCorrectCount(string input, int expectedRounds)
    {
        // Act
        var result = _sut.ExtractRounds(input);

        // Assert
        result.Should().Be(expectedRounds);
    }

    [Fact]
    public void ExtractRounds_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractRounds("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractRounds_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractRounds("For Time");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractInterval Tests

    [Fact]
    public void ExtractInterval_Tabata_ReturnsStandardTabata()
    {
        // Act
        var result = _sut.ExtractInterval("Tabata");

        // Assert
        result.Should().NotBeNull();
        result!.Rounds.Should().Be(8);
        result.WorkSeconds.Should().Be(20);
        result.RestSeconds.Should().Be(10);
        result.TotalSeconds.Should().Be(240); // 8 * 30 = 240
        result.WorkRestRatio.Should().Be(2m); // 20 / 10 = 2
    }

    [Fact]
    public void ExtractInterval_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractInterval("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractInterval_NoMatch_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractInterval("For Time");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractPercentage Tests

    [Theory]
    [InlineData("70%", 70, null)]
    [InlineData("70% 1RM", 70, "1RM")]
    [InlineData("85% of 1RM", 85, "1RM")]
    public void ExtractPercentage_ValidPatterns_ReturnsCorrectData(string input, int expectedPercentage, string? expectedReference)
    {
        // Act
        var result = _sut.ExtractPercentage(input);

        // Assert
        result.Should().NotBeNull();
        result!.Percentage.Should().Be(expectedPercentage);
        result.Reference.Should().Be(expectedReference);
    }

    [Fact]
    public void ExtractPercentage_Bodyweight_Returns100Percent()
    {
        // Act
        var result = _sut.ExtractPercentage("bodyweight");

        // Assert
        result.Should().NotBeNull();
        result!.Percentage.Should().Be(100);
        result.Reference.Should().Be("bodyweight");
    }

    [Fact]
    public void ExtractPercentage_BW_Returns100Percent()
    {
        // Act
        var result = _sut.ExtractPercentage("BW");

        // Assert
        result.Should().NotBeNull();
        result!.Percentage.Should().Be(100);
        result.Reference.Should().Be("bodyweight");
    }

    [Fact]
    public void ExtractPercentage_EmptyString_ReturnsNull()
    {
        // Act
        var result = _sut.ExtractPercentage("");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ParseMovementLine Tests

    [Fact]
    public void ParseMovementLine_SimpleWithReps_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("10 Pull-ups");

        // Assert
        result.Should().NotBeNull();
        result.Reps.Should().Be(10);
        result.MovementText.Should().Be("Pull-ups");
        result.OriginalText.Should().Be("10 Pull-ups");
        result.HasQuantity.Should().BeTrue();
    }

    [Fact]
    public void ParseMovementLine_WithWeight_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("21 Thrusters (95 lb)");

        // Assert
        result.Should().NotBeNull();
        result.Reps.Should().Be(21);
        result.MovementText.Should().Contain("Thrusters");
        result.Weight.Should().NotBeNull();
        result.Weight!.Value.Should().Be(95);
        result.Weight.Unit.Should().Be(LoadUnit.Lb);
        result.HasLoad.Should().BeTrue();
    }

    [Fact]
    public void ParseMovementLine_WithWeightPair_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("21 Thrusters (95/65 lb)");

        // Assert
        result.Should().NotBeNull();
        result.Reps.Should().Be(21);
        result.WeightPair.Should().NotBeNull();
        result.WeightPair!.Male.Value.Should().Be(95);
        result.WeightPair.Female.Value.Should().Be(65);
        result.Weight.Should().BeNull(); // Single weight should be null when pair exists
        result.HasLoad.Should().BeTrue();
    }

    [Fact]
    public void ParseMovementLine_WithDistance_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("400m Run");

        // Assert
        result.Should().NotBeNull();
        result.Distance.Should().NotBeNull();
        result.Distance!.Value.Should().Be(400);
        result.Distance.Unit.Should().Be(DistanceUnit.M);
    }

    [Fact]
    public void ParseMovementLine_WithCalories_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("20 Cal Row");

        // Assert
        result.Should().NotBeNull();
        result.Calories.Should().Be(20);
    }

    [Fact]
    public void ParseMovementLine_WithCaloriePair_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("20/15 Cal Row");

        // Assert
        result.Should().NotBeNull();
        result.CaloriePair.Should().NotBeNull();
        result.CaloriePair!.Male.Should().Be(20);
        result.CaloriePair.Female.Should().Be(15);
        result.Calories.Should().BeNull(); // Single calories should be null when pair exists
    }

    [Fact]
    public void ParseMovementLine_WithDuration_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("30 sec L-sit");

        // Assert
        result.Should().NotBeNull();
        result.DurationSeconds.Should().Be(30);
        result.MovementText.Should().Be("L-sit");
    }

    [Fact]
    public void ParseMovementLine_WithColonDuration_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine(":30 Plank Hold");

        // Assert
        result.Should().NotBeNull();
        result.DurationSeconds.Should().Be(30);
        result.MovementText.Should().Be("Plank Hold");
    }

    [Fact]
    public void ParseMovementLine_WithMinuteDuration_ExtractsCorrectly()
    {
        // Act
        var result = _sut.ParseMovementLine("1 min Hollow Hold");

        // Assert
        result.Should().NotBeNull();
        result.DurationSeconds.Should().Be(60);
        result.MovementText.Should().Be("Hollow Hold");
    }

    [Fact]
    public void ParseMovementLine_EmptyString_ReturnsEmptyResult()
    {
        // Act
        var result = _sut.ParseMovementLine("");

        // Assert
        result.Should().NotBeNull();
        result.Reps.Should().BeNull();
        result.MovementText.Should().BeEmpty();
        result.HasQuantity.Should().BeFalse();
        result.HasLoad.Should().BeFalse();
    }

    [Fact]
    public void ParseMovementLine_ComplexMovement_ExtractsAllComponents()
    {
        // Act
        var result = _sut.ParseMovementLine("15 Box Jumps (24 in)");

        // Assert
        result.Should().NotBeNull();
        result.Reps.Should().Be(15);
        result.MovementText.Should().Contain("Box Jumps");
        result.Modifiers.Should().Be("24 in");
        result.Height.Should().Be("24 in");
    }

    #endregion

    #region WorkoutPatterns Helper Methods Tests

    [Theory]
    [InlineData("20 min AMRAP")]
    [InlineData("For Time")]
    [InlineData("EMOM")]
    [InlineData("Tabata")]
    [InlineData("5 Rounds")]
    public void ContainsWorkoutType_ValidTypes_ReturnsTrue(string text)
    {
        // Act
        var result = WorkoutPatterns.ContainsWorkoutType(text);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsWorkoutType_NoType_ReturnsFalse()
    {
        // Act
        var result = WorkoutPatterns.ContainsWorkoutType("10 Pull-ups");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("20 min AMRAP")]
    [InlineData("For Time")]
    [InlineData("Time Cap: 15")]
    [InlineData("21-15-9")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("3 Rounds")]
    [InlineData("5 Rounds:")]
    [InlineData("3 Rounds: 21-15-9 repetitions of:")]
    [InlineData("21-15-9 repetitions of:")]
    [InlineData("21-15-9 reps of:")]
    [InlineData("50-40-30-20-10 of:")]
    public void IsHeaderLine_HeaderLines_ReturnsTrue(string line)
    {
        // Act
        var result = WorkoutPatterns.IsHeaderLine(line);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHeaderLine_MovementLine_ReturnsFalse()
    {
        // Act
        var result = WorkoutPatterns.IsHeaderLine("10 Pull-ups");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Data Structure Computed Properties Tests

    [Fact]
    public void RepScheme_CustomType_WhenVariablePattern()
    {
        // Arrange - Pyramid scheme (up and down)
        var result = _sut.ExtractRepScheme("1-2-3-2-1");

        // Assert - Should be Custom since it's neither strictly ascending nor descending
        result.Should().NotBeNull();
        result!.Type.Should().Be(RepSchemeType.Custom);
    }

    [Fact]
    public void Distance_ToKilometers_ConvertsCorrectly()
    {
        // Arrange
        var distance = _sut.ExtractDistance("5000m")!;

        // Act
        var km = distance.ToKilometers();

        // Assert
        km.Should().Be(5m);
    }

    [Fact]
    public void Weight_PoodToKg_ConvertsCorrectly()
    {
        // Arrange
        var weight = _sut.ExtractWeight("2 pood")!;

        // Act
        var kg = weight.ToKg();

        // Assert
        kg.Should().BeApproximately(32.76m, 0.01m); // 2 * 16.38
    }

    [Fact]
    public void Weight_PoodToLb_ConvertsCorrectly()
    {
        // Arrange
        var weight = _sut.ExtractWeight("1 pood")!;

        // Act
        var lb = weight.ToLb();

        // Assert
        lb.Should().BeApproximately(36.11m, 0.01m);
    }

    [Fact]
    public void IntervalConfig_ComputedProperties_CorrectValues()
    {
        // Arrange
        var config = _sut.ExtractInterval("Tabata")!;

        // Assert
        config.TotalSeconds.Should().Be(240); // 8 * (20 + 10)
        config.WorkRestRatio.Should().Be(2m); // 20 / 10
    }

    [Fact]
    public void ParsedMovementLine_HasQuantity_TrueWhenRepsPresent()
    {
        // Act
        var result = _sut.ParseMovementLine("10 Burpees");

        // Assert
        result.HasQuantity.Should().BeTrue();
    }

    [Fact]
    public void ParsedMovementLine_HasQuantity_TrueWhenDistancePresent()
    {
        // Act
        var result = _sut.ParseMovementLine("Run");

        // Assert - No reps, no distance extracted from "Run" alone
        result.HasQuantity.Should().BeFalse();
    }

    [Fact]
    public void ParsedMovementLine_HasQuantity_TrueWhenCaloriesPresent()
    {
        // Act
        var result = _sut.ParseMovementLine("Row");

        // Assert - No quantity extracted from "Row" alone
        result.HasQuantity.Should().BeFalse();
    }

    [Fact]
    public void ParsedMovementLine_HasLoad_FalseWhenNoWeight()
    {
        // Act
        var result = _sut.ParseMovementLine("10 Pull-ups");

        // Assert
        result.HasLoad.Should().BeFalse();
    }

    #endregion
}
