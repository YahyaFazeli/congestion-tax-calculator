using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithPositiveValue_CreatesMoneyInstance()
    {
        // Arrange & Act
        var money = new Money(100);

        // Assert
        money.Value.Should().Be(100);
    }

    [Fact]
    public void Constructor_WithZeroValue_CreatesMoneyInstance()
    {
        // Arrange & Act
        var money = new Money(0);

        // Assert
        money.Value.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNegativeValue_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Money(-10);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Money cannot be negative.");
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroValue()
    {
        // Arrange & Act
        var zero = Money.Zero;

        // Assert
        zero.Value.Should().Be(0);
    }

    [Fact]
    public void Addition_TwoMoneyValues_ReturnsSum()
    {
        // Arrange
        var money1 = new Money(10);
        var money2 = new Money(20);

        // Act
        var result = money1 + money2;

        // Assert
        result.Value.Should().Be(30);
    }

    [Fact]
    public void Subtraction_TwoMoneyValues_ReturnsDifference()
    {
        // Arrange
        var money1 = new Money(30);
        var money2 = new Money(10);

        // Act
        var result = money1 - money2;

        // Assert
        result.Value.Should().Be(20);
    }

    [Fact]
    public void GreaterThan_FirstValueLarger_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(20);
        var money2 = new Money(10);

        // Act & Assert
        (money1 > money2)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void GreaterThan_FirstValueSmaller_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(10);
        var money2 = new Money(20);

        // Act & Assert
        (money1 > money2)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void LessThan_FirstValueSmaller_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(10);
        var money2 = new Money(20);

        // Act & Assert
        (money1 < money2)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void LessThan_FirstValueLarger_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(20);
        var money2 = new Money(10);

        // Act & Assert
        (money1 < money2)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void CompareTo_FirstValueLarger_ReturnsPositive()
    {
        // Arrange
        var money1 = new Money(20);
        var money2 = new Money(10);

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_FirstValueSmaller_ReturnsNegative()
    {
        // Arrange
        var money1 = new Money(10);
        var money2 = new Money(20);

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_EqualValues_ReturnsZero()
    {
        // Arrange
        var money1 = new Money(10);
        var money2 = new Money(10);

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        // Arrange
        var money = new Money(10.5m);

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("10.5");
    }
}
