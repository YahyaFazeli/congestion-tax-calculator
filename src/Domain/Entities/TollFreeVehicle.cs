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
        Id = id;
        VehicleType = vehicleType;
    }

    public static TollFreeVehicle Create(VehicleType vehicleType)
    {
        return new(NewId(), vehicleType);
    }

    public bool Matches(Vehicle vehicle) => vehicle.Type == VehicleType;
}
