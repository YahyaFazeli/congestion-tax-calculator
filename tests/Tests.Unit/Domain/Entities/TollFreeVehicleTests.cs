using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class TollFreeVehicleTests
{
    [Fact]
    public void Create_WithValidVehicleType_CreatesTollFreeVehicle()
    {
        // Arrange & Act
        var tollFreeVehicle = TollFreeVehicle.Create(VehicleType.Motorbike);

        // Assert
        tollFreeVehicle.VehicleType.Should().Be(VehicleType.Motorbike);
        tollFreeVehicle.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Matches_MatchingVehicleType_ReturnsTrue()
    {
        // Arrange
        var tollFreeVehicle = TollFreeVehicle.Create(VehicleType.Motorbike);
        var vehicle = new Vehicle("ABC123", VehicleType.Motorbike);

        // Act
        var result = tollFreeVehicle.Matches(vehicle);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_NonMatchingVehicleType_ReturnsFalse()
    {
        // Arrange
        var tollFreeVehicle = TollFreeVehicle.Create(VehicleType.Motorbike);
        var vehicle = new Vehicle("ABC123", VehicleType.Car);

        // Act
        var result = tollFreeVehicle.Matches(vehicle);

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
    public void Matches_AllExemptVehicleTypes_ReturnsTrue(VehicleType type)
    {
        // Arrange
        var tollFreeVehicle = TollFreeVehicle.Create(type);
        var vehicle = new Vehicle("ABC123", type);

        // Act
        var result = tollFreeVehicle.Matches(vehicle);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => new TollFreeVehicle(Guid.Empty, VehicleType.Motorbike);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Toll free vehicle ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithInvalidVehicleType_ThrowsArgumentException()
    {
        // Act
        var invalidType = unchecked((VehicleType)999);
        var act = () => new TollFreeVehicle(Guid.NewGuid(), invalidType);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid vehicle type value*");
    }
}
