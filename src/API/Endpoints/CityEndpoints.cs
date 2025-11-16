using API.ViewModels;
using Application.Commands.AddCityTaxRule;
using Application.Commands.CreateCity;
using Application.Commands.UpdateCity;
using Application.Commands.UpdateCityTaxRule;
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
            .MapPost("", CreateCity)
            .WithName("CreateCity")
            .WithSummary("Create a new city")
            .WithDescription("Creates a new city with the specified name");

        group
            .MapPut("/{cityId:guid}", UpdateCity)
            .WithName("UpdateCity")
            .WithSummary("Update an existing city")
            .WithDescription("Updates the name of an existing city");

        group
            .MapGet("/{cityId:guid}/rules", GetCityTaxRules)
            .WithName("GetCityTaxRules")
            .WithSummary("Get simple tax rules for a city")
            .WithDescription("Returns basic tax rule entities without detailed mapping");

        group
            .MapPost("/{cityId:guid}/rules", AddCityTaxRule)
            .WithName("AddCityTaxRule")
            .WithSummary("Add a new tax rule to a city")
            .WithDescription("Creates a new tax rule for the specified city and year");

        group
            .MapGet("/{cityId:guid}/rules/{ruleId:guid}", GetCityTaxRule)
            .WithName("GetCityTaxRule")
            .WithSummary("Get detailed tax rule for a city")
            .WithDescription(
                "Returns comprehensive tax rule including intervals, free dates, and exemptions"
            );

        group
            .MapPut("/{cityId:guid}/rules/{ruleId:guid}", UpdateCityTaxRule)
            .WithName("UpdateCityTaxRule")
            .WithSummary("Update an existing tax rule")
            .WithDescription("Updates an existing tax rule with new configuration");
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

    private static async Task<IResult> CreateCity(
        [FromBody] CreateCityRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateCityCommand(request.Name);
        var result = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/cities/{result.Id}", result);
    }

    private static async Task<IResult> UpdateCity(
        [FromRoute] Guid cityId,
        [FromBody] UpdateCityRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateCityCommand(cityId, request.Name);
        var result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AddCityTaxRule(
        [FromRoute] Guid cityId,
        [FromBody] AddCityTaxRuleRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var command = new AddCityTaxRuleCommand(
            cityId,
            request.Year,
            request.DailyMax,
            request.SingleChargeMinutes,
            request.Intervals.Select(i => new Application.Commands.AddCityTaxRule.TollIntervalDto(
                i.Start,
                i.End,
                i.Amount
            )),
            request.FreeDates.Select(d => new Application.Commands.AddCityTaxRule.TollFreeDateDto(
                d.Date,
                d.IncludeDayBefore
            )),
            request.FreeMonths,
            request.FreeWeekdays,
            request.FreeVehicles
        );

        var result = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/cities/{cityId}/rules/{result.RuleId}", result);
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

    private static async Task<IResult> UpdateCityTaxRule(
        [FromRoute] Guid cityId,
        [FromRoute] Guid ruleId,
        [FromBody] UpdateCityTaxRuleRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
            request.Year,
            request.DailyMax,
            request.SingleChargeMinutes,
            request.Intervals.Select(
                i => new Application.Commands.UpdateCityTaxRule.TollIntervalDto(
                    i.Start,
                    i.End,
                    i.Amount
                )
            ),
            request.FreeDates.Select(
                d => new Application.Commands.UpdateCityTaxRule.TollFreeDateDto(
                    d.Date,
                    d.IncludeDayBefore
                )
            ),
            request.FreeMonths,
            request.FreeWeekdays,
            request.FreeVehicles
        );

        var result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}
