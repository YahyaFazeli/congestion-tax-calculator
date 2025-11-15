using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder
            .Property(tr => tr.DailyMax)
            .HasConversion(m => m.Value, v => new Domain.ValueObjects.Money(v));

        builder.HasIndex(tr => new { tr.CityId, tr.Year }).IsUnique();

        builder.Navigation(tr => tr.Intervals).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(tr => tr.TollFreeDates).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(tr => tr.TollFreeMonths).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(tr => tr.TollFreeWeekdays).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(tr => tr.TollFreeVehicles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
