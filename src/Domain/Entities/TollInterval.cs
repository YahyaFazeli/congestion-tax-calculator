using Domain.Base;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class TollInterval : Entity
{
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }
    public Money Amount { get; private set; }

    private TollInterval() { }

    public TollInterval(Guid id, TimeOnly start, TimeOnly end, Money amount)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Toll interval ID cannot be empty.", nameof(id));

        if (start >= end)
            throw new ArgumentException(
                $"Start time ({start}) must be before end time ({end}).",
                nameof(start)
            );

        if (amount.Value < 0)
            throw new ArgumentException("Toll amount cannot be negative.", nameof(amount));

        Id = id;
        Start = start;
        End = end;
        Amount = amount;
    }

    public static TollInterval Create(TimeOnly start, TimeOnly end, Money amount)
    {
        return new(NewId(), start, end, amount);
    }

    public bool IsWithin(DateTime dt)
    {
        var t = TimeOnly.FromDateTime(dt);
        return t >= Start && t < End;
    }
}
