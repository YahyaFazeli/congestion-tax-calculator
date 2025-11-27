using Domain.Base;

namespace Domain.Entities;

public sealed class TollFreeWeekday : Entity
{
    public DayOfWeek Day { get; private set; }

    private TollFreeWeekday() { }

    public TollFreeWeekday(Guid id, DayOfWeek day)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Toll free weekday ID cannot be empty.", nameof(id));

        if (!Enum.IsDefined(typeof(DayOfWeek), day))
            throw new ArgumentException($"Invalid day of week value: {day}.", nameof(day));

        Id = id;
        Day = day;
    }

    public static TollFreeWeekday Create(DayOfWeek day)
    {
        return new(NewId(), day);
    }

    public bool Matches(DateTime dt)
    {
        return dt.DayOfWeek == Day;
    }
}
