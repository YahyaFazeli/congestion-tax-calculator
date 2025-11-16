using Domain.Enums;

namespace API.ViewModels;

public sealed record UpdateCityTaxRuleRequest(
    int Year,
    decimal DailyMax,
    int SingleChargeMinutes,
    IEnumerable<TollIntervalUpdateRequest> Intervals,
    IEnumerable<TollFreeDateUpdateRequest> FreeDates,
    IEnumerable<Month> FreeMonths,
    IEnumerable<DayOfWeek> FreeWeekdays,
    IEnumerable<VehicleType> FreeVehicles
);

public sealed record TollIntervalUpdateRequest(string Start, string End, decimal Amount);

public sealed record TollFreeDateUpdateRequest(string Date, bool IncludeDayBefore);
