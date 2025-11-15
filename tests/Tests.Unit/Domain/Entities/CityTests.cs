using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Unit.Domain.Entities;

public class CityTests
{
    [Fact]
    public void Create_WithValidName_CreatesCity()
    {
        // Arrange & Act
        var city = City.Create("Gothenburg");

        // Assert
        city.Name.Should().Be("Gothenburg");
        city.Id.Should().NotBeEmpty();
        city.TaxRules.Should().BeEmpty();
    }

    [Fact]
    public void AddRule_WithValidRule_AddsRuleToCollection()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var rule = CreateTestRule(city.Id, 2013);

        // Act
        city.AddRule(rule);

        // Assert
        city.TaxRules.Should().HaveCount(1);
        city.TaxRules.Should().Contain(rule);
    }

    [Fact]
    public void AddRule_WithMultipleRules_AddsAllRules()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var rule2013 = CreateTestRule(city.Id, 2013);
        var rule2014 = CreateTestRule(city.Id, 2014);

        // Act
        city.AddRule(rule2013);
        city.AddRule(rule2014);

        // Assert
        city.TaxRules.Should().HaveCount(2);
        city.TaxRules.Should().Contain(rule2013);
        city.TaxRules.Should().Contain(rule2014);
    }

    [Fact]
    public void GetRuleForYear_WithExistingYear_ReturnsRule()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var rule2013 = CreateTestRule(city.Id, 2013);
        var rule2014 = CreateTestRule(city.Id, 2014);
        city.AddRule(rule2013);
        city.AddRule(rule2014);

        // Act
        var result = city.GetRuleForYear(2013);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(rule2013);
        result!.Year.Should().Be(2013);
    }

    [Fact]
    public void GetRuleForYear_WithNonExistingYear_ReturnsNull()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var rule2013 = CreateTestRule(city.Id, 2013);
        city.AddRule(rule2013);

        // Act
        var result = city.GetRuleForYear(2014);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRuleForYear_WithNoRules_ReturnsNull()
    {
        // Arrange
        var city = City.Create("Gothenburg");

        // Act
        var result = city.GetRuleForYear(2013);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        // Arrange
        var city = City.Create("Gothenburg");

        // Act
        var result = city.ToString();

        // Assert
        result.Should().Be("Gothenburg");
    }

    private static TaxRule CreateTestRule(Guid cityId, int year)
    {
        return TaxRule.Create(cityId, year, new Money(60), 60, [], [], [], [], []);
    }
}
