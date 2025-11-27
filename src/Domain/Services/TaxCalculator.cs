using Domain.Entities;
using Domain.Specifications;
using Domain.ValueObjects;

namespace Domain.Services;

public sealed class TaxCalculator : ITaxCalculator
{
    public Money Calculate(TaxRule rule, Vehicle vehicle, IEnumerable<DateTime> timestamps)
    {
        if (rule.IsVehicleExempt(vehicle))
            return Money.Zero;

        var ordered = timestamps.OrderBy(t => t).ToList();
        if (ordered.Count == 0)
            return Money.Zero;

        var dailyGroups = ordered.GroupBy(t => t.Date);

        Money grandTotal = Money.Zero;

        foreach (var dayGroup in dailyGroups)
        {
            Money dailyTotal = Money.Zero;

            var chargeWindows = SingleChargeSpecification.GroupByChargeWindow(
                dayGroup,
                rule.SingleChargeMinutes
            );

            foreach (var window in chargeWindows)
            {
                var maxFee = window.Max(dt =>
                    rule.IsTollFreeDate(dt) ? Money.Zero : rule.GetFeeFor(dt)
                );

                dailyTotal += maxFee;
            }

            var cappedDaily = dailyTotal.Value > rule.DailyMax.Value ? rule.DailyMax : dailyTotal;
            grandTotal += cappedDaily;
        }

        return grandTotal;
    }
}
