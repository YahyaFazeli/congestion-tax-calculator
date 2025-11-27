namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails for user input.
/// Maps to HTTP 400 status code.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Gets the dictionary of validation errors.
    /// Key is the field name, value is an array of error messages for that field.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with structured validation errors.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errors">Dictionary of field names to error messages.</param>
    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a single error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
