using Application.Queries.GetAllCities;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Queries;

public class GetAllCitiesQueryHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly GetAllCitiesQueryHandler _handler;

    public GetAllCitiesQueryHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _handler = new GetAllCitiesQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllCities()
    {
        // Arrange
        var cities = new[]
        {
            City.Create("Gothenburg"),
            City.Create("Stockholm"),
            City.Create("Malmö"),
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        var query = new GetAllCitiesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(dto => dto.Name == "Gothenburg");
        result.Should().Contain(dto => dto.Name == "Stockholm");
        result.Should().Contain(dto => dto.Name == "Malmö");
    }

    [Fact]
    public async Task Handle_MapsToCityDtoCorrectly()
    {
        // Arrange
        var city = City.Create("Gothenburg");

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { city });

        var query = new GetAllCitiesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Single();
        dto.Id.Should().Be(city.Id);
        dto.Name.Should().Be("Gothenburg");
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<City>());

        var query = new GetAllCitiesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<City>());

        var query = new GetAllCitiesQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Array.Empty<City>());

        var query = new GetAllCitiesQuery();

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }
}
