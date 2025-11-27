using Application.Commands.UpdateCityTaxRule;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Commands;

public class UpdateCityTaxRuleCommandHandlerTests
{
    private readonly Mock<ITaxRuleRepository> _mockRepository;
    private readonly Mock<ILogger<UpdateCityTaxRuleCommandHandler>> _mockLogger;
    private readonly UpdateCityTaxRuleCommandHandler _handler;

    public UpdateCityTaxRuleCommandHandlerTests()
    {
        _mockRepository = new Mock<ITaxRuleRepository>();
        _mockLogger = new Mock<ILogger<UpdateCityTaxRuleCommandHandler>>();
        _handler = new UpdateCityTaxRuleCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesTaxRule()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var year = 2024;
        var existingRule = TaxRule.Create(cityId, year, new(60), 60, [], [], [], [], []);

        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
            year,
            80m,
            60,
            new[] { new TollIntervalDto("06:00", "06:29", 10m) },
            new[] { new TollFreeDateDto("2024-01-01", false) },
            new[] { Month.July },
            new[] { DayOfWeek.Saturday },
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
            r => r.ReplaceRuleAsync(ruleId, It.IsAny<TaxRule>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_RuleNotFound_ReturnsFailureResult()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRule?)null);

        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaxRule.NotFound");
        result.Error.Message.Should().Contain(cityId.ToString());
        result.Error.Message.Should().Contain("2024");
    }

    [Fact]
    public async Task Handle_WrongCityId_ThrowsValidationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var wrongCityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var existingRule = TaxRule.Create(cityId, 2024, new(60), 60, [], [], [], [], []);

        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var command = new UpdateCityTaxRuleCommand(
            wrongCityId,
            ruleId,
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
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage($"Tax rule '{ruleId}' does not belong to city '{wrongCityId}'");
    }

    [Fact]
    public async Task Handle_ChangingYearToDuplicateYear_ThrowsValidationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var existingRule = TaxRule.Create(cityId, 2024, new(60), 60, [], [], [], [], []);
        var duplicateRule = TaxRule.Create(cityId, 2025, new(60), 60, [], [], [], [], []);

        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        _mockRepository
            .Setup(r => r.GetByCityAndYearAsync(cityId, 2025, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateRule);

        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
            2025,
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
            .WithMessage("Another tax rule for year 2025 already exists for this city");
    }

    [Fact]
    public async Task Handle_SameYear_DoesNotCheckForDuplicates()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var year = 2024;
        var existingRule = TaxRule.Create(cityId, year, new(60), 60, [], [], [], [], []);

        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
            year,
            80m,
            60,
            Array.Empty<TollIntervalDto>(),
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
            r =>
                r.GetByCityAndYearAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var existingRule = TaxRule.Create(cityId, 2024, new(60), 60, [], [], [], [], []);
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByIdWithAllRelationsAsync(ruleId, cancellationToken))
            .ReturnsAsync(existingRule);

        var command = new UpdateCityTaxRuleCommand(
            cityId,
            ruleId,
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
        _mockRepository.Verify(
            r => r.GetByIdWithAllRelationsAsync(ruleId, cancellationToken),
            Times.Once
        );
        _mockRepository.Verify(
            r => r.ReplaceRuleAsync(ruleId, It.IsAny<TaxRule>(), cancellationToken),
            Times.Once
        );
    }
}
