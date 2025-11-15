using Domain.Entities;

namespace Application.Queries.GetCityTaxRules;

public sealed record CityTaxRuleDto(Guid CityId, string CityName, IEnumerable<TaxRuleDto> Rules);

public sealed record TaxRuleDto(
    Guid Id,
    int Year,
    decimal DailyMax,
    int SingleChargeMinutes,
    List<TollInterval> Intervals,
    List<TollFreeDate> TollFreeDates,
    List<TollFreeMonth> TollFreeMonths,
    List<TollFreeWeekday> TollFreeWeekdays,
    List<TollFreeVehicle> TollFreeVehicles
);
