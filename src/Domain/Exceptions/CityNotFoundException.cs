namespace Domain.Exceptions;

public class CityNotFoundException : NotFoundException
{
    public Guid CityId { get; }

    public CityNotFoundException(Guid cityId)
        : base($"City with ID {cityId} not found")
    {
        CityId = cityId;
    }
}
