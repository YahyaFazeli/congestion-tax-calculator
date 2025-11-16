using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.Services;

public class TaxCalculatorTests
{
    private readonly TaxCalculator _calculator = new();

    [Fact]
    public void Calculate_ExemptVehicle_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Motorbike);
        var timestamps = new[] { new DateTime(2013, 2, 7, 8, 0, 0) };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Theory]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Military)]
    [InlineData(VehicleType.Foreign)]
    public void Calculate_AllExemptVehicleTypes_ReturnsZero(VehicleType type)
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", type);
        var timestamps = new[] { new DateTime(2013, 2, 7, 8, 0, 0) };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_EmptyTimestamps_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = Array.Empty<DateTime>();

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_SingleTimestamp_ChargesOnce()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 2, 7, 6, 23, 27) };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(8);
    }

    [Fact]
    public void Calculate_Saturday_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 2, 9, 8, 0, 0) }; // Saturday

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_Sunday_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 2, 10, 8, 0, 0) }; // Sunday

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_JulyDate_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 7, 15, 8, 0, 0) };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_PublicHoliday_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 3, 29, 8, 0, 0) }; // Good Friday

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_DayBeforePublicHoliday_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 3, 28, 14, 7, 27) }; // Day before Good Friday

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_OutsideTollHours_ReturnsZero()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 1, 14, 21, 0, 0),
            new DateTime(2013, 1, 15, 21, 0, 0),
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Should().Be(Money.Zero);
    }

    [Fact]
    public void Calculate_TwoPassesWithin60Minutes_ChargesHighestFeeOnce()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 6, 20, 27), // 8 SEK
            new DateTime(2013, 2, 8, 6, 27, 0), // 8 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(8); // Only charged once
    }

    [Fact]
    public void Calculate_ThreePassesWithin60Minutes_ChargesHighestFeeOnce()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 15, 29, 0), // 13 SEK
            new DateTime(2013, 2, 8, 15, 47, 0), // 18 SEK
            new DateTime(2013, 2, 8, 16, 1, 0), // 18 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(18); // Highest fee in window
    }

    [Fact]
    public void Calculate_TwoPassesOver60MinutesApart_ChargesBoth()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 16, 1, 0), // 18 SEK
            new DateTime(2013, 2, 8, 17, 2, 0), // 13 SEK (61 minutes later)
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(31); // Both charged
    }

    [Fact]
    public void Calculate_DailyMaxExceeded_CapsAtDailyMax()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 8, 6, 20, 27),
            new DateTime(2013, 2, 8, 6, 27, 0),
            new DateTime(2013, 2, 8, 14, 35, 0),
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 1, 0),
            new DateTime(2013, 2, 8, 16, 48, 0),
            new DateTime(2013, 2, 8, 17, 49, 0),
            new DateTime(2013, 2, 8, 18, 29, 0),
            new DateTime(2013, 2, 8, 18, 35, 0),
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(60); // Capped at daily max
    }

    [Fact]
    public void Calculate_BelowDailyMax_ChargesActualTotal()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 23, 27), // 8 SEK
            new DateTime(2013, 2, 7, 15, 27, 0), // 13 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(21);
    }

    [Fact]
    public void Calculate_MultipleDays_CalculatesPerDayIndependently()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 23, 27), // Day 1: 8 SEK
            new DateTime(2013, 2, 7, 15, 27, 0), // Day 1: 13 SEK
            new DateTime(2013, 3, 26, 14, 25, 0), // Day 2: 8 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(29); // 21 + 8
    }

    [Fact]
    public void Calculate_MultipleDaysWithCapping_CapsEachDayIndependently()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            // Day 1 - exceeds cap
            new DateTime(2013, 2, 7, 7, 0, 0), // 18 SEK
            new DateTime(2013, 2, 7, 9, 0, 0), // 8 SEK (over 60 min from first)
            new DateTime(2013, 2, 7, 15, 30, 0), // 18 SEK
            new DateTime(2013, 2, 7, 17, 0, 0), // 13 SEK (over 60 min from 15:30)
            new DateTime(2013, 2, 7, 18, 15, 0), // 8 SEK (over 60 min from 17:00)
            // Day 2 - below cap
            new DateTime(2013, 2, 11, 7, 0, 0), // 18 SEK
            new DateTime(2013, 2, 11, 15, 30, 0), // 18 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(96); // 60 (capped) + 36
    }

    [Fact]
    public void Calculate_UnsortedTimestamps_SortsAndCalculatesCorrectly()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 15, 27, 0), // 13 SEK
            new DateTime(2013, 2, 7, 6, 23, 27), // 8 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(21);
    }

    [Fact]
    public void Calculate_AssignmentTestData_ReturnsExpectedTotal()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 1, 14, 21, 0, 0), // Monday 21:00 → 0 SEK
            new DateTime(2013, 1, 15, 21, 0, 0), // Tuesday 21:00 → 0 SEK
            new DateTime(2013, 2, 7, 6, 23, 27), // Thursday → 21 SEK total
            new DateTime(2013, 2, 7, 15, 27, 0),
            new DateTime(2013, 2, 8, 6, 27, 0), // Friday → 60 SEK (capped)
            new DateTime(2013, 2, 8, 6, 20, 27),
            new DateTime(2013, 2, 8, 14, 35, 0),
            new DateTime(2013, 2, 8, 15, 29, 0),
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 1, 0),
            new DateTime(2013, 2, 8, 16, 48, 0),
            new DateTime(2013, 2, 8, 17, 49, 0),
            new DateTime(2013, 2, 8, 18, 29, 0),
            new DateTime(2013, 2, 8, 18, 35, 0),
            new DateTime(2013, 3, 26, 14, 25, 0), // Tuesday → 8 SEK
            new DateTime(2013, 3, 28, 14, 7, 27), // Thursday (day before Good Friday) → 0 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(89); // 0 + 0 + 21 + 60 + 8 + 0
    }

    [Fact]
    public void Calculate_MidnightCrossing_HandlesDateBoundaryCorrectly()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 18, 15, 0), // Day 1: 8 SEK
            new DateTime(2013, 2, 8, 6, 15, 0), // Day 2: 8 SEK
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(16); // Separate days
    }

    [Theory]
    [InlineData(6, 15, 8)]
    [InlineData(6, 45, 13)]
    [InlineData(7, 30, 18)]
    [InlineData(8, 15, 13)]
    [InlineData(10, 0, 8)]
    [InlineData(15, 15, 13)]
    [InlineData(16, 0, 18)]
    [InlineData(17, 30, 13)]
    [InlineData(18, 15, 8)]
    [InlineData(21, 0, 0)]
    public void Calculate_VariousTimeIntervals_ReturnsCorrectFee(
        int hour,
        int minute,
        int expectedFee
    )
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[] { new DateTime(2013, 2, 7, hour, minute, 0) };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(expectedFee);
    }

    [Fact]
    public void Calculate_MixedTollFreeAndChargeableInSameWindow_ChargesHighestOnly()
    {
        // Arrange
        var rule = CreateGothenburgRule();
        var vehicle = new Vehicle("ABC123", VehicleType.Car);
        var timestamps = new[]
        {
            new DateTime(2013, 2, 7, 6, 15, 0), // 8 SEK
            new DateTime(2013, 2, 9, 6, 15, 0), // 0 SEK (Saturday, same window if on same day)
        };

        // Act
        var result = _calculator.Calculate(rule, vehicle, timestamps);

        // Assert
        result.Value.Should().Be(8); // Only Thursday charged, Saturday is toll-free
    }

    private static TaxRule CreateGothenburgRule()
    {
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), new Money(18)),
            TollInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 29), new Money(13)),
            TollInterval.Create(new TimeOnly(8, 30), new TimeOnly(14, 59), new Money(8)),
            TollInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 29), new Money(13)),
            TollInterval.Create(new TimeOnly(15, 30), new TimeOnly(16, 59), new Money(18)),
            TollInterval.Create(new TimeOnly(17, 0), new TimeOnly(17, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 29), new Money(8)),
        };

        var holidays = new[]
        {
            TollFreeDate.Create(new DateOnly(2013, 1, 1), true),
            TollFreeDate.Create(new DateOnly(2013, 1, 6), true),
            TollFreeDate.Create(new DateOnly(2013, 3, 29), true),
            TollFreeDate.Create(new DateOnly(2013, 3, 31), true),
            TollFreeDate.Create(new DateOnly(2013, 4, 1), true),
            TollFreeDate.Create(new DateOnly(2013, 5, 1), true),
            TollFreeDate.Create(new DateOnly(2013, 5, 9), true),
            TollFreeDate.Create(new DateOnly(2013, 5, 19), true),
            TollFreeDate.Create(new DateOnly(2013, 5, 20), true),
            TollFreeDate.Create(new DateOnly(2013, 6, 6), true),
            TollFreeDate.Create(new DateOnly(2013, 6, 22), true),
            TollFreeDate.Create(new DateOnly(2013, 11, 1), true),
            TollFreeDate.Create(new DateOnly(2013, 12, 25), true),
            TollFreeDate.Create(new DateOnly(2013, 12, 26), true),
            TollFreeDate.Create(new DateOnly(2013, 12, 31), true),
        };

        var freeMonths = new[] { TollFreeMonth.Create(Month.July) };

        var freeWeekdays = new[]
        {
            TollFreeWeekday.Create(DayOfWeek.Saturday),
            TollFreeWeekday.Create(DayOfWeek.Sunday),
        };

        var exemptVehicles = new[]
        {
            TollFreeVehicle.Create(VehicleType.Motorbike),
            TollFreeVehicle.Create(VehicleType.Bus),
            TollFreeVehicle.Create(VehicleType.Emergency),
            TollFreeVehicle.Create(VehicleType.Diplomat),
            TollFreeVehicle.Create(VehicleType.Military),
            TollFreeVehicle.Create(VehicleType.Foreign),
        };

        return new TaxRule(
            ruleId,
            cityId,
            2013,
            new Money(60),
            60,
            intervals,
            holidays,
            freeMonths,
            freeWeekdays,
            exemptVehicles
        );
    }
}
