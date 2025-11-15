using Application.Commands.CalculateTax;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Commands;

public class CalculateTaxCommandHandlerTests
{
    private readonly Mock<ITaxRuleRepository> _mockRepository;
    private readonly Mock<ITaxCalculator> _mockCalculator;
    private readonly CalculateTaxCommandHandler _handler;

    public CalculateTaxCommandHandlerTests()
    {
        _mockRepository = new Mock<ITaxRuleRepository>();
        _mockCalculator = new Mock<ITaxCalculator>();
        _handler = new CalculateTaxCommandHandler(_mockRepository.Object, _mockCalculator.Object);
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
        result.TotalTax.Should().Be(21);
        result.Currency.Should().Be("SEK");
    }

    [Fact]
    public async Task Handle_CityNotFound_ThrowsInvalidOperationException()
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
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"No tax rule found for city {cityId} and year {year}");
    }

    [Fact]
    public async Task Handle_YearNotFound_ThrowsInvalidOperationException()
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
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
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
        result.TotalTax.Should().Be(0);
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
        result.TotalTax.Should().Be(0);
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

    private static TaxRule CreateTestRule(Guid cityId, int year)
    {
        return TaxRule.Create(cityId, year, new Money(60), 60, [], [], [], [], []);
    }
}
