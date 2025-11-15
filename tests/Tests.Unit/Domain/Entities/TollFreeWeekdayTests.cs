using Domain.Entities;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TollFreeWeekdayTests
{
    [Fact]
    public void Create_WithValidDay_CreatesTollFreeWeekday()
    {
        // Arrange & Act
        var tollFreeWeekday = TollFreeWeekday.Create(DayOfWeek.Saturday);

        // Assert
        tollFreeWeekday.Day.Should().Be(DayOfWeek.Saturday);
        tollFreeWeekday.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Matches_Saturday_ReturnsTrue()
    {
        // Arrange
        var tollFreeWeekday = TollFreeWeekday.Create(DayOfWeek.Saturday);
        var timestamp = new DateTime(2013, 2, 9, 10, 0, 0); // Saturday

        // Act
        var result = tollFreeWeekday.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_Sunday_ReturnsTrue()
    {
        // Arrange
        var tollFreeWeekday = TollFreeWeekday.Create(DayOfWeek.Sunday);
        var timestamp = new DateTime(2013, 2, 10, 10, 0, 0); // Sunday

        // Act
        var result = tollFreeWeekday.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_Weekday_ReturnsFalse()
    {
        // Arrange
        var tollFreeWeekday = TollFreeWeekday.Create(DayOfWeek.Saturday);
        var timestamp = new DateTime(2013, 2, 7, 10, 0, 0); // Thursday

        // Act
        var result = tollFreeWeekday.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2013, 2, 4, DayOfWeek.Monday)]
    [InlineData(2013, 2, 5, DayOfWeek.Tuesday)]
    [InlineData(2013, 2, 6, DayOfWeek.Wednesday)]
    [InlineData(2013, 2, 7, DayOfWeek.Thursday)]
    [InlineData(2013, 2, 8, DayOfWeek.Friday)]
    public void Matches_SpecificWeekday_ReturnsTrue(
        int year,
        int month,
        int day,
        DayOfWeek expectedDay
    )
    {
        // Arrange
        var tollFreeWeekday = TollFreeWeekday.Create(expectedDay);
        var timestamp = new DateTime(year, month, day, 10, 0, 0);

        // Act
        var result = tollFreeWeekday.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_DifferentWeekday_ReturnsFalse()
    {
        // Arrange
        var tollFreeWeekday = TollFreeWeekday.Create(DayOfWeek.Monday);
        var timestamp = new DateTime(2013, 2, 5, 10, 0, 0); // Tuesday

        // Act
        var result = tollFreeWeekday.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }
}
