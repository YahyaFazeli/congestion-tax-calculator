using Application.Queries.GetCityTaxRules;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Queries;

public class GetCityTaxRuleQueryHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly GetCityTaxRuleQueryHandler _handler;

    public GetCityTaxRuleQueryHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _handler = new GetCityTaxRuleQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidIds_ReturnsDetailedRule()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule = CreateDetailedTestRule(city.Id, ruleId, 2013);
        city.AddRule(rule);

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(city);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CityId.Should().Be(city.Id);
        result.CityName.Should().Be("Gothenburg");
        result.Rules.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_InvalidCityId_ReturnsNull()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvalidRuleId_ReturnsNull()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_IncludesAllRelations()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule = CreateDetailedTestRule(city.Id, ruleId, 2013);
        city.AddRule(rule);

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(city);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var ruleDto = result!.Rules.Single();
        ruleDto.Intervals.Should().HaveCount(2);
        ruleDto.TollFreeDates.Should().HaveCount(1);
        ruleDto.TollFreeMonths.Should().HaveCount(1);
        ruleDto.TollFreeWeekdays.Should().HaveCount(2);
        ruleDto.TollFreeVehicles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_MapsToDtoCorrectly()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule = CreateDetailedTestRule(city.Id, ruleId, 2013);
        city.AddRule(rule);

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(city);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var ruleDto = result!.Rules.Single();
        ruleDto.Id.Should().Be(rule.Id);
        ruleDto.Year.Should().Be(2013);
        ruleDto.DailyMax.Should().Be(60);
        ruleDto.SingleChargeMinutes.Should().Be(60);
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        _mockRepository
            .Setup(r =>
                r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetByIdWithDetailedRulesAsync(cityId, ruleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByIdWithDetailedRulesAsync(cityId, ruleId, cancellationToken))
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRuleQuery(cityId, ruleId);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetByIdWithDetailedRulesAsync(cityId, ruleId, cancellationToken),
            Times.Once
        );
    }

    private static TaxRule CreateDetailedTestRule(Guid cityId, Guid ruleId, int year)
    {
        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), new Money(13)),
        };

        var holidays = new[] { TollFreeDate.Create(new DateOnly(2013, 3, 29), true) };

        var freeMonths = new[] { TollFreeMonth.Create(Month.July) };

        var freeWeekdays = new[]
        {
            TollFreeWeekday.Create(DayOfWeek.Saturday),
            TollFreeWeekday.Create(DayOfWeek.Sunday),
        };

        var exemptVehicles = new[] { TollFreeVehicle.Create(ruleId, VehicleType.Motorbike) };

        return new TaxRule(
            ruleId,
            cityId,
            year,
            new Money(60),
            60,
            intervals,
            holidays,
            freeMonths,
            freeWeekdays,
            exemptVehicles
        );
    }
}
