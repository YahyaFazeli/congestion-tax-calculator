using Application.Commands.CalculateTax;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class CalculateTaxCommandHandlerTests
{
    private readonly Mock<ITaxRuleRepository> _mockRepository;
    private readonly Mock<ITaxCalculator> _mockCalculator;
    private readonly Mock<ILogger<CalculateTaxCommandHandler>> _mockLogger;
    private readonly CalculateTaxCommandHandler _handler;

    public CalculateTaxCommandHandlerTests()
    {
        _mockRepository = new Mock<ITaxRuleRepository>();
        _mockCalculator = new Mock<ITaxCalculator>();
        _mockLogger = new Mock<ILogger<CalculateTaxCommandHandler>>();
        _handler = new CalculateTaxCommandHandler(
            _mockRepository.Object,
            _mockCalculator.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCalculatedTax()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);
        var expectedTax = new Money(21);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(expectedTax);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalTax.Should().Be(21);
        result.Value.Currency.Should().Be("SEK");
    }

    [Fact]
    public async Task Handle_CityNotFound_ReturnsFailureResult()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRule?)null);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaxRule.NotFound");
        result.Error.Message.Should().Contain(cityId.ToString());
        result.Error.Message.Should().Contain(year.ToString());
    }

    [Fact]
    public async Task Handle_YearNotFound_ReturnsFailureResult()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2014;

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRule?)null);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2014, 2, 7, 8, 0, 0) }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaxRule.NotFound");
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(Money.Zero);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CallsCalculatorWithCorrectParameters()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);
        var timestamps = new[] { new DateTime(2013, 2, 7, 8, 0, 0) };

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(Money.Zero);

        var command = new CalculateTaxCommand(cityId, year, "ABC123", VehicleType.Car, timestamps);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockCalculator.Verify(
            c =>
                c.Calculate(
                    rule,
                    It.Is<Vehicle>(v => v.Registration == "ABC123" && v.Type == VehicleType.Car),
                    timestamps
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ExemptVehicle_ReturnsZero()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(Money.Zero);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Motorbike,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalTax.Should().Be(0);
    }

    [Fact]
    public async Task Handle_EmptyTimestamps_ReturnsZero()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(Money.Zero);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            Array.Empty<DateTime>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalTax.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, cancellationToken))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(Money.Zero);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetByCityAndYearAsync(cityId, year, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_LogsInformation_WhenCalculatingTax()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(new Money(21));

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Calculating tax")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_DoesNotLogWarning_WhenRuleNotFound()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRule?)null);

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (TaxRuleNotFoundException)
        {
            // Expected exception - now handled by global handler
        }

        // Assert - The warning is now logged by the global exception handler, not here
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No tax rule found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_LogsSuccess_WhenTaxCalculatedSuccessfully()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;
        var rule = CreateTestRule(cityId, year);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        _mockCalculator
            .Setup(c =>
                c.Calculate(
                    It.IsAny<TaxRule>(),
                    It.IsAny<Vehicle>(),
                    It.IsAny<IEnumerable<DateTime>>()
                )
            )
            .Returns(new Money(21));

        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Tax calculated successfully")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    private static TaxRule CreateTestRule(Guid cityId, int year)
    {
        return TaxRule.Create(cityId, year, new Money(60), 60, [], [], [], [], []);
    }
}
