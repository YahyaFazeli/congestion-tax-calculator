using Domain.Entities;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TollFreeDateTests
{
    [Fact]
    public void Create_WithValidDate_CreatesTollFreeDate()
    {
        // Arrange
        var date = new DateOnly(2013, 3, 29);

        // Act
        var tollFreeDate = TollFreeDate.Create(date, false);

        // Assert
        tollFreeDate.Date.Should().Be(date);
        tollFreeDate.IncludeDayBefore.Should().BeFalse();
        tollFreeDate.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithIncludeDayBefore_SetsFlagCorrectly()
    {
        // Arrange
        var date = new DateOnly(2013, 3, 29);

        // Act
        var tollFreeDate = TollFreeDate.Create(date, true);

        // Assert
        tollFreeDate.IncludeDayBefore.Should().BeTrue();
    }

    [Fact]
    public void Matches_ExactDateMatch_ReturnsTrue()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), false);
        var timestamp = new DateTime(2013, 3, 29, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_DifferentDate_ReturnsFalse()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), false);
        var timestamp = new DateTime(2013, 3, 30, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Matches_DayBeforeHoliday_WithIncludeDayBeforeTrue_ReturnsTrue()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), true);
        var timestamp = new DateTime(2013, 3, 28, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_DayBeforeHoliday_WithIncludeDayBeforeFalse_ReturnsFalse()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), false);
        var timestamp = new DateTime(2013, 3, 28, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Matches_TwoDaysBeforeHoliday_WithIncludeDayBeforeTrue_ReturnsFalse()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), true);
        var timestamp = new DateTime(2013, 3, 27, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Matches_DayAfterHoliday_ReturnsFalse()
    {
        // Arrange
        var tollFreeDate = TollFreeDate.Create(new DateOnly(2013, 3, 29), true);
        var timestamp = new DateTime(2013, 3, 30, 10, 0, 0);

        // Act
        var result = tollFreeDate.Matches(timestamp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => new TollFreeDate(Guid.Empty, new DateOnly(2013, 3, 29), false);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Toll free date ID cannot be empty*");
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2101)]
    public void Constructor_WithInvalidYear_ThrowsArgumentException(int year)
    {
        // Act
        var act = () => new TollFreeDate(Guid.NewGuid(), new DateOnly(year, 1, 1), false);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Date year must be between 1900 and 2100*");
    }
}
