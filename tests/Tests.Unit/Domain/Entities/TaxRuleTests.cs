using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TaxRuleTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesTaxRule()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var dailyMax = new Money(60);
        var singleChargeMinutes = 60;

        // Act
        var rule = TaxRule.Create(cityId, year, dailyMax, singleChargeMinutes, [], [], [], [], []);

        // Assert
        rule.CityId.Should().Be(cityId);
        rule.Year.Should().Be(year);
        rule.DailyMax.Should().Be(dailyMax);
        rule.SingleChargeMinutes.Should().Be(singleChargeMinutes);
        rule.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void IsVehicleExempt_ExemptVehicleType_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRuleWithExemptVehicles(VehicleType.Motorbike);
        var vehicle = new Vehicle("ABC123", VehicleType.Motorbike);

        // Act
        var result = rule.IsVehicleExempt(vehicle);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVehicleExempt_NonExemptVehicleType_ReturnsFalse()
    {
        // Arrange
        var rule = CreateTestRuleWithExemptVehicles(VehicleType.Motorbike);
        var vehicle = new Vehicle("ABC123", VehicleType.Car);

        // Act
        var result = rule.IsVehicleExempt(vehicle);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Military)]
    [InlineData(VehicleType.Foreign)]
    public void IsVehicleExempt_AllExemptTypes_ReturnsTrue(VehicleType type)
    {
        // Arrange
        var rule = CreateTestRuleWithExemptVehicles(
            VehicleType.Motorbike,
            VehicleType.Bus,
            VehicleType.Emergency,
            VehicleType.Diplomat,
            VehicleType.Military,
            VehicleType.Foreign
        );
        var vehicle = new Vehicle("ABC123", type);

        // Act
        var result = rule.IsVehicleExempt(vehicle);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTollFreeDate_Weekend_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRuleWithWeekends();
        var saturday = new DateTime(2013, 2, 9, 10, 0, 0);
        var sunday = new DateTime(2013, 2, 10, 10, 0, 0);

        // Act & Assert
        rule.IsTollFreeDate(saturday).Should().BeTrue();
        rule.IsTollFreeDate(sunday).Should().BeTrue();
    }

    [Fact]
    public void IsTollFreeDate_Weekday_ReturnsFalse()
    {
        // Arrange
        var rule = CreateTestRuleWithWeekends();
        var thursday = new DateTime(2013, 2, 7, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(thursday);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTollFreeDate_July_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRuleWithJulyFree();
        var julyDate = new DateTime(2013, 7, 15, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(julyDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTollFreeDate_NonJulyMonth_ReturnsFalse()
    {
        // Arrange
        var rule = CreateTestRuleWithJulyFree();
        var augustDate = new DateTime(2013, 8, 1, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(augustDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTollFreeDate_PublicHoliday_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRuleWithHoliday(new DateOnly(2013, 3, 29), false);
        var goodFriday = new DateTime(2013, 3, 29, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(goodFriday);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTollFreeDate_DayBeforePublicHoliday_WithIncludeDayBefore_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRuleWithHoliday(new DateOnly(2013, 3, 29), true);
        var dayBefore = new DateTime(2013, 3, 28, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(dayBefore);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTollFreeDate_DayBeforePublicHoliday_WithoutIncludeDayBefore_ReturnsFalse()
    {
        // Arrange
        var rule = CreateTestRuleWithHoliday(new DateOnly(2013, 3, 29), false);
        var dayBefore = new DateTime(2013, 3, 28, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(dayBefore);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTollFreeDate_OutsideAllowedYear_ReturnsTrue()
    {
        // Arrange
        var rule = CreateTestRule(2013);
        var wrongYear = new DateTime(2014, 2, 7, 10, 0, 0);

        // Act
        var result = rule.IsTollFreeDate(wrongYear);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetFeeFor_TimestampWithinInterval_ReturnsCorrectFee()
    {
        // Arrange
        var rule = CreateTestRuleWithIntervals();
        var timestamp = new DateTime(2013, 2, 7, 6, 15, 0);

        // Act
        var result = rule.GetFeeFor(timestamp);

        // Assert
        result.Value.Should().Be(8);
    }

    [Fact]
    public void GetFeeFor_TimestampOutsideAllIntervals_ReturnsZero()
    {
        // Arrange
        var rule = CreateTestRuleWithIntervals();
        var timestamp = new DateTime(2013, 2, 7, 21, 0, 0);

        // Act
        var result = rule.GetFeeFor(timestamp);

        // Assert
        result.Should().Be(Money.Zero);
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
    public void GetFeeFor_VariousTimeIntervals_ReturnsCorrectFee(
        int hour,
        int minute,
        int expectedFee
    )
    {
        // Arrange
        var rule = CreateGothenburgTestRule();
        var timestamp = new DateTime(2013, 2, 7, hour, minute, 0);

        // Act
        var result = rule.GetFeeFor(timestamp);

        // Assert
        result.Value.Should().Be(expectedFee);
    }

    private static TaxRule CreateTestRule(int year)
    {
        return TaxRule.Create(Guid.NewGuid(), year, new Money(60), 60, [], [], [], [], []);
    }

    private static TaxRule CreateTestRuleWithExemptVehicles(params VehicleType[] types)
    {
        var ruleId = Guid.NewGuid();
        var exemptVehicles = types.Select(t => TollFreeVehicle.Create(ruleId, t)).ToList();

        return TaxRule.Create(
            Guid.NewGuid(),
            2013,
            new Money(60),
            60,
            [],
            [],
            [],
            [],
            exemptVehicles
        );
    }

    private static TaxRule CreateTestRuleWithWeekends()
    {
        var weekends = new[]
        {
            TollFreeWeekday.Create(DayOfWeek.Saturday),
            TollFreeWeekday.Create(DayOfWeek.Sunday),
        };

        return TaxRule.Create(Guid.NewGuid(), 2013, new Money(60), 60, [], [], [], weekends, []);
    }

    private static TaxRule CreateTestRuleWithJulyFree()
    {
        var julyFree = new[] { TollFreeMonth.Create(Month.July) };

        return TaxRule.Create(Guid.NewGuid(), 2013, new Money(60), 60, [], [], julyFree, [], []);
    }

    private static TaxRule CreateTestRuleWithHoliday(DateOnly date, bool includeDayBefore)
    {
        var holidays = new[] { TollFreeDate.Create(date, includeDayBefore) };

        return TaxRule.Create(Guid.NewGuid(), 2013, new Money(60), 60, [], holidays, [], [], []);
    }

    private static TaxRule CreateTestRuleWithIntervals()
    {
        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
        };

        return TaxRule.Create(Guid.NewGuid(), 2013, new Money(60), 60, intervals, [], [], [], []);
    }

    private static TaxRule CreateGothenburgTestRule()
    {
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

        return TaxRule.Create(Guid.NewGuid(), 2013, new Money(60), 60, intervals, [], [], [], []);
    }
}
