using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using Tests.Integration.TestHelpers;

namespace Tests.Integration.Repositories;

public class TaxRuleRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;

    public TaxRuleRepositoryTests()
    {
        _fixture = new DatabaseFixture();
    }

    [Fact]
    public async Task GetByCityAndYearAsync_ExistingRule_ReturnsRuleWithRelations()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await cityRepository.AddAsync(city);

        // Act
        var result = await taxRuleRepository.GetByCityAndYearAsync(city.Id, 2013);

        // Assert
        result.Should().NotBeNull();
        result!.Year.Should().Be(2013);
        result.CityId.Should().Be(city.Id);
        result.Intervals.Should().NotBeEmpty();
        result.TollFreeDates.Should().NotBeEmpty();
        result.TollFreeMonths.Should().NotBeEmpty();
        result.TollFreeWeekdays.Should().NotBeEmpty();
        result.TollFreeVehicles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByCityAndYearAsync_NonExistingYear_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await cityRepository.AddAsync(city);

        // Act
        var result = await taxRuleRepository.GetByCityAndYearAsync(city.Id, 2014);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityAndYearAsync_NonExistingCity_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(context);

        // Act
        var result = await taxRuleRepository.GetByCityAndYearAsync(Guid.NewGuid(), 2013);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityIdAsync_CityWithMultipleRules_ReturnsAllRules()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013, 2014, 2015);
        await cityRepository.AddAsync(city);

        // Act
        var result = await taxRuleRepository.GetByCityIdAsync(city.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Year == 2013);
        result.Should().Contain(r => r.Year == 2014);
        result.Should().Contain(r => r.Year == 2015);

        // Verify all relations are loaded
        foreach (var rule in result)
        {
            rule.Intervals.Should().NotBeEmpty();
            rule.TollFreeDates.Should().NotBeEmpty();
            rule.TollFreeMonths.Should().NotBeEmpty();
            rule.TollFreeWeekdays.Should().NotBeEmpty();
            rule.TollFreeVehicles.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetByCityIdAsync_NonExistingCity_ReturnsEmptyList()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(context);

        // Act
        var result = await taxRuleRepository.GetByCityIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWithAllRelationsAsync_ExistingRule_ReturnsWithAllRelations()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await cityRepository.AddAsync(city);
        var ruleId = city.TaxRules.First().Id;

        // Act
        var result = await taxRuleRepository.GetByIdWithAllRelationsAsync(ruleId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ruleId);
        result.Intervals.Should().HaveCount(3);
        result.TollFreeDates.Should().HaveCount(2);
        result.TollFreeMonths.Should().HaveCount(1);
        result.TollFreeWeekdays.Should().HaveCount(2);
        result.TollFreeVehicles.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdWithAllRelationsAsync_NonExistingRule_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(context);

        // Act
        var result = await taxRuleRepository.GetByIdWithAllRelationsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleRules_ReturnsAllWithRelations()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city1 = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013, 2014);
        var city2 = TestDataBuilder.CreateCityWithRules("Stockholm", 2013);
        await cityRepository.AddAsync(city1);
        await cityRepository.AddAsync(city2);

        // Act
        var result = await taxRuleRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);

        // Verify all relations are loaded
        foreach (var rule in result)
        {
            rule.Intervals.Should().NotBeEmpty();
            rule.TollFreeDates.Should().NotBeEmpty();
            rule.TollFreeMonths.Should().NotBeEmpty();
            rule.TollFreeWeekdays.Should().NotBeEmpty();
            rule.TollFreeVehicles.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRule_ReturnsRule()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await cityRepository.AddAsync(city);
        var ruleId = city.TaxRules.First().Id;

        // Act
        var result = await taxRuleRepository.GetByIdAsync(ruleId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ruleId);
    }

    [Fact]
    public async Task ExistsAsync_ExistingRule_ReturnsTrue()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context);
        var taxRuleRepository = new TaxRuleRepository(context);

        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await cityRepository.AddAsync(city);
        var ruleId = city.TaxRules.First().Id;

        // Act
        var result = await taxRuleRepository.ExistsAsync(ruleId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingRule_ReturnsFalse()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(context);

        // Act
        var result = await taxRuleRepository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }
}
