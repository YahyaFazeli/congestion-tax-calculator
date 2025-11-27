using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.GetAllCities;

public class GetAllCitiesQueryHandler(
    ICityRepository cityRepository,
    ILogger<GetAllCitiesQueryHandler> logger
) : IRequestHandler<GetAllCitiesQuery, IEnumerable<CityDto>>
{
    public async Task<IEnumerable<CityDto>> Handle(
        GetAllCitiesQuery request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Retrieving all cities");

        try
        {
            var cities = await cityRepository.GetAllAsync(cancellationToken);
            var cityList = cities.ToList();

            logger.LogInformation("Retrieved {CityCount} cities", cityList.Count);

            return cityList.Select(c => new CityDto(c.Id, c.Name));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all cities");
            throw;
        }
    }
}
