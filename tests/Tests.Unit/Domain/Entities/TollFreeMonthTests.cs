using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TollFreeMonthTests
{
    [Fact]
    public void Create_WithValidMonth_CreatesTollFreeMonth()
    {
        // Arrange & Act
        var tollFreeMonth = TollFreeMonth.Create(Month.July);

        // Assert
        tollFreeMonth.Month.Should().Be(Month.July);
        tollFreeMonth.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Matches_DateInJuly_ReturnsTrue()
    {
        // Arrange
        var tollFreeMonth = TollFreeMonth.Create(Month.July);
        var timestamp = new DateTime(2013, 7, 15, 10, 0, 0);

        // Act
        var result = tollFreeMonth.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_FirstDayOfJuly_ReturnsTrue()
    {
        // Arrange
        var tollFreeMonth = TollFreeMonth.Create(Month.July);
        var timestamp = new DateTime(2013, 7, 1, 10, 0, 0);

        // Act
        var result = tollFreeMonth.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_LastDayOfJuly_ReturnsTrue()
    {
        // Arrange
        var tollFreeMonth = TollFreeMonth.Create(Month.July);
        var timestamp = new DateTime(2013, 7, 31, 10, 0, 0);

        // Act
        var result = tollFreeMonth.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_DateInOtherMonth_ReturnsFalse()
    {
        // Arrange
        var tollFreeMonth = TollFreeMonth.Create(Month.July);
        var timestamp = new DateTime(2013, 8, 1, 10, 0, 0);

        // Act
        var result = tollFreeMonth.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void Matches_DateInNonJulyMonth_ReturnsFalse(int month)
    {
        // Arrange
        var tollFreeMonth = TollFreeMonth.Create(Month.July);
        var timestamp = new DateTime(2013, month, 15, 10, 0, 0);

        // Act
        var result = tollFreeMonth.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }
}
