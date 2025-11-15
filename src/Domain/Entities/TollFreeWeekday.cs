using Domain.Base;

namespace Domain.Entities;

public sealed class TollFreeWeekday : Entity
{
    public DayOfWeek Day { get; private set; }


    private TollFreeWeekday() { }

    public TollFreeWeekday(Guid id, DayOfWeek day)
    {
        Id = id;
        Day = day;
    }

    public static TollFreeWeekday Create(DayOfWeek day)
    {
        return new(NewId(), day);
    }

    public bool Matches(DateTime dt) => dt.DayOfWeek == Day;
}
