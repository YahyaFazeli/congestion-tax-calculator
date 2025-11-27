namespace Domain.Specifications;

/// <summary>
/// Helper class for grouping timestamps by charge windows using the Specification pattern.
/// Maintains backward compatibility with existing code.
/// </summary>
public static class SingleChargeSpecification
{
    /// <summary>
    /// Groups timestamps into charge windows based on the specification.
    /// </summary>
    public static IEnumerable<List<DateTime>> GroupByChargeWindow(
        IEnumerable<DateTime> timestamps,
        int windowMinutes
    )
    {
        var specification = new SingleChargeWindowSpecification(windowMinutes);
        return GroupBySpecification(timestamps, specification);
    }

    /// <summary>
    /// Groups timestamps using a custom specification.
    /// Allows for flexible grouping logic through specification composition.
    /// </summary>
    public static IEnumerable<List<DateTime>> GroupBySpecification(
        IEnumerable<DateTime> timestamps,
        ISpecification<(DateTime first, DateTime second)> specification
    )
    {
        var ordered = timestamps.OrderBy(t => t).ToList();
        if (ordered.Count == 0)
            yield break;

        var currentGroup = new List<DateTime> { ordered[0] };

        for (int i = 1; i < ordered.Count; i++)
        {
            var windowStart = currentGroup[0];

            if (specification.IsSatisfiedBy((windowStart, ordered[i])))
            {
                currentGroup.Add(ordered[i]);
            }
            else
            {
                yield return currentGroup;
                currentGroup = [ordered[i]];
            }
        }

        yield return currentGroup;
    }
}
