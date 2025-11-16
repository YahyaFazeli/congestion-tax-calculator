using Application.Commands.CreateCity;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Commands;

public class CreateCityCommandHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly CreateCityCommandHandler _handler;

    public CreateCityCommandHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _handler = new CreateCityCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCity()
    {
        // Arrange
        var cityName = "Stockholm";
        _mockRepository
            .Setup(r => r.GetByNameAsync(cityName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new CreateCityCommand(cityName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be(cityName);
        result.Id.Should().NotBeEmpty();
        _mockRepository.Verify(
            r => r.AddAsync(It.Is<City>(c => c.Name == cityName), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NullName_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateCityCommand(null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("City name cannot be null or whitespace*");
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateCityCommand("");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("City name cannot be null or whitespace*");
    }

    [Fact]
    public async Task Handle_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateCityCommand("   ");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("City name cannot be null or whitespace*");
    }

    [Fact]
    public async Task Handle_DuplicateCityName_ThrowsInvalidOperationException()
    {
        // Arrange
        var cityName = "Stockholm";
        var existingCity = City.Create(cityName);

        _mockRepository
            .Setup(r => r.GetByNameAsync(cityName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var command = new CreateCityCommand(cityName);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"City with name '{cityName}' already exists");
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityName = "Stockholm";
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByNameAsync(cityName, cancellationToken))
            .ReturnsAsync((City?)null);

        var command = new CreateCityCommand(cityName);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetByNameAsync(cityName, cancellationToken), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<City>(), cancellationToken), Times.Once);
    }
}
