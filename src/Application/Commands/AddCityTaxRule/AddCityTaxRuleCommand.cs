using Domain.Common;
using Domain.Enums;
using MediatR;

namespace Application.Commands.AddCityTaxRule;

public sealed record AddCityTaxRuleCommand(
    Guid CityId,
    int Year,
    decimal DailyMax,
    int SingleChargeMinutes,
    IEnumerable<TollIntervalDto> Intervals,
    IEnumerable<TollFreeDateDto> FreeDates,
    IEnumerable<Month> FreeMonths,
    IEnumerable<DayOfWeek> FreeWeekdays,
    IEnumerable<VehicleType> FreeVehicles
) : IRequest<Result<AddCityTaxRuleResult>>;

public sealed record TollIntervalDto(string Start, string End, decimal Amount);

public sealed record TollFreeDateDto(string Date, bool IncludeDayBefore);
