using Domain.Enums;

namespace API.ViewModels;

public sealed record CalculateTaxRequest(
    Guid CityId,
    int Year,
    string VehicleRegistration,
    VehicleType VehicleType,
    IEnumerable<DateTime> Timestamps
);
