using Domain.Enums;

namespace Domain.ValueObjects;

public readonly struct Vehicle
{
    public string Registration { get; }
    public VehicleType Type { get; }

    public Vehicle(string registration, VehicleType type)
    {
        if (string.IsNullOrWhiteSpace(registration))
            throw new ArgumentException("Registration cannot be empty.", nameof(registration));

        Registration = registration;
        Type = type;
    }

    public override string ToString() => $"{Type} ({Registration})";
}
