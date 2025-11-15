using Domain.Interfaces;
using MediatR;

namespace Application.Queries.GetAllCities;

public class GetAllCitiesQueryHandler(ICityRepository cityRepository)
    : IRequestHandler<GetAllCitiesQuery, IEnumerable<CityDto>>
{
    public async Task<IEnumerable<CityDto>> Handle(
        GetAllCitiesQuery request,
        CancellationToken cancellationToken
    )
    {
        var cities = await cityRepository.GetAllAsync(cancellationToken);
        return cities.Select(c => new CityDto(c.Id, c.Name));
    }
}
