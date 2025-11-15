namespace Domain.Specifications;

public static class SingleChargeSpecification
{
    public static IEnumerable<List<DateTime>> GroupByChargeWindow(
        IEnumerable<DateTime> timestamps,
        int windowMinutes
    )
    {
        var ordered = timestamps.OrderBy(t => t).ToList();
        if (ordered.Count == 0)
            yield break;

        var currentGroup = new List<DateTime> { ordered[0] };

        for (int i = 1; i < ordered.Count; i++)
        {
            var windowStart = currentGroup[0];

            if ((ordered[i] - windowStart).TotalMinutes <= windowMinutes)
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
