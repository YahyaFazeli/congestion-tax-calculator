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

    public static Money operator -(Money a, Money b)
    {
        var result = a.Value - b.Value;
        if (result < 0)
            throw new InvalidOperationException("Money cannot become negative.");

        return new Money(result);
    }

    public static bool operator >(Money a, Money b) => a.Value > b.Value;

    public static bool operator <(Money a, Money b) => a.Value < b.Value;

    public static bool operator ==(Money a, Money b) => a.Value == b.Value;

    public static bool operator !=(Money a, Money b) => a.Value != b.Value;

    public bool Equals(Money other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(Money other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString("0.##");
}
