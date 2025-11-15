using Domain.Base;

namespace Domain.Entities;

public sealed class TollFreeDate : Entity
{
    public DateOnly Date { get; private set; }
    public bool IncludeDayBefore { get; private set; }


    private TollFreeDate() { }

    public TollFreeDate(Guid id, DateOnly date, bool includeDayBefore = false)
    {
        Id = id;
        Date = date;
        IncludeDayBefore = includeDayBefore;
    }

    public static TollFreeDate Create(DateOnly date, bool includeDayBefore = false)
    {
        return new(NewId(), date, includeDayBefore);
    }

    public bool Matches(DateTime dt)
    {
        var checkDate = DateOnly.FromDateTime(dt);

        if (checkDate == Date)
            return true;

        if (IncludeDayBefore && checkDate.AddDays(1) == Date)
            return true;

        return false;
    }
}
