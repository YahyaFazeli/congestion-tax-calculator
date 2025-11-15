using MediatR;

namespace Application.Queries.GetAllCities;

public record GetAllCitiesQuery : IRequest<IEnumerable<CityDto>>;
