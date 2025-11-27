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

        try
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
                throw new InvalidOperationException(
                    $"City with name '{request.Name}' already exists"
                );

            city.UpdateName(request.Name);

            await cityRepository.UpdateAsync(city, cancellationToken);

            logger.LogInformation(
                "City updated successfully. CityId: {CityId}, Name: {CityName}",
                city.Id,
                city.Name
            );

            return new UpdateCityResult(city.Id, city.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating city. CityId: {CityId}, NewName: {CityName}",
                request.Id,
                request.Name
            );
            throw;
        }
    }
}
