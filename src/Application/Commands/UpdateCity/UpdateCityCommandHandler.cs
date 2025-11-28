using Domain.Common;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(
    ICityRepository cityRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCityCommandHandler> logger
) : IRequestHandler<UpdateCityCommand, Result<UpdateCityResult>>
{
    public async Task<Result<UpdateCityResult>> Handle(
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
            logger.LogWarning("City not found. CityId: {CityId}", request.Id);
            return Result.Failure<UpdateCityResult>(Errors.City.NotFound(request.Id));
        }

        var existingCity = await cityRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingCity is not null && existingCity.Id != request.Id)
        {
            logger.LogWarning(
                "City with name already exists. Name: {CityName}, ExistingCityId: {ExistingCityId}",
                request.Name,
                existingCity.Id
            );
            return Result.Failure<UpdateCityResult>(Errors.City.AlreadyExists(request.Name));
        }

        city.UpdateName(request.Name);

        await cityRepository.UpdateAsync(city, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "City updated successfully. CityId: {CityId}, Name: {CityName}",
            city.Id,
            city.Name
        );

        return Result.Success(new UpdateCityResult(city.Id, city.Name));
    }
}
