using Application.Queries.GetCityTaxRules;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.Queries;

public class GetCityTaxRulesQueryHandlerTests
{
    private readonly Mock<ICityRepository> _mockRepository;
    private readonly Mock<ILogger<GetCityTaxRulesQueryHandler>> _mockLogger;
    private readonly GetCityTaxRulesQueryHandler _handler;

    public GetCityTaxRulesQueryHandlerTests()
    {
        _mockRepository = new Mock<ICityRepository>();
        _mockLogger = new Mock<ILogger<GetCityTaxRulesQueryHandler>>();
        _handler = new GetCityTaxRulesQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCityId_ReturnsRules()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule2013 = CreateTestRule(city.Id, 2013);
        var rule2014 = CreateTestRule(city.Id, 2014);
        city.AddRule(rule2013);
        city.AddRule(rule2014);

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CityId.Should().Be(city.Id);
        result.CityName.Should().Be("Gothenburg");
        result.Rules.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_InvalidCityId_ReturnsNull()
    {
        // Arrange
        var cityId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CityWithMultipleYears_ReturnsAllRules()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule2013 = CreateTestRule(city.Id, 2013);
        var rule2014 = CreateTestRule(city.Id, 2014);
        var rule2015 = CreateTestRule(city.Id, 2015);
        city.AddRule(rule2013);
        city.AddRule(rule2014);
        city.AddRule(rule2015);

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Rules.Should().HaveCount(3);
        result.Rules.Should().Contain(r => r.Year == 2013);
        result.Rules.Should().Contain(r => r.Year == 2014);
        result.Rules.Should().Contain(r => r.Year == 2015);
    }

    [Fact]
    public async Task Handle_MapsToDtoCorrectly()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = City.Create("Gothenburg");
        var rule = CreateTestRule(city.Id, 2013);
        city.AddRule(rule);

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var query = new GetCityTaxRulesQuery(cityId);

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
    public async Task Handle_CityWithNoRules_ReturnsEmptyRulesList()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = City.Create("Gothenburg");

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Rules.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        var cityId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetByIdWithRulesAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToRepository()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.GetByIdWithRulesAsync(cityId, cancellationToken))
            .ReturnsAsync((City?)null);

        var query = new GetCityTaxRulesQuery(cityId);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetByIdWithRulesAsync(cityId, cancellationToken), Times.Once);
    }

    private static TaxRule CreateTestRule(Guid cityId, int year)
    {
        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
            TollInterval.Create(new TimeOnly(6, 30), new TimeOnly(6, 59), new Money(13)),
            TollInterval.Create(new TimeOnly(7, 0), new TimeOnly(7, 59), new Money(18)),
        };
        return TaxRule.Create(cityId, year, new Money(60), 60, intervals, [], [], [], []);
    }
}
