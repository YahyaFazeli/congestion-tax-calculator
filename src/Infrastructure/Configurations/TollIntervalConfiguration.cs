using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TollIntervalConfiguration : IEntityTypeConfiguration<TollInterval>
{
    public void Configure(EntityTypeBuilder<TollInterval> builder)
    {
        builder
            .Property(ti => ti.Amount)
            .HasConversion(m => m.Value, v => new Domain.ValueObjects.Money(v));

        builder.HasIndex(ti => new { ti.Start, ti.End });
    }
}
