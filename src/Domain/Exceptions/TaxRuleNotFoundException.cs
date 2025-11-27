namespace Domain.Exceptions;

public class TaxRuleNotFoundException : NotFoundException
{
    public Guid CityId { get; }

    public int Year { get; }

    public TaxRuleNotFoundException(Guid cityId, int year)
        : base($"No tax rule found for city {cityId} and year {year}")
    {
        CityId = cityId;
        Year = year;
    }
}
