namespace Application.Commands.AddCityTaxRule;

public sealed record AddCityTaxRuleResult(Guid RuleId, Guid CityId, int Year);
