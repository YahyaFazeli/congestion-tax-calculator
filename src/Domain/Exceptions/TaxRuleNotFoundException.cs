namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a tax rule is not found for a given city and year.
/// Maps to HTTP 404 status code.
/// </summary>
public class TaxRuleNotFoundException : NotFoundException
{
    /// <summary>
    /// Gets the city identifier that was searched.
    /// </summary>
    public Guid CityId { get; }

    /// <summary>
    /// Gets the year that was searched.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaxRuleNotFoundException"/> class.
    /// </summary>
    /// <param name="cityId">The city identifier that was not found.</param>
    /// <param name="year">The year that was not found.</param>
    public TaxRuleNotFoundException(Guid cityId, int year)
        : base($"No tax rule found for city {cityId} and year {year}")
    {
        CityId = cityId;
        Year = year;
    }
}
