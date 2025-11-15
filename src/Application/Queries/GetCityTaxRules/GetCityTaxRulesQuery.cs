using MediatR;

namespace Application.Queries.GetCityTaxRules;

public sealed record GetCityTaxRulesQuery(Guid CityId) : IRequest<CityTaxRulesDto?>;
