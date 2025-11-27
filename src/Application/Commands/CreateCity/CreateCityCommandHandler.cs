using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.CreateCity;

public sealed class CreateCityCommandHandler(
    ICityRepository cityRepository,
    ILogger<CreateCityCommandHandler> logger
) : IRequestHandler<CreateCityCommand, Result<CreateCityResult>>
{
    public async Task<Result<CreateCityResult>> Handle(
        CreateCityCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Creating city with name: {CityName}", request.Name);

        var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingCity is not null)
        {
            logger.LogWarning("City with name already exists. Name: {CityName}", request.Name);
            return Result.Failure<CreateCityResult>(Errors.City.AlreadyExists(request.Name));
        }

        var city = City.Create(request.Name);

        await cityRepository.AddAsync(city, cancellationToken);
        await cityRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "City created successfully. CityId: {CityId}, Name: {CityName}",
            city.Id,
            city.Name
        );

        return Result.Success(new CreateCityResult(city.Id, city.Name));
    }
}
