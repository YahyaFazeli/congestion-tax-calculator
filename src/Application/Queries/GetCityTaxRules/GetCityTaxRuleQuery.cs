using Domain.Entities;
using MediatR;

namespace Application.Queries.GetCityTaxRules;

public sealed record GetCityTaxRuleQuery(Guid CityId, Guid RuleId) : IRequest<CityTaxRuleDto?>;
