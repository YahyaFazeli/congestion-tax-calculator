namespace Domain.Specifications;

public static class SingleChargeSpecification
{
    public static IEnumerable<List<DateTime>> GroupByChargeWindow(
        IEnumerable<DateTime> timestamps,
        int windowMinutes
    )
    {
        var specification = new SingleChargeWindowSpecification(windowMinutes);
        return GroupBySpecification(timestamps, specification);
    }

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
