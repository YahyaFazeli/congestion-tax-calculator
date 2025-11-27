namespace Domain.Specifications;

/// <summary>
/// Specification that determines if two timestamps fall within the same charge window.
/// Used for the "single charge rule" where multiple passages within a time window
/// are charged as one.
/// </summary>
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
