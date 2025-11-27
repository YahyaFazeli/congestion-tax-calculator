namespace Domain.Specifications;

public class AndSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfiedBy(T candidate)
    {
        return _left.IsSatisfiedBy(candidate) && _right.IsSatisfiedBy(candidate);
    }
}

public class OrSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfiedBy(T candidate)
    {
        return _left.IsSatisfiedBy(candidate) || _right.IsSatisfiedBy(candidate);
    }
}

public class NotSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _spec;

    public NotSpecification(ISpecification<T> spec)
    {
        _spec = spec;
    }

    public bool IsSatisfiedBy(T candidate)
    {
        return !_spec.IsSatisfiedBy(candidate);
    }
}

public static class SpecificationExtensions
{
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        return new AndSpecification<T>(left, right);
    }

    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        return new OrSpecification<T>(left, right);
    }

    public static ISpecification<T> Not<T>(this ISpecification<T> spec)
    {
        return new NotSpecification<T>(spec);
    }
}
