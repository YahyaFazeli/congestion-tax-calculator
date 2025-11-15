using Application.Commands.CalculateTax;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class TaxEndpoints
{
    public static void MapTaxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tax").WithTags("Tax Calculation").WithOpenApi();

        group
            .MapPost("/calculate", CalculateTax)
            .WithName("CalculateTax")
            .WithSummary("Calculate congestion tax for a vehicle")
            .WithDescription(
                "Calculates the total congestion tax for a vehicle based on timestamps and city rules"
            );
    }

    private static async Task<IResult> CalculateTax(
        [FromBody] CalculateTaxRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var command = new CalculateTaxCommand(
            request.CityId,
            request.Year,
            request.VehicleRegistration,
            request.VehicleType,
            request.Timestamps
        );

        var result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}

public sealed record CalculateTaxRequest(
    Guid CityId,
    int Year,
    string VehicleRegistration,
    VehicleType VehicleType,
    IEnumerable<DateTime> Timestamps
);
