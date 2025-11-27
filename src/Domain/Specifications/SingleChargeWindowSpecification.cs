namespace Domain.Specifications;

public class SingleChargeWindowSpecification : ISpecification<(DateTime first, DateTime second)>
{
    private readonly int _windowMinutes;

    public SingleChargeWindowSpecification(int windowMinutes)
    {
        if (windowMinutes <= 0)
            throw new ArgumentException("Window minutes must be positive", nameof(windowMinutes));

        _windowMinutes = windowMinutes;
    }

    public bool IsSatisfiedBy((DateTime first, DateTime second) candidate)
    {
        var (first, second) = candidate;
        var difference = (second - first).TotalMinutes;

        // Use <= to match current behavior (timestamps exactly at window boundary are grouped)
        return difference >= 0 && difference <= _windowMinutes;
    }
}
