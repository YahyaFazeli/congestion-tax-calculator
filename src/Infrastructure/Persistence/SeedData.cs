using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedGothenburg2013Async(CongestionTaxDbContext context)
    {
        if (context.Cities.Any(c => c.Name == "Gothenburg"))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var gothenburg = City.Create("Gothenburg");

        var intervals = new List<TollInterval>
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), new Money(18)),
            TollInterval.Create(new TimeOnly(8, 0), new TimeOnly(8, 29), new Money(13)),
            TollInterval.Create(new TimeOnly(8, 30), new TimeOnly(14, 59), new Money(8)),
            TollInterval.Create(new TimeOnly(15, 0), new TimeOnly(15, 29), new Money(13)),
            TollInterval.Create(new TimeOnly(15, 30), new TimeOnly(16, 59), new Money(18)),
            TollInterval.Create(new TimeOnly(17, 0), new TimeOnly(17, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(18, 0), new TimeOnly(18, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(18, 30), new TimeOnly(5, 59), new Money(0)),
        };

        var freeDates = new List<TollFreeDate>
        {
            TollFreeDate.Create(new DateOnly(2013, 1, 1), true), // New Year's Day
            TollFreeDate.Create(new DateOnly(2013, 1, 6), true), // Epiphany
            TollFreeDate.Create(new DateOnly(2013, 3, 29), true), // Good Friday
            TollFreeDate.Create(new DateOnly(2013, 3, 31), true), // Easter Sunday
            TollFreeDate.Create(new DateOnly(2013, 4, 1), true), // Easter Monday
            TollFreeDate.Create(new DateOnly(2013, 5, 1), true), // May Day
            TollFreeDate.Create(new DateOnly(2013, 5, 9), true), // Ascension Day
            TollFreeDate.Create(new DateOnly(2013, 5, 19), true), // Whit Sunday
            TollFreeDate.Create(new DateOnly(2013, 5, 20), true), // Whit Monday
            TollFreeDate.Create(new DateOnly(2013, 6, 6), true), // National Day
            TollFreeDate.Create(new DateOnly(2013, 6, 22), true), // Midsummer Day
            TollFreeDate.Create(new DateOnly(2013, 11, 1), true), // All Saints' Day
            TollFreeDate.Create(new DateOnly(2013, 12, 25), true), // Christmas Day
            TollFreeDate.Create(new DateOnly(2013, 12, 26), true), // Boxing Day
            TollFreeDate.Create(new DateOnly(2013, 12, 31), true), // New Year's Eve
        };

        var freeMonths = new List<TollFreeMonth> { TollFreeMonth.Create(Month.July) };

        var freeWeekdays = new List<TollFreeWeekday>
        {
            TollFreeWeekday.Create(DayOfWeek.Saturday),
            TollFreeWeekday.Create(DayOfWeek.Sunday),
        };

        var freeVehicles = new List<TollFreeVehicle>
        {
            TollFreeVehicle.Create(VehicleType.Motorbike),
            TollFreeVehicle.Create(VehicleType.Bus),
            TollFreeVehicle.Create(VehicleType.Emergency),
            TollFreeVehicle.Create(VehicleType.Diplomat),
            TollFreeVehicle.Create(VehicleType.Foreign),
            TollFreeVehicle.Create(VehicleType.Military),
        };

        var taxRule = TaxRule.Create(
            gothenburg.Id,
            2013,
            new Money(60),
            60,
            intervals,
            freeDates,
            freeMonths,
            freeWeekdays,
            freeVehicles
        );

        gothenburg.AddRule(taxRule);

        context.Add(gothenburg);
        await context.SaveChangesAsync();
    }
}
