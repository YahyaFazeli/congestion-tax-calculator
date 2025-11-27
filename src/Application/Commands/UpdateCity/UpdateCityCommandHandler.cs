using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(
    ICityRepository cityRepository,
    ILogger<UpdateCityCommandHandler> logger
) : IRequestHandler<UpdateCityCommand, UpdateCityResult>
{
    public async Task<UpdateCityResult> Handle(
        UpdateCityCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Updating city. CityId: {CityId}, NewName: {CityName}",
            request.Id,
            request.Name
        );

        var city = await cityRepository.GetByIdAsync(request.Id, cancellationToken);

        if (city is null)
        {
            throw new CityNotFoundException(request.Id);
        }

        var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingCity is not null && existingCity.Id != request.Id)
        {
            throw new ValidationException(
                $"City with name '{request.Name}' already exists",
                new Dictionary<string, string[]>
                {
                    {
                        nameof(request.Name),
                        new[] { $"City with name '{request.Name}' already exists" }
                    },
                }
            );
        }

        city.UpdateName(request.Name);

        await cityRepository.UpdateAsync(city, cancellationToken);

        logger.LogInformation(
            "City updated successfully. CityId: {CityId}, Name: {CityName}",
            city.Id,
            city.Name
        );

        return new UpdateCityResult(city.Id, city.Name);
    }
}
