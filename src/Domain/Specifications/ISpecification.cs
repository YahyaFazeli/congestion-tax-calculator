namespace Domain.Specifications;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
}
