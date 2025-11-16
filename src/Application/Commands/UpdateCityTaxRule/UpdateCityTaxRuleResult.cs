namespace Application.Commands.UpdateCityTaxRule;

public sealed record UpdateCityTaxRuleResult(Guid RuleId, Guid CityId, int Year);
