using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

public interface ITaxCalculator
{
    Money Calculate(TaxRule rule, Vehicle vehicle, IEnumerable<DateTime> timestamps);
}
