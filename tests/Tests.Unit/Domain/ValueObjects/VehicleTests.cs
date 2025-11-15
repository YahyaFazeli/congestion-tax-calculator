using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.ValueObjects;

public class VehicleTests
{
    [Fact]
    public void Constructor_WithValidRegistrationAndType_CreatesVehicle()
    {
        // Arrange & Act
        var vehicle = new Vehicle("ABC123", VehicleType.Car);

        // Assert
        vehicle.Registration.Should().Be("ABC123");
        vehicle.Type.Should().Be(VehicleType.Car);
    }

    [Fact]
    public void Constructor_WithNullRegistration_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Vehicle(null!, VehicleType.Car);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Registration cannot be empty.*");
    }

    [Fact]
    public void Constructor_WithEmptyRegistration_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Vehicle("", VehicleType.Car);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Registration cannot be empty.*");
    }

    [Fact]
    public void Constructor_WithWhitespaceRegistration_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Vehicle("   ", VehicleType.Car);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Registration cannot be empty.*");
    }

    [Theory]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Military)]
    [InlineData(VehicleType.Foreign)]
    public void Constructor_WithAllVehicleTypes_CreatesVehicle(VehicleType type)
    {
        // Arrange & Act
        var vehicle = new Vehicle("ABC123", type);

        // Assert
        vehicle.Type.Should().Be(type);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var vehicle = new Vehicle("ABC123", VehicleType.Car);

        // Act
        var result = vehicle.ToString();

        // Assert
        result.Should().Be("Car (ABC123)");
    }
}
