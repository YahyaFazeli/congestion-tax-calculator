namespace Domain.ValueObjects;

public readonly struct Money : IComparable<Money>
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Money cannot be negative.");

        Value = value;
    }

    public static Money Zero => new(0);

    public static Money operator +(Money a, Money b) => new(a.Value + b.Value);

    public static Money operator -(Money a, Money b) => new(a.Value - b.Value);

    public static bool operator >(Money a, Money b) => a.Value > b.Value;

    public static bool operator <(Money a, Money b) => a.Value < b.Value;

    public int CompareTo(Money other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString("0.##");
}
