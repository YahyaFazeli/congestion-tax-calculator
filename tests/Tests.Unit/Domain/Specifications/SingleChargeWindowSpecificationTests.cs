using Domain.Specifications;
using FluentAssertions;

namespace Tests.Unit.Domain.Specifications;

public class SingleChargeWindowSpecificationTests
{
    [Fact]
    public void Constructor_WithNegativeWindowMinutes_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => new SingleChargeWindowSpecification(-1);
        act.Should().Throw<ArgumentException>().WithParameterName("windowMinutes");
    }

    [Fact]
    public void Constructor_WithZeroWindowMinutes_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => new SingleChargeWindowSpecification(0);
        act.Should().Throw<ArgumentException>().WithParameterName("windowMinutes");
    }

    [Fact]
    public void IsSatisfiedBy_WithinWindow_ReturnsTrue()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(60);
        var first = new DateTime(2013, 2, 7, 6, 0, 0);
        var second = new DateTime(2013, 2, 7, 6, 30, 0);

        // Act
        var result = spec.IsSatisfiedBy((first, second));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_ExactlyAtWindowBoundary_ReturnsTrue()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(60);
        var first = new DateTime(2013, 2, 7, 6, 0, 0);
        var second = new DateTime(2013, 2, 7, 7, 0, 0);

        // Act
        var result = spec.IsSatisfiedBy((first, second));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_BeyondWindow_ReturnsFalse()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(60);
        var first = new DateTime(2013, 2, 7, 6, 0, 0);
        var second = new DateTime(2013, 2, 7, 7, 5, 0);

        // Act
        var result = spec.IsSatisfiedBy((first, second));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_SameTimestamp_ReturnsTrue()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(60);
        var timestamp = new DateTime(2013, 2, 7, 6, 0, 0);

        // Act
        var result = spec.IsSatisfiedBy((timestamp, timestamp));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_SecondBeforeFirst_ReturnsFalse()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(60);
        var first = new DateTime(2013, 2, 7, 7, 0, 0);
        var second = new DateTime(2013, 2, 7, 6, 0, 0);

        // Act
        var result = spec.IsSatisfiedBy((first, second));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_CustomWindowSize_WorksCorrectly()
    {
        // Arrange
        var spec = new SingleChargeWindowSpecification(30);
        var first = new DateTime(2013, 2, 7, 6, 0, 0);
        var withinWindow = new DateTime(2013, 2, 7, 6, 25, 0);
        var outsideWindow = new DateTime(2013, 2, 7, 6, 35, 0);

        // Act & Assert
        spec.IsSatisfiedBy((first, withinWindow)).Should().BeTrue();
        spec.IsSatisfiedBy((first, outsideWindow)).Should().BeFalse();
    }
}
