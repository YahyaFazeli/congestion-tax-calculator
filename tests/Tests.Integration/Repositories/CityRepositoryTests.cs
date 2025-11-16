using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using Tests.Integration.TestHelpers;

namespace Tests.Integration.Repositories;

public class CityRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;

    public CityRepositoryTests()
    {
        _fixture = new DatabaseFixture();
    }

    [Fact]
    public async Task AddAsync_ValidCity_PersistsToDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("Gothenburg");

        // Act
        var result = await repository.AddAsync(city);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(city.Id);
        result.Name.Should().Be("Gothenburg");

        // Verify it's in database
        var retrieved = await repository.GetByIdAsync(city.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Gothenburg");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCity_ReturnsCity()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("Stockholm");
        await repository.AddAsync(city);

        // Act
        var result = await repository.GetByIdAsync(city.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(city.Id);
        result.Name.Should().Be("Stockholm");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCity_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleCities_ReturnsAllWithRules()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city1 = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        var city2 = TestDataBuilder.CreateCityWithRules("Stockholm", 2014);
        await repository.AddAsync(city1);
        await repository.AddAsync(city2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Gothenburg");
        result.Should().Contain(c => c.Name == "Stockholm");

        // Verify rules are loaded
        var gothenburg = result.First(c => c.Name == "Gothenburg");
        gothenburg.TaxRules.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByNameAsync_ExistingCity_ReturnsCity()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("Malmö");
        await repository.AddAsync(city);

        // Act
        var result = await repository.GetByNameAsync("Malmö");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Malmö");
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingCity_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);

        // Act
        var result = await repository.GetByNameAsync("NonExisting");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithRulesAsync_CityWithRules_ReturnsWithEagerLoadedRules()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013, 2014);
        await repository.AddAsync(city);

        // Act
        var result = await repository.GetByIdWithRulesAsync(city.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TaxRules.Should().HaveCount(2);
        result.TaxRules.Should().Contain(r => r.Year == 2013);
        result.TaxRules.Should().Contain(r => r.Year == 2014);
    }

    [Fact]
    public async Task GetByIdWithDetailedRulesAsync_ValidIds_ReturnsWithAllRelations()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateCityWithRules("Gothenburg", 2013);
        await repository.AddAsync(city);
        var ruleId = city.TaxRules.First().Id;

        // Act
        var result = await repository.GetByIdWithDetailedRulesAsync(city.Id, ruleId);

        // Assert
        result.Should().NotBeNull();
        result!.TaxRules.Should().HaveCount(1);

        var rule = result.TaxRules.First();
        rule.Intervals.Should().NotBeEmpty();
        rule.TollFreeDates.Should().NotBeEmpty();
        rule.TollFreeMonths.Should().NotBeEmpty();
        rule.TollFreeWeekdays.Should().NotBeEmpty();
        rule.TollFreeVehicles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddAsync_CityWithRules_PersistsAllEntities()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateCityWithRules("TestCity", 2013, 2014);

        // Act
        await repository.AddAsync(city);

        // Assert
        context.ChangeTracker.Clear();
        var retrieved = await repository.GetByIdWithRulesAsync(city.Id);
        retrieved.Should().NotBeNull();
        retrieved!.TaxRules.Should().HaveCount(2);
        retrieved.TaxRules.Should().Contain(r => r.Year == 2013);
        retrieved.TaxRules.Should().Contain(r => r.Year == 2014);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCity_RemovesFromDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("ToDelete");
        await repository.AddAsync(city);

        // Act
        await repository.DeleteAsync(city.Id);

        // Assert
        var retrieved = await repository.GetByIdAsync(city.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingCity_ReturnsTrue()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("Exists");
        await repository.AddAsync(city);

        // Act
        var result = await repository.ExistsAsync(city.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingCity_ReturnsFalse()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);

        // Act
        var result = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddTaxRuleAsync_ValidRule_PersistsToDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("Stockholm");
        await repository.AddAsync(city);

        var taxRule = TestDataBuilder.CreateTestRule(city.Id, 2024);

        // Act
        var result = await repository.AddTaxRuleAsync(taxRule);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(taxRule.Id);
        result.Year.Should().Be(2024);

        // Verify it's in database
        context.ChangeTracker.Clear();
        var retrieved = await repository.GetByIdWithRulesAsync(city.Id);
        retrieved.Should().NotBeNull();
        retrieved!.TaxRules.Should().HaveCount(1);
        retrieved.TaxRules.First().Year.Should().Be(2024);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCity_UpdatesInDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new CityRepository(context);
        var city = TestDataBuilder.CreateTestCity("OldName");
        await repository.AddAsync(city);

        // Act
        city.UpdateName("NewName");
        await repository.UpdateAsync(city);

        // Assert
        context.ChangeTracker.Clear();
        var retrieved = await repository.GetByIdAsync(city.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("NewName");
    }

    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }
}
