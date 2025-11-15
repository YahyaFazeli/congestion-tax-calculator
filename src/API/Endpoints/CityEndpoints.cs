using Application.Queries.GetAllCities;
using Application.Queries.GetCityTaxRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class CityEndpoints
{
    public static void MapCityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cities").WithTags("Cities").WithOpenApi();

        group
            .MapGet("", GetAllCities)
            .WithName("GetAllCities")
            .WithSummary("Get all cities")
            .WithDescription("Returns a list of all cities with their IDs and names");

        group
            .MapGet("/{cityId:guid}/rules", GetCityTaxRules)
            .WithName("GetCityTaxRules")
            .WithSummary("Get simple tax rules for a city")
            .WithDescription("Returns basic tax rule entities without detailed mapping");

        group
            .MapGet("/{cityId:guid}/rules/{ruleId:guid}", GetCityTaxRule)
            .WithName("GetCityTaxRule")
            .WithSummary("Get detailed tax rule for a city")
            .WithDescription(
                "Returns comprehensive tax rule including intervals, free dates, and exemptions"
            );
    }

    private static async Task<IResult> GetAllCities(
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var query = new GetAllCitiesQuery();
        var result = await sender.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetCityTaxRules(
        [FromRoute] Guid cityId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCityTaxRulesQuery(cityId);
        var result = await sender.Send(query, cancellationToken);

        return result is not null
            ? Results.Ok(result)
            : Results.NotFound(new { Message = $"City with ID {cityId} not found" });
    }

    private static async Task<IResult> GetCityTaxRule(
        [FromRoute] Guid cityId,
        [FromRoute] Guid ruleId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCityTaxRuleQuery(cityId, ruleId);
        var result = await sender.Send(query, cancellationToken);

        return result is not null
            ? Results.Ok(result)
            : Results.NotFound(new { Message = $"City Rule with ID {ruleId} not found" });
    }
}
