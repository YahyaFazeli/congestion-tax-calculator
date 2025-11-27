using Application.Commands.CalculateTax;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Integration.TestHelpers;

namespace Tests.Integration.API;

/// <summary>
/// Integration tests verifying that custom exceptions are thrown correctly by handlers and repositories.
/// The global exception handler (tested separately) will convert these to proper HTTP responses.
/// </summary>
public class ExceptionHandlingTests : IDisposable
{
    private readonly DatabaseFixture _fixture;

    public ExceptionHandlingTests()
    {
        _fixture = new DatabaseFixture();
    }

    [Fact]
    public async Task CalculateTaxHandler_WithNonExistentTaxRule_ThrowsTaxRuleNotFoundException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(
            context,
            NullLogger<TaxRuleRepository>.Instance
        );
        var taxCalculator = new TaxCalculator();
        var handler = new CalculateTaxCommandHandler(
            taxRuleRepository,
            taxCalculator,
            NullLogger<CalculateTaxCommandHandler>.Instance
        );

        var nonExistentCityId = Guid.NewGuid();
        var command = new CalculateTaxCommand(
            nonExistentCityId,
            2099,
            "ABC123",
            VehicleType.Car,
            new[] { DateTime.Now }
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<TaxRuleNotFoundException>();
        exception.Which.CityId.Should().Be(nonExistentCityId);
        exception.Which.Year.Should().Be(2099);
        exception.Which.Message.Should().Contain("No tax rule found");
    }

    [Fact]
    public async Task TaxRuleNotFoundException_ContainsCorrectContext()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var taxRuleRepository = new TaxRuleRepository(
            context,
            NullLogger<TaxRuleRepository>.Instance
        );
        var taxCalculator = new TaxCalculator();
        var handler = new CalculateTaxCommandHandler(
            taxRuleRepository,
            taxCalculator,
            NullLogger<CalculateTaxCommandHandler>.Instance
        );

        var cityId = Guid.NewGuid();
        var year = 2013;
        var command = new CalculateTaxCommand(
            cityId,
            year,
            "ABC123",
            VehicleType.Car,
            new[] { new DateTime(2013, 2, 7, 8, 0, 0) }
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<TaxRuleNotFoundException>();
        exception.Which.CityId.Should().Be(cityId);
        exception.Which.Year.Should().Be(year);
    }

    [Fact]
    public async Task UpdateCityHandler_WithNonExistentCity_ThrowsCityNotFoundException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context, NullLogger<CityRepository>.Instance);
        var handler = new Application.Commands.UpdateCity.UpdateCityCommandHandler(
            cityRepository,
            NullLogger<Application.Commands.UpdateCity.UpdateCityCommandHandler>.Instance
        );

        var nonExistentCityId = Guid.NewGuid();
        var command = new Application.Commands.UpdateCity.UpdateCityCommand(
            nonExistentCityId,
            "NewName"
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<CityNotFoundException>();
        exception.Which.CityId.Should().Be(nonExistentCityId);
        exception.Which.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateCityHandler_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context, NullLogger<CityRepository>.Instance);
        var handler = new Application.Commands.UpdateCity.UpdateCityCommandHandler(
            cityRepository,
            NullLogger<Application.Commands.UpdateCity.UpdateCityCommandHandler>.Instance
        );

        var command = new Application.Commands.UpdateCity.UpdateCityCommand(Guid.NewGuid(), "");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Message.Should().Contain("City name cannot be null or whitespace");
        exception.Which.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task UpdateCityHandler_WithDuplicateName_ThrowsValidationException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context, NullLogger<CityRepository>.Instance);

        // Create two cities
        var city1 = City.Create("Stockholm");
        var city2 = City.Create("Gothenburg");
        await cityRepository.AddAsync(city1);
        await cityRepository.AddAsync(city2);

        var handler = new Application.Commands.UpdateCity.UpdateCityCommandHandler(
            cityRepository,
            NullLogger<Application.Commands.UpdateCity.UpdateCityCommandHandler>.Instance
        );

        // Try to rename city1 to city2's name
        var command = new Application.Commands.UpdateCity.UpdateCityCommand(city1.Id, "Gothenburg");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Message.Should().Contain("already exists");
        exception.Which.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task AddCityTaxRuleHandler_WithNonExistentCity_ThrowsCityNotFoundException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context, NullLogger<CityRepository>.Instance);
        var handler = new Application.Commands.AddCityTaxRule.AddCityTaxRuleCommandHandler(
            cityRepository,
            NullLogger<Application.Commands.AddCityTaxRule.AddCityTaxRuleCommandHandler>.Instance
        );

        var nonExistentCityId = Guid.NewGuid();
        var command = new Application.Commands.AddCityTaxRule.AddCityTaxRuleCommand(
            nonExistentCityId,
            2024,
            60,
            60,
            Array.Empty<Application.Commands.AddCityTaxRule.TollIntervalDto>(),
            Array.Empty<Application.Commands.AddCityTaxRule.TollFreeDateDto>(),
            Array.Empty<Month>(),
            Array.Empty<DayOfWeek>(),
            Array.Empty<VehicleType>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<CityNotFoundException>();
        exception.Which.CityId.Should().Be(nonExistentCityId);
    }

    [Fact]
    public async Task ValidationException_ContainsStructuredErrors()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var cityRepository = new CityRepository(context, NullLogger<CityRepository>.Instance);
        var handler = new Application.Commands.UpdateCity.UpdateCityCommandHandler(
            cityRepository,
            NullLogger<Application.Commands.UpdateCity.UpdateCityCommandHandler>.Instance
        );

        var command = new Application.Commands.UpdateCity.UpdateCityCommand(Guid.NewGuid(), "   ");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().NotBeEmpty();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors["Name"].Should().Contain("City name is required");
    }

    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }
}
