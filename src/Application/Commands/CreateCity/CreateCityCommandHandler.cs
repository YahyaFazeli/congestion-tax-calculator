using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.CreateCity;

public sealed class CreateCityCommandHandler(
    ICityRepository cityRepository,
    ILogger<CreateCityCommandHandler> logger
) : IRequestHandler<CreateCityCommand, CreateCityResult>
{
    public async Task<CreateCityResult> Handle(
        CreateCityCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Creating city with name: {CityName}", request.Name);

        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException(
                    "City name cannot be null or whitespace",
                    nameof(request.Name)
                );

            var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

            if (existingCity is not null)
                throw new InvalidOperationException(
                    $"City with name '{request.Name}' already exists"
                );

            var city = City.Create(request.Name);

            await cityRepository.AddAsync(city, cancellationToken);

            logger.LogInformation(
                "City created successfully. CityId: {CityId}, Name: {CityName}",
                city.Id,
                city.Name
            );

            return new CreateCityResult(city.Id, city.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating city with name: {CityName}", request.Name);
            throw;
        }
    }
}
