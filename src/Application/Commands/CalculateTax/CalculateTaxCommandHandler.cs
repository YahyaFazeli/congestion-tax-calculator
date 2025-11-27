using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.CalculateTax;

public sealed class CalculateTaxCommandHandler(
    ITaxRuleRepository taxRuleRepository,
    ITaxCalculator taxCalculator,
    ILogger<CalculateTaxCommandHandler> logger
) : IRequestHandler<CalculateTaxCommand, CalculateTaxResult>
{
    private static string MaskVehicleRegistration(string registration)
    {
        if (string.IsNullOrEmpty(registration) || registration.Length <= 3)
            return "***";
        return registration.Substring(0, 3) + "***";
    }

    public async Task<CalculateTaxResult> Handle(
        CalculateTaxCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Calculating tax for CityId: {CityId}, Year: {Year}, VehicleType: {VehicleType}, VehicleRegistration: {VehicleRegistration}",
            request.CityId,
            request.Year,
            request.VehicleType,
            MaskVehicleRegistration(request.VehicleRegistration)
        );

        var rule = await taxRuleRepository.GetByCityAndYearAsync(
            request.CityId,
            request.Year,
            cancellationToken
        );

        if (rule is null)
        {
            throw new TaxRuleNotFoundException(request.CityId, request.Year);
        }

        var vehicle = new Vehicle(request.VehicleRegistration, request.VehicleType);

        var totalTax = taxCalculator.Calculate(rule, vehicle, request.Timestamps);

        logger.LogInformation(
            "Tax calculated successfully. CityId: {CityId}, Year: {Year}, TotalTax: {TotalTax}",
            request.CityId,
            request.Year,
            totalTax.Value
        );

        return new CalculateTaxResult(totalTax.Value);
    }
}
