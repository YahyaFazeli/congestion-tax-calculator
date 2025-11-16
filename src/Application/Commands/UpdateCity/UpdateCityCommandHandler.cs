using Domain.Interfaces;
using MediatR;

namespace Application.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(ICityRepository cityRepository)
    : IRequestHandler<UpdateCityCommand, UpdateCityResult>
{
    public async Task<UpdateCityResult> Handle(
        UpdateCityCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException(
                "City name cannot be null or whitespace",
                nameof(request.Name)
            );

        var city = await cityRepository.GetByIdAsync(request.Id, cancellationToken);

        if (city is null)
            throw new InvalidOperationException($"City with ID '{request.Id}' not found");

        var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingCity is not null && existingCity.Id != request.Id)
            throw new InvalidOperationException($"City with name '{request.Name}' already exists");

        city.UpdateName(request.Name);

        await cityRepository.UpdateAsync(city, cancellationToken);

        return new UpdateCityResult(city.Id, city.Name);
    }
}
