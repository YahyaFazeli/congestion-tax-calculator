namespace Domain.Exceptions;

/// <summary>
/// Base class for all "not found" exceptions.
/// Automatically maps to HTTP 404 status code in the global exception handler.
/// </summary>
public abstract class NotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected NotFoundException(string message)
        : base(message) { }
}
