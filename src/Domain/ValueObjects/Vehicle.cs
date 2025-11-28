using System.Text.RegularExpressions;
using Domain.Enums;

namespace Domain.ValueObjects;

public readonly struct Vehicle
{
    public string Registration { get; }
    public VehicleType Type { get; }

    private static readonly Regex RegistrationPattern = new("^[A-Z0-9]+$");

    public Vehicle(string registration, VehicleType type)
    {
        if (string.IsNullOrWhiteSpace(registration))
            throw new ArgumentException("Registration cannot be empty.", nameof(registration));

        if (!RegistrationPattern.IsMatch(registration))
            throw new ArgumentException("Invalid registration format.");

        Registration = registration;
        Type = type;
    }

    public static bool operator ==(Vehicle a, Vehicle b) => a.Equals(b);

    public static bool operator !=(Vehicle a, Vehicle b) => !a.Equals(b);

    public bool Equals(Vehicle other) =>
        string.Equals(Registration, other.Registration, StringComparison.OrdinalIgnoreCase) && Type == other.Type;

    public override bool Equals(object? obj) =>
        obj is Vehicle other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Registration.ToUpperInvariant(), Type);

    public int CompareTo(Vehicle other)
    {
        var regComparison = string.Compare(
            Registration,
            other.Registration,
            StringComparison.OrdinalIgnoreCase
        );

        if (regComparison != 0)
            return regComparison;

        return Type.CompareTo(other.Type);
    }

    public override string ToString() => $"{Type} ({Registration})";
}
