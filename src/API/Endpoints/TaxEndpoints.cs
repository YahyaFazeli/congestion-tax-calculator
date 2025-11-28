using API.ViewModels;
using Application.Commands.CalculateTax;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class TaxEndpoints
{
    public static void MapTaxEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/tax")
            .WithApiVersionSet(versionSet)
            .WithTags("Tax Calculation")
            .WithOpenApi();

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

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(new { result.Error.Code, result.Error.Message })
                : Results.BadRequest(new { result.Error.Code, result.Error.Message });
        }

        return Results.Ok(result.Value);
    }
}
