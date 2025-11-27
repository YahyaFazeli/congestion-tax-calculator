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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ThrowsArgumentException(string? name)
    {
        // Act
        var act = () => City.Create(name!);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*City name cannot be null or whitespace*");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => City.Create(longName);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*City name cannot exceed 100 characters*");
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => new City(Guid.Empty, "Gothenburg");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*City ID cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithNullOrWhitespaceName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var city = City.Create("Gothenburg");

        // Act
        var act = () => city.UpdateName(name!);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*City name cannot be null or whitespace*");
    }

    [Fact]
    public void UpdateName_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var longName = new string('A', 101);

        // Act
        var act = () => city.UpdateName(longName);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*City name cannot exceed 100 characters*");
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesCityName()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var newName = "Stockholm";

        // Act
        city.UpdateName(newName);

        // Assert
        city.Name.Should().Be(newName);
    }

    [Fact]
    public void AddRule_WithNullRule_ThrowsArgumentNullException()
    {
        // Arrange
        var city = City.Create("Gothenburg");

        // Act
        var act = () => city.AddRule(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*Tax rule cannot be null*");
    }

    [Fact]
    public void AddRule_WithRuleForDifferentCity_ThrowsArgumentException()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var differentCityId = Guid.NewGuid();
        var rule = CreateTestRule(differentCityId, 2013);

        // Act
        var act = () => city.AddRule(rule);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Tax rule belongs to a different city*");
    }

    [Fact]
    public void AddRule_WithDuplicateYear_ThrowsInvalidOperationException()
    {
        // Arrange
        var city = City.Create("Gothenburg");
        var rule1 = CreateTestRule(city.Id, 2013);
        var rule2 = CreateTestRule(city.Id, 2013);
        city.AddRule(rule1);

        // Act
        var act = () => city.AddRule(rule2);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*A tax rule for year 2013 already exists*");
    }

    private static TaxRule CreateTestRule(Guid cityId, int year)
    {
        var intervals = new[]
        {
            TollInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), new Money(8)),
        };
        return TaxRule.Create(cityId, year, new Money(60), 60, intervals, [], [], [], []);
    }
}
