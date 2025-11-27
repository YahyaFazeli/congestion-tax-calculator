using Domain.Enums;
using Domain.Base;

namespace Domain.Entities;

public sealed class TollFreeMonth : Entity
{
    public Month Month { get; private set; }


    private TollFreeMonth() { }

    public TollFreeMonth(Guid id, Month month)
    {
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
