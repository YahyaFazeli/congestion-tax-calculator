using Domain.Enums;

namespace API.Endpoints;

public sealed record AddCityTaxRuleRequest(
    int Year,
    decimal DailyMax,
    int SingleChargeMinutes,
    IEnumerable<TollIntervalRequest> Intervals,
    IEnumerable<TollFreeDateRequest> FreeDates,
    IEnumerable<Month> FreeMonths,
    IEnumerable<DayOfWeek> FreeWeekdays,
    IEnumerable<VehicleType> FreeVehicles
);

public sealed record TollIntervalRequest(string Start, string End, decimal Amount);

public sealed record TollFreeDateRequest(string Date, bool IncludeDayBefore);
