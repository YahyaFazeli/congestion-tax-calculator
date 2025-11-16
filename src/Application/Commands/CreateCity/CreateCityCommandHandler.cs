using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.CreateCity;

public sealed class CreateCityCommandHandler(ICityRepository cityRepository)
    : IRequestHandler<CreateCityCommand, CreateCityResult>
{
    public async Task<CreateCityResult> Handle(
        CreateCityCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException(
                "City name cannot be null or whitespace",
                nameof(request.Name)
            );

        var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingCity is not null)
            throw new InvalidOperationException($"City with name '{request.Name}' already exists");

        var city = City.Create(request.Name);

        await cityRepository.AddAsync(city, cancellationToken);

        return new CreateCityResult(city.Id, city.Name);
    }
}
