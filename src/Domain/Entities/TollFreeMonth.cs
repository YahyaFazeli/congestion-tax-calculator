using Domain.Base;
using Domain.Enums;

namespace Domain.Entities;

public sealed class TollFreeMonth : Entity
{
    public Month Month { get; private set; }

    private TollFreeMonth() { }

    public TollFreeMonth(Guid id, Month month)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Toll free month ID cannot be empty.", nameof(id));

        if (!Enum.IsDefined(typeof(Month), month))
            throw new ArgumentException($"Invalid month value: {month}.", nameof(month));

        Id = id;
        Month = month;
    }

    public static TollFreeMonth Create(Month month)
    {
        return new(NewId(), month);
    }

    public bool Matches(DateTime dt)
    {
        return dt.Month == (int)Month;
    }
}
