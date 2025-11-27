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
        if (id == Guid.Empty)
            throw new ArgumentException("Tax rule ID cannot be empty.", nameof(id));

        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));

        if (year < 1900 || year > 2100)
            throw new ArgumentException("Year must be between 1900 and 2100.", nameof(year));

        if (dailyMax.Value <= 0)
            throw new ArgumentException(
                "Daily maximum must be greater than zero.",
                nameof(dailyMax)
            );

        if (singleChargeMinutes <= 0)
            throw new ArgumentException(
                "Single charge minutes must be greater than zero.",
                nameof(singleChargeMinutes)
            );

        var intervalList =
            intervals?.ToList() ?? throw new ArgumentNullException(nameof(intervals));
        if (intervalList.Count == 0)
            throw new ArgumentException(
                "At least one toll interval is required.",
                nameof(intervals)
            );

        ValidateNoOverlappingIntervals(intervalList);

        Id = id;
        CityId = cityId;
        Year = year;
        DailyMax = dailyMax;
        SingleChargeMinutes = singleChargeMinutes;

        _intervals.AddRange(intervalList);
        _tollFreeDates.AddRange(freeDates ?? []);
        _tollFreeMonths.AddRange(freeMonths ?? []);
        _tollFreeWeekdays.AddRange(freeWeekdays ?? []);
        _tollFreeVehicles.AddRange(freeVehicles ?? []);
    }

    private static void ValidateNoOverlappingIntervals(List<TollInterval> intervals)
    {
        var sorted = intervals.OrderBy(i => i.Start).ToList();

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var current = sorted[i];
            var next = sorted[i + 1];

            // Check if current interval's end (inclusive) overlaps with next interval's start
            if (current.End >= next.Start)
            {
                throw new ArgumentException(
                    $"Toll intervals cannot overlap. Interval {current.Start}-{current.End} overlaps with {next.Start}-{next.End}.",
                    nameof(intervals)
                );
            }
        }
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
