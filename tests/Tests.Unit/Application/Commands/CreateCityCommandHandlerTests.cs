using Application.Commands.CreateCity;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class CreateCityCommandHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<CreateCityCommandHandler>> _mockLogger;
    private readonly CreateCityCommandHandler _handler;

    public CreateCityCommandHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<CreateCityCommandHandler>>();
        _handler = new CreateCityCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
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
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(cityName);
        result.Value.Id.Should().NotBeEmpty();
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("City.AlreadyExists");
        result.Error.Message.Should().Contain(cityName);
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

    [Fact]
    public async Task Handle_LogsInformation_WhenCreatingCity()
    {
        // Arrange
        var cityName = "Stockholm";
        _mockRepository
            .Setup(r => r.GetByNameAsync(cityName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new CreateCityCommand(cityName);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating city")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_LogsSuccess_WhenCityCreatedSuccessfully()
    {
        // Arrange
        var cityName = "Stockholm";
        _mockRepository
            .Setup(r => r.GetByNameAsync(cityName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new CreateCityCommand(cityName);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("City created successfully")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
