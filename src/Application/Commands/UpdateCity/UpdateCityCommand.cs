using MediatR;

namespace Application.Commands.UpdateCity;

public sealed record UpdateCityCommand(Guid Id, string Name) : IRequest<UpdateCityResult>;
