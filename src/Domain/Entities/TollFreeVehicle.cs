using Domain.Base;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class TollFreeVehicle : Entity
{
    public VehicleType VehicleType { get; private set; }

    private TollFreeVehicle() { }

    public TollFreeVehicle(Guid id, VehicleType vehicleType)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Toll free vehicle ID cannot be empty.", nameof(id));

        if (!Enum.IsDefined(typeof(VehicleType), vehicleType))
            throw new ArgumentException(
                $"Invalid vehicle type value: {vehicleType}.",
                nameof(vehicleType)
            );

        Id = id;
        VehicleType = vehicleType;
    }

    public static TollFreeVehicle Create(VehicleType vehicleType)
    {
        return new(NewId(), vehicleType);
    }

    public bool Matches(Vehicle vehicle)
    {
        return vehicle.Type == VehicleType;
    }
}
