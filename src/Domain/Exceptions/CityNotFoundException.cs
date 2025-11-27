namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a city is not found.
/// Maps to HTTP 404 status code.
/// </summary>
public class CityNotFoundException : NotFoundException
{
    /// <summary>
    /// Gets the city identifier that was not found.
    /// </summary>
    public Guid CityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CityNotFoundException"/> class.
    /// </summary>
    /// <param name="cityId">The city identifier that was not found.</param>
    public CityNotFoundException(Guid cityId)
        : base($"City with ID {cityId} not found")
    {
        CityId = cityId;
    }
}
