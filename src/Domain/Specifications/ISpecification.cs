namespace Domain.Specifications;

/// <summary>
/// Base interface for the Specification pattern.
/// Encapsulates business rules that can be composed and reused.
/// </summary>
/// <typeparam name="T">The type of object to evaluate</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Determines whether the candidate satisfies this specification.
    /// </summary>
    bool IsSatisfiedBy(T candidate);
}
