using Domain.Enums;
using Domain.Base;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class TollFreeVehicle : Entity
{
    public Guid TaxRuleId { get; private set; }
    public VehicleType VehicleType { get; private set; }


    private TollFreeVehicle() { }

    public TollFreeVehicle(Guid id, Guid taxRuleId, VehicleType vehicleType)
    {
        Id = id;
        TaxRuleId = taxRuleId;
        VehicleType = vehicleType;
    }

    public static TollFreeVehicle Create(Guid taxRuleId, VehicleType vehicleType)
    {
        return new(NewId(), taxRuleId, vehicleType);
    }

    public bool Matches(Vehicle vehicle) => vehicle.Type == VehicleType;
}
