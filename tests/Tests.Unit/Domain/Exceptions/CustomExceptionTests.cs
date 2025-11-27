using Domain.Exceptions;

namespace Tests.Unit.Domain.Exceptions;

public class CustomExceptionTests
{
    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new TestDomainException("Test message");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void DomainException_ShouldSetMessage()
    {
        // Arrange
        var message = "Test domain exception message";

        // Act
        var exception = new TestDomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void DomainException_ShouldSetInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");
        var message = "Test domain exception message";

        // Act
        var exception = new TestDomainException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void NotFoundException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new TestNotFoundException("Test message");

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void NotFoundException_ShouldSetMessage()
    {
        // Arrange
        var message = "Test not found exception message";

        // Act
        var exception = new TestNotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void TaxRuleNotFoundException_ShouldInheritFromNotFoundException()
    {
        // Arrange & Act
        var exception = new TaxRuleNotFoundException(Guid.NewGuid(), 2013);

        // Assert
        Assert.IsAssignableFrom<NotFoundException>(exception);
    }

    [Fact]
    public void TaxRuleNotFoundException_ShouldSetCityIdProperty()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;

        // Act
        var exception = new TaxRuleNotFoundException(cityId, year);

        // Assert
        Assert.Equal(cityId, exception.CityId);
    }

    [Fact]
    public void TaxRuleNotFoundException_ShouldSetYearProperty()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var year = 2013;

        // Act
        var exception = new TaxRuleNotFoundException(cityId, year);

        // Assert
        Assert.Equal(year, exception.Year);
    }

    [Fact]
    public void TaxRuleNotFoundException_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var cityId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
        var year = 2013;
        var expectedMessage = $"No tax rule found for city {cityId} and year {year}";

        // Act
        var exception = new TaxRuleNotFoundException(cityId, year);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void CityNotFoundException_ShouldInheritFromNotFoundException()
    {
        // Arrange & Act
        var exception = new CityNotFoundException(Guid.NewGuid());

        // Assert
        Assert.IsAssignableFrom<NotFoundException>(exception);
    }

    [Fact]
    public void CityNotFoundException_ShouldSetCityIdProperty()
    {
        // Arrange
        var cityId = Guid.NewGuid();

        // Act
        var exception = new CityNotFoundException(cityId);

        // Assert
        Assert.Equal(cityId, exception.CityId);
    }

    [Fact]
    public void CityNotFoundException_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var cityId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
        var expectedMessage = $"City with ID {cityId} not found";

        // Act
        var exception = new CityNotFoundException(cityId);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void ValidationException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new ValidationException("Validation failed");

        // Assert
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Fact]
    public void ValidationException_WithMessageOnly_ShouldSetEmptyErrorsDictionary()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var exception = new ValidationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.NotNull(exception.Errors);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void ValidationException_WithErrors_ShouldSetErrorsDictionary()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "CityId", new[] { "City ID is required" } },
            { "Year", new[] { "Year must be between 2000 and 2100" } },
        };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(errors, exception.Errors);
        Assert.Equal(2, exception.Errors.Count);
    }

    [Fact]
    public void ValidationException_WithNullErrors_ShouldSetEmptyErrorsDictionary()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var exception = new ValidationException(message, null!);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.NotNull(exception.Errors);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void ValidationException_WithMultipleErrorsPerField_ShouldStoreAllErrors()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid" } },
        };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        Assert.Equal(2, exception.Errors["Email"].Length);
        Assert.Contains("Email is required", exception.Errors["Email"]);
        Assert.Contains("Email format is invalid", exception.Errors["Email"]);
    }

    // Test helper classes
    private class TestDomainException : DomainException
    {
        public TestDomainException(string message)
            : base(message) { }

        public TestDomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    private class TestNotFoundException : NotFoundException
    {
        public TestNotFoundException(string message)
            : base(message) { }
    }
}
