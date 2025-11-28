using Application.Commands.AddCityTaxRule;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class AddCityTaxRuleCommandHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<AddCityTaxRuleCommandHandler>> _mockLogger;
    private readonly AddCityTaxRuleCommandHandler _handler;

    public AddCityTaxRuleCommandHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<AddCityTaxRuleCommandHandler>>();
        _handler = new AddCityTaxRuleCommandHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
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
        city.TaxRules.Should().HaveCount(1);
        city.TaxRules.First().Year.Should().Be(year);
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
            new[] { new TollIntervalDto("06:00", "06:29", 8m) },
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("City.NotFound");
        result.Error.Message.Should().Contain(cityId.ToString());
    }

    [Fact]
    public async Task Handle_DuplicateYear_ThrowsValidationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2024;
        var city = new City(cityId, "Stockholm");
        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
        };
        var existingRule = TaxRule.Create(cityId, year, new(60), 60, intervals, [], [], [], []);
        city.AddRule(existingRule);

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var command = new AddCityTaxRuleCommand(
            cityId,
            year,
            60m,
            60,
            new[] { new TollIntervalDto("06:00", "06:29", 8m) },
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaxRule.AlreadyExists");
        result.Error.Message.Should().Contain(year.ToString());
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
        city.TaxRules.Should().HaveCount(1);
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
            new[] { new TollIntervalDto("06:00", "06:29", 8m) },
            Array.Empty<TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetByIdWithRulesAsync(cityId, cancellationToken), Times.Once);
        city.TaxRules.Should().HaveCount(1);
    }
}
