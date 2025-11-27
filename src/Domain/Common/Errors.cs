namespace Domain.Common;

public static class Errors
{
    public static class City
    {
        public static Error NotFound(Guid cityId) =>
            new("City.NotFound", $"City with ID {cityId} not found");

        public static Error AlreadyExists(string name) =>
            new("City.AlreadyExists", $"City with name '{name}' already exists");
    }

    public static class TaxRule
    {
        public static Error NotFound(Guid cityId, int year) =>
            new("TaxRule.NotFound", $"No tax rule found for city {cityId} and year {year}");

        public static Error AlreadyExists(int year) =>
            new("TaxRule.AlreadyExists", $"Tax rule for year {year} already exists");

        public static Error WrongCity(Guid ruleId, Guid expectedCityId) =>
            new(
                "TaxRule.WrongCity",
                $"Tax rule '{ruleId}' does not belong to city '{expectedCityId}'"
            );
    }
}
