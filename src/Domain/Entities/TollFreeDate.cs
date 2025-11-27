using Domain.Base;

namespace Domain.Entities;

public sealed class TollFreeDate : Entity
{
    public DateOnly Date { get; private set; }
    public bool IncludeDayBefore { get; private set; }

    private TollFreeDate() { }

    public TollFreeDate(Guid id, DateOnly date, bool includeDayBefore = false)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Toll free date ID cannot be empty.", nameof(id));

        if (date.Year < 1900 || date.Year > 2100)
            throw new ArgumentException("Date year must be between 1900 and 2100.", nameof(date));

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
