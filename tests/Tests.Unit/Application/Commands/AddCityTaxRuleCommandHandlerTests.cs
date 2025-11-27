using Application.Commands.AddCityTaxRule;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class AddCityTaxRuleCommandHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<ILogger<AddCityTaxRuleCommandHandler>> _mockLogger;
    private readonly AddCityTaxRuleCommandHandler _handler;

    public AddCityTaxRuleCommandHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockLogger = new Mock<ILogger<AddCityTaxRuleCommandHandler>>();
        _handler = new AddCityTaxRuleCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsTaxRule()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2024;
        var city = new City(cityId, "Stockholm");

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var command = new AddCityTaxRuleCommand(
            cityId,
            year,
            60m,
            60,
            new[] { new TollIntervalDto("06:00", "06:29", 8m) },
            new[] { new TollFreeDateDto("2024-01-01", false) },
            new[] { Month.July },
            new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
            new[] { VehicleType.Motorbike }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CityId.Should().Be(cityId);
        result.Value.Year.Should().Be(year);
        result.Value.RuleId.Should().NotBeEmpty();
        _mockRepository.Verify(
            r => r.AddTaxRuleAsync(It.IsAny<TaxRule>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CityNotFound_ThrowsCityNotFoundException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var command = new AddCityTaxRuleCommand(
            cityId,
            2024,
            60m,
            60,
            Array.Empty<TollIntervalDto>(),
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<CityNotFoundException>()
            .WithMessage($"City with ID {cityId} not found");

        exception.Which.CityId.Should().Be(cityId);
    }

    [Fact]
    public async Task Handle_DuplicateYear_ThrowsValidationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2024;
        var city = new City(cityId, "Stockholm");
        var existingRule = TaxRule.Create(cityId, year, new(60), 60, [], [], [], [], []);
        city.AddRule(existingRule);

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var command = new AddCityTaxRuleCommand(
            cityId,
            year,
            60m,
            60,
            Array.Empty<TollIntervalDto>(),
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage($"Tax rule for year {year} already exists for city 'Stockholm'");
    }

    [Fact]
    public async Task Handle_WithMultipleIntervals_CreatesRuleSuccessfully()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new City(cityId, "Stockholm");

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var command = new AddCityTaxRuleCommand(
            cityId,
            2024,
            60m,
            60,
            new[]
            {
                new TollIntervalDto("06:00", "06:29", 8m),
                new TollIntervalDto("06:30", "06:59", 13m),
                new TollIntervalDto("07:00", "07:59", 18m),
            },
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(
            r => r.AddTaxRuleAsync(It.IsAny<TaxRule>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new City(cityId, "Stockholm");
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, cancellationToken))
            .ReturnsAsync(city);

        var command = new AddCityTaxRuleCommand(
            cityId,
            2024,
            60m,
            60,
            Array.Empty<TollIntervalDto>(),
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetByIdWithRulesAsync(cityId, cancellationToken), Times.Once);
        _mockRepository.Verify(
            r => r.AddTaxRuleAsync(It.IsAny<TaxRule>(), cancellationToken),
            Times.Once
        );
    }
}
