using Domain.Enums;
using MediatR;

namespace Application.Commands.UpdateCityTaxRule;

public sealed record UpdateCityTaxRuleCommand(
    Guid CityId,
    Guid RuleId,
    int Year,
    decimal DailyMax,
    int SingleChargeMinutes,
    IEnumerable<TollIntervalDto> Intervals,
    IEnumerable<TollFreeDateDto> FreeDates,
    IEnumerable<Month> FreeMonths,
    IEnumerable<DayOfWeek> FreeWeekdays,
    IEnumerable<VehicleType> FreeVehicles
) : IRequest<UpdateCityTaxRuleResult>;

public sealed record TollIntervalDto(string Start, string End, decimal Amount);

public sealed record TollFreeDateDto(string Date, bool IncludeDayBefore);
