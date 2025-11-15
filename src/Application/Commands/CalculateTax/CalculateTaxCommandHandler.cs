using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Commands.CalculateTax;

public sealed class CalculateTaxCommandHandler(
    ITaxRuleRepository taxRuleRepository,
    ITaxCalculator taxCalculator
) : IRequestHandler<CalculateTaxCommand, CalculateTaxResult>
{
    public async Task<CalculateTaxResult> Handle(
        CalculateTaxCommand request,
        CancellationToken cancellationToken
    )
    {
        var rule = await taxRuleRepository.GetByCityAndYearAsync(
            request.CityId,
            request.Year,
            cancellationToken
        );

        if (rule is null)
            throw new InvalidOperationException(
                $"No tax rule found for city {request.CityId} and year {request.Year}"
            );

        var vehicle = new Vehicle(request.VehicleRegistration, request.VehicleType);

        var totalTax = taxCalculator.Calculate(rule, vehicle, request.Timestamps);

        return new CalculateTaxResult(totalTax.Value);
    }
}
