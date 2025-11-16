using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Tests.Integration.TestHelpers;

public static class TestDataBuilder
{
    public static City CreateTestCity(string name = "TestCity")
    {
        return City.Create(name);
    }

    public static TaxRule CreateTestRule(Guid cityId, int year = 2013)
    {
        var ruleId = Guid.NewGuid();

        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), new Money(18)),
        };

        var holidays = new[]
        {
            TollFreeDate.Create(new DateOnly(year, 3, 29), true),
            TollFreeDate.Create(new DateOnly(year, 12, 25), true),
        };

        var freeMonths = new[] { TollFreeMonth.Create(Month.July) };

        var freeWeekdays = new[]
        {
            TollFreeWeekday.Create(DayOfWeek.Saturday),
            TollFreeWeekday.Create(DayOfWeek.Sunday),
        };

        var exemptVehicles = new[]
        {
            TollFreeVehicle.Create(VehicleType.Motorbike),
            TollFreeVehicle.Create(VehicleType.Bus),
        };

        return new TaxRule(
            ruleId,
            cityId,
            year,
            new Money(60),
            60,
            intervals,
            holidays,
            freeMonths,
            freeWeekdays,
            exemptVehicles
        );
    }

    public static City CreateCityWithRules(string name = "TestCity", params int[] years)
    {
        var city = CreateTestCity(name);
        foreach (var year in years)
        {
            var rule = CreateTestRule(city.Id, year);
            city.AddRule(rule);
        }
        return city;
    }
}
