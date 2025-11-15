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
        var endExclusive = End.AddMinutes(1);
        return t >= Start && t < endExclusive;
    }
}
