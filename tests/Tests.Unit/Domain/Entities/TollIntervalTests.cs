using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TollIntervalTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesInterval()
    {
        // Arrange
        var start = new TimeOnly(6, 0);
        var end = new TimeOnly(6, 29);
        var amount = new Money(8);

        // Act
        var interval = TollInterval.Create(start, end, amount);

        // Assert
        interval.Start.Should().Be(start);
        interval.End.Should().Be(end);
        interval.Amount.Should().Be(amount);
        interval.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void IsWithin_TimestampWithinInterval_ReturnsTrue()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 6, 15, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithin_TimestampAtStartBoundary_ReturnsTrue()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 6, 0, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithin_TimestampAtEndBoundary_ReturnsFalse()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 6, 30, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithin_TimestampBeforeInterval_ReturnsFalse()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 5, 59, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithin_TimestampAfterInterval_ReturnsFalse()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 7, 0, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithin_TimestampOneMinuteBeforeEnd_ReturnsTrue()
    {
        // Arrange
        var interval = TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8));
        var timestamp = new DateTime(2013, 2, 7, 6, 29, 0);

        // Act
        var result = interval.IsWithin(timestamp);

        // Assert
        result.Should().BeTrue();
    }
}
