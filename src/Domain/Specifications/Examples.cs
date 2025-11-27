namespace Domain.Specifications;

/// <summary>
/// Example specifications demonstrating the composability of the Specification pattern.
/// These can be used to extend the tax calculation logic with additional business rules.
/// </summary>
public static class SpecificationExamples
{
    /// <summary>
    /// Specification that checks if two timestamps are on the same day.
    /// Useful for ensuring charge windows don't span across days.
    /// </summary>
    public class SameDaySpecification : ISpecification<(DateTime first, DateTime second)>
    {
        public bool IsSatisfiedBy((DateTime first, DateTime second) candidate)
        {
            return candidate.first.Date == candidate.second.Date;
        }
    }

    /// <summary>
    /// Specification that checks if two timestamps are within business hours.
    /// Example: Only group charges that occur during 6:00-18:00.
    /// </summary>
    public class BusinessHoursSpecification : ISpecification<(DateTime first, DateTime second)>
    {
        private readonly TimeSpan _startTime;
        private readonly TimeSpan _endTime;

        public BusinessHoursSpecification(TimeSpan startTime, TimeSpan endTime)
        {
            _startTime = startTime;
            _endTime = endTime;
        }

        public bool IsSatisfiedBy((DateTime first, DateTime second) candidate)
        {
            var firstTime = candidate.first.TimeOfDay;
            var secondTime = candidate.second.TimeOfDay;

            return firstTime >= _startTime
                && firstTime <= _endTime
                && secondTime >= _startTime
                && secondTime <= _endTime;
        }
    }

    /// <summary>
    /// Example: Create a charge window specification that combines multiple rules.
    /// Timestamps must be within 60 minutes AND on the same day.
    /// </summary>
    public static ISpecification<(DateTime first, DateTime second)> CreateStrictChargeWindow(
        int windowMinutes
    )
    {
        var withinWindow = new SingleChargeWindowSpecification(windowMinutes);
        var sameDay = new SameDaySpecification();

        return withinWindow.And(sameDay);
    }

    /// <summary>
    /// Example: Create a business hours charge window.
    /// Timestamps must be within 60 minutes AND both during business hours (6:00-18:00).
    /// </summary>
    public static ISpecification<(DateTime first, DateTime second)> CreateBusinessHoursChargeWindow(
        int windowMinutes,
        TimeSpan startTime,
        TimeSpan endTime
    )
    {
        var withinWindow = new SingleChargeWindowSpecification(windowMinutes);
        var businessHours = new BusinessHoursSpecification(startTime, endTime);

        return withinWindow.And(businessHours);
    }

    /// <summary>
    /// Example: Create a complex specification with multiple conditions.
    /// Timestamps must be (within 60 minutes AND same day) OR (within 30 minutes during rush hour).
    /// </summary>
    public static ISpecification<(DateTime first, DateTime second)> CreateComplexChargeWindow()
    {
        // Standard rule: 60 minutes, same day
        var standardWindow = new SingleChargeWindowSpecification(60).And(
            new SameDaySpecification()
        );

        // Rush hour rule: 30 minutes during 7:00-9:00 or 16:00-18:00
        var rushHourMorning = new BusinessHoursSpecification(
            new TimeSpan(7, 0, 0),
            new TimeSpan(9, 0, 0)
        );
        var rushHourEvening = new BusinessHoursSpecification(
            new TimeSpan(16, 0, 0),
            new TimeSpan(18, 0, 0)
        );
        var rushHourWindow = new SingleChargeWindowSpecification(30).And(
            rushHourMorning.Or(rushHourEvening)
        );

        return standardWindow.Or(rushHourWindow);
    }
}
