using Domain.Enums;
using MediatR;

namespace Application.Commands.CalculateTax;

public sealed record CalculateTaxCommand(
    Guid CityId,
    int Year,
    string VehicleRegistration,
    VehicleType VehicleType,
    IEnumerable<DateTime> Timestamps
) : IRequest<CalculateTaxResult>;
