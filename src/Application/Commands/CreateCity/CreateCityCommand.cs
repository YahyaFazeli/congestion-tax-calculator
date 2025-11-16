using MediatR;

namespace Application.Commands.CreateCity;

public sealed record CreateCityCommand(string Name) : IRequest<CreateCityResult>;
