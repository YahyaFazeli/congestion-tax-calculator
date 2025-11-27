using Application.Commands.UpdateCity;
using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class UpdateCityCommandHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<ILogger<UpdateCityCommandHandler>> _mockLogger;
    private readonly UpdateCityCommandHandler _handler;

    public UpdateCityCommandHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockLogger = new Mock<ILogger<UpdateCityCommandHandler>>();
        _handler = new UpdateCityCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var oldName = "Stockholm";
        var newName = "Gothenburg";
        var city = City.Create(oldName);

        _mockRepository
            .Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _mockRepository
            .Setup(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new UpdateCityCommand(cityId, newName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(city.Id);
        result.Value.Name.Should().Be(newName);
        _mockRepository.Verify(
            r => r.UpdateAsync(It.Is<City>(c => c.Name == newName), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NullName_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateCityCommand(Guid.NewGuid(), null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("City name cannot be null or whitespace");
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateCityCommand(Guid.NewGuid(), "");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("City name cannot be null or whitespace");
    }

    [Fact]
    public async Task Handle_WhitespaceName_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateCityCommand(Guid.NewGuid(), "   ");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("City name cannot be null or whitespace");
    }

    [Fact]
    public async Task Handle_CityNotFound_ThrowsCityNotFoundException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new UpdateCityCommand(cityId, "Stockholm");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<CityNotFoundException>()
            .WithMessage($"City with ID {cityId} not found");

        exception.Which.CityId.Should().Be(cityId);
    }

    [Fact]
    public async Task Handle_DuplicateCityName_ThrowsValidationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var newName = "Gothenburg";
        var city = City.Create("Stockholm");
        var existingCity = City.Create(newName);

        _mockRepository
            .Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _mockRepository
            .Setup(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var command = new UpdateCityCommand(cityId, newName);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage($"City with name '{newName}' already exists");
    }

    [Fact]
    public async Task Handle_SameCityUpdatingItsOwnName_Succeeds()
    {
        // Arrange
        var city = City.Create("Stockholm");
        var newName = "New Stockholm";

        _mockRepository
            .Setup(r => r.GetByIdAsync(city.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _mockRepository
            .Setup(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var command = new UpdateCityCommand(city.Id, newName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        _mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<City>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var newName = "Stockholm";
        var city = City.Create("Old Name");
        var cancellationToken = new CancellationToken();

        _mockRepository.Setup(r => r.GetByIdAsync(cityId, cancellationToken)).ReturnsAsync(city);

        _mockRepository
            .Setup(r => r.GetByNameAsync(newName, cancellationToken))
            .ReturnsAsync((City?)null);

        var command = new UpdateCityCommand(cityId, newName);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(cityId, cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<City>(), cancellationToken), Times.Once);
    }
}
