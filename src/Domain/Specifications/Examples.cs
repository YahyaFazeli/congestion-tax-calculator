namespace Domain.Specifications;

public static class SpecificationExamples
{
    public class SameDaySpecification : ISpecification<(DateTime first, DateTime second)>
    {
        public bool IsSatisfiedBy((DateTime first, DateTime second) candidate)
        {
            return candidate.first.Date == candidate.second.Date;
        }
    }

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

    public static ISpecification<(DateTime first, DateTime second)> CreateStrictChargeWindow(
        int windowMinutes
    )
    {
        var withinWindow = new SingleChargeWindowSpecification(windowMinutes);
        var sameDay = new SameDaySpecification();

        return withinWindow.And(sameDay);
    }

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
