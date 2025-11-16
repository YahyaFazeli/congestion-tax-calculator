using Domain.Base;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class TaxRule : Entity
{
    public Guid CityId { get; private set; }
    public int Year { get; private set; }
    public Money DailyMax { get; private set; }
    public int SingleChargeMinutes { get; private set; }

    private readonly List<TollInterval> _intervals = [];
    private readonly List<TollFreeDate> _tollFreeDates = [];
    private readonly List<TollFreeMonth> _tollFreeMonths = [];
    private readonly List<TollFreeWeekday> _tollFreeWeekdays = [];
    private readonly List<TollFreeVehicle> _tollFreeVehicles = [];

    public IReadOnlyCollection<TollInterval> Intervals => _intervals;
    public IReadOnlyCollection<TollFreeDate> TollFreeDates => _tollFreeDates;
    public IReadOnlyCollection<TollFreeMonth> TollFreeMonths => _tollFreeMonths;
    public IReadOnlyCollection<TollFreeWeekday> TollFreeWeekdays => _tollFreeWeekdays;
    public IReadOnlyCollection<TollFreeVehicle> TollFreeVehicles => _tollFreeVehicles;

    private TaxRule() { }

    public TaxRule(
        Guid id,
        Guid cityId,
        int year,
        Money dailyMax,
        int singleChargeMinutes,
        IEnumerable<TollInterval> intervals,
        IEnumerable<TollFreeDate> freeDates,
        IEnumerable<TollFreeMonth> freeMonths,
        IEnumerable<TollFreeWeekday> freeWeekdays,
        IEnumerable<TollFreeVehicle> freeVehicles
    )
    {
        Id = id;
        CityId = cityId;
        Year = year;
        DailyMax = dailyMax;
        SingleChargeMinutes = singleChargeMinutes;

        _intervals.AddRange(intervals);
        _tollFreeDates.AddRange(freeDates);
        _tollFreeMonths.AddRange(freeMonths);
        _tollFreeWeekdays.AddRange(freeWeekdays);
        _tollFreeVehicles.AddRange(freeVehicles);
    }

    public static TaxRule Create(
        Guid cityId,
        int year,
        Money dailyMax,
        int singleChargeMinutes,
        IEnumerable<TollInterval> intervals,
        IEnumerable<TollFreeDate> freeDates,
        IEnumerable<TollFreeMonth> freeMonths,
        IEnumerable<TollFreeWeekday> freeWeekdays,
        IEnumerable<TollFreeVehicle> freeVehicles
    )
    {
        return new(
            NewId(),
            cityId,
            year,
            dailyMax,
            singleChargeMinutes,
            intervals,
            freeDates,
            freeMonths,
            freeWeekdays,
            freeVehicles
        );
    }

    public bool IsVehicleExempt(Vehicle vehicle)
    {
        return TollFreeVehicles.Any(x => x.Matches(vehicle));
    }

    public bool IsTollFreeDate(DateTime dt)
    {
        if (dt.Year != Year)
            return true; // outside allowed year

        if (TollFreeWeekdays.Any(x => x.Matches(dt)))
            return true;

        if (TollFreeMonths.Any(x => x.Matches(dt)))
            return true;

        if (TollFreeDates.Any(x => x.Matches(dt)))
            return true;

        return false;
    }

    public Money GetFeeFor(DateTime dt)
    {
        var interval = Intervals.FirstOrDefault(i => i.IsWithin(dt));
        return interval?.Amount ?? Money.Zero;
    }
}
